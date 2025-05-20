using HarmonicArchiveBackend.Data;
using HarmonicArchiveBackend.Models;
using HarmonicArchiveBackend.Repository;

namespace HarmonicArchiveBackend.Services
{
    public class MusicSheetService
    {
        private readonly MusicSheetRepository _repository;

        public MusicSheetService(MusicSheetRepository repository)
        {
            _repository = repository;
        }

        public async Task<(List<MusicSheetDto>, int)> GetAllMusicSheetsAsync(
    string? title = null,
    string? composer = null,
    List<string>? genres = null,
    List<string>? instruments = null,
    string sortBy = "title",
    string sortOrder = "asc",
    int page = 1,
    int limit = 10,
    int userId = 0)
        {
            var musicSheets = await _repository.GetAllAsync(title, composer, genres, instruments, sortBy, sortOrder, page, limit, userId);
            var totalCount = await _repository.GetTotalCountAsync();

            // Map MusicSheet entities to MusicSheetDto
            var musicSheetDtos = musicSheets.Select(ms => new MusicSheetDto
            {
                Id = ms.Id, // Map the Id
                Title = ms.Title.Name,
                Composer = ms.Composer.Name,
                Year = ms.Year,
                Key = ms.Key,
                Genres = ms.MusicSheetGenres.Select(g => g.Genre.Name).ToList(),
                Instruments = ms.MusicSheetInstruments.Select(i => i.Instrument.Name).ToList(),
                MusicFileUrl = ms.MusicFilePath, // File handling is not included in the DTO
                UserId = ms.UserId
            }).ToList();

            return (musicSheetDtos, totalCount);
        }

        public async Task<MusicSheetDto?> GetMusicSheetByIdAsync(int id)
        {
            var musicSheet = await _repository.GetByIdAsync(id);
            if (musicSheet == null)
            {
                return null;
            }

            // Map MusicSheet entity to MusicSheetDto
            return new MusicSheetDto
            {
                Id = musicSheet.Id, // Map the Id
                Title = musicSheet.Title.Name,
                Composer = musicSheet.Composer.Name,
                Year = musicSheet.Year,
                Key = musicSheet.Key,
                Genres = musicSheet.MusicSheetGenres.Select(g => g.Genre.Name).ToList(),
                Instruments = musicSheet.MusicSheetInstruments.Select(i => i.Instrument.Name).ToList(),
                MusicFileUrl = musicSheet.MusicFilePath, // File handling is not included in the DTO
                UserId = musicSheet.UserId
            };
        }

        public async Task AddMusicSheetFromDtoAsync(MusicSheetDto dto)
        {
            // Retrieve existing genres and instruments from the database
            var existingGenres = await _repository.GetGenresByNameAsync(dto.Genres);
            var existingInstruments = await _repository.GetInstrumentsByNameAsync(dto.Instruments);

            // Map the DTO to the MusicSheet entity
            var musicSheet = new MusicSheet
            {
                Key = dto.Key,
                Year = dto.Year,
                MusicFilePath = dto.MusicFileUrl,
                Title = new Title { Name = dto.Title },
                Composer = new Composer { Name = dto.Composer },
                MusicSheetGenres = dto.Genres.Select(genre =>
                {
                    var existingGenre = existingGenres.FirstOrDefault(g => g.Name == genre);
                    return new MusicSheetGenre
                    {
                        Genre = existingGenre ?? new Genre { Name = genre }
                    };
                }).ToList(),
                MusicSheetInstruments = dto.Instruments.Select(instrument =>
                {
                    var existingInstrument = existingInstruments.FirstOrDefault(i => i.Name == instrument);
                    return new MusicSheetInstrument
                    {
                        Instrument = existingInstrument ?? new Instrument { Name = instrument }
                    };
                }).ToList(),
                UserId = dto.UserId
            };

            // Save the MusicSheet entity to the database
            await _repository.AddAsync(musicSheet);
        }

        public async Task AddMusicSheetFromDtoWithoutDuplicateCheckAsync(MusicSheetDto dto)
        {
            // Map the DTO to the MusicSheet entity
            var musicSheet = new MusicSheet
            {
                Key = dto.Key,
                Year = dto.Year,
                MusicFilePath = dto.MusicFileUrl,
                Title = new Title { Name = dto.Title },
                Composer = new Composer { Name = dto.Composer },
                MusicSheetGenres = dto.Genres.Select(genre => new MusicSheetGenre
                {
                    Genre = new Genre { Name = genre }
                }).ToList(),
                MusicSheetInstruments = dto.Instruments.Select(instrument => new MusicSheetInstrument
                {
                    Instrument = new Instrument { Name = instrument }
                }).ToList()
            };

            // Save the MusicSheet entity to the database
            await _repository.AddAsync(musicSheet);
        }

        public async Task UpdateMusicSheetFromDtoAsync(int id, MusicSheetDto dto)
        {
            var existingMusicSheet = await _repository.GetByIdAsync(id);
            if (existingMusicSheet == null)
            {
                throw new KeyNotFoundException("Music sheet not found.");
            }

            // Retrieve existing genres and instruments from the database
            var existingGenres = await _repository.GetGenresByNameAsync(dto.Genres);
            var existingInstruments = await _repository.GetInstrumentsByNameAsync(dto.Instruments);

            // Update properties
            existingMusicSheet.Key = dto.Key;
            existingMusicSheet.Year = dto.Year;
            existingMusicSheet.MusicFilePath = dto.MusicFileUrl ?? existingMusicSheet.MusicFilePath;
            if (existingMusicSheet.Title == null || existingMusicSheet.Title.Name != dto.Title)
            {
                existingMusicSheet.Title = new Title { Name = dto.Title };
            }

            if (existingMusicSheet.Composer == null || existingMusicSheet.Composer.Name != dto.Composer)
            {
                existingMusicSheet.Composer = new Composer { Name = dto.Composer };
            }

            // Update genres
            existingMusicSheet.MusicSheetGenres.Clear();
            foreach (var genre in dto.Genres)
            {
                var existingGenre = existingGenres.FirstOrDefault(g => g.Name == genre);
                existingMusicSheet.MusicSheetGenres.Add(new MusicSheetGenre
                {
                    Genre = existingGenre ?? new Genre { Name = genre }
                });
            }

            // Update instruments
            existingMusicSheet.MusicSheetInstruments.Clear();
            foreach (var instrument in dto.Instruments)
            {
                var existingInstrument = existingInstruments.FirstOrDefault(i => i.Name == instrument);
                existingMusicSheet.MusicSheetInstruments.Add(new MusicSheetInstrument
                {
                    Instrument = existingInstrument ?? new Instrument { Name = instrument }
                });
            }

            // Save changes to the database
            await _repository.UpdateAsync(existingMusicSheet);
        }

        public async Task DeleteMusicSheetAsync(int id)
        {
            var musicSheet = await _repository.GetByIdAsync(id);
            if (musicSheet != null)
            {
                await _repository.DeleteAsync(musicSheet);
            }
        }

        public async Task<string> UploadMusicSheetFileAsync(IFormFile file)
        {
            // Validate the file type
            if (!ValidateFileType(file))
            {
                throw new InvalidOperationException("Invalid file type. Only images, PDFs, and videos are allowed.");
            }

            // Define the directory where files will be stored
            var rootDirectory = AppContext.BaseDirectory; // Gets the root directory of the project
            var uploadDirectory = Path.Combine(rootDirectory, "uploaded");

            // Ensure the directory exists
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Generate a unique file name to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadDirectory, uniqueFileName);

            // Save the file to the specified path
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await file.CopyToAsync(stream);
            }

            // Return the file path
            return filePath;
        }

        private static readonly List<string> AllowedMimeTypes = new()
       {
           "image/jpeg", "image/png", "application/pdf", "video/mp4", "video/mpeg"
       };

        public bool ValidateFileType(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Validate MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                return false;
            }

            return true;
        }

        public async Task<(List<string> Genres, List<string> Instruments)> GetUniqueTagsForUserAsync(int userId)
        {
            var musicSheets = await _repository.GetMusicSheetsByUserIdAsync(userId);

            // Extract unique genres and instruments
            var uniqueGenres = musicSheets
                .SelectMany(ms => ms.MusicSheetGenres.Select(g => g.Genre.Name))
                .Distinct()
                .ToList();

            var uniqueInstruments = musicSheets
                .SelectMany(ms => ms.MusicSheetInstruments.Select(i => i.Instrument.Name))
                .Distinct()
                .ToList();

            return (uniqueGenres, uniqueInstruments);
        }
    }
}

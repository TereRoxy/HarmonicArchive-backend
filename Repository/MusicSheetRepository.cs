using HarmonicArchiveBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HarmonicArchiveBackend.Repository
{
    public class MusicSheetRepository
    {
        private readonly ApplicationDbContext _context;

        public MusicSheetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MusicSheet>> GetAllAsync(
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
            var query = _context.MusicSheets
                .AsNoTracking() // Add AsNoTracking here
                .AsQueryable();

            query = query.Where(ms => ms.UserId == userId);
            

            // Apply filtering
            if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(composer))
            {
                query = query.Where(ms =>
                    (!string.IsNullOrEmpty(title) && ms.Title.Name.Contains(title)) ||
                    (!string.IsNullOrEmpty(composer) && ms.Composer.Name.Contains(composer)));
            }

            if (genres != null && genres.Any())
            {
                query = query.Where(ms => ms.MusicSheetGenres.Any(msg => genres.Contains(msg.Genre.Name)));
            }

            if (instruments != null && instruments.Any())
            {
                query = query.Where(ms => ms.MusicSheetInstruments.Any(msi => instruments.Contains(msi.Instrument.Name)));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(ms => ms.Title.Name)
                    : query.OrderBy(ms => ms.Title.Name),
                "composer" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(ms => ms.Composer.Name)
                    : query.OrderBy(ms => ms.Composer.Name),
                "year" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(ms => ms.Year)
                    : query.OrderBy(ms => ms.Year),
                _ => query.OrderBy(ms => ms.Title.Name) // Default sorting by title
            };

            // Apply pagination
            return await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(ms => new MusicSheet
                {
                    Id = ms.Id,
                    Key = ms.Key,
                    Year = ms.Year,
                    Title = new Title { Name = ms.Title.Name },
                    Composer = new Composer { Name = ms.Composer.Name },
                    MusicSheetGenres = ms.MusicSheetGenres.Select(msg => new MusicSheetGenre
                    {
                        Genre = new Genre { Name = msg.Genre.Name }
                    }).ToList(),
                    MusicSheetInstruments = ms.MusicSheetInstruments.Select(msi => new MusicSheetInstrument
                    {
                        Instrument = new Instrument { Name = msi.Instrument.Name }
                    }).ToList()
                })
                .ToListAsync();

        }

        public async Task<List<MusicSheet>> GetByUserIdAsync(int userId)
        {
            return await _context.MusicSheets
                .Where(ms => ms.UserId == userId)
                .Include(ms => ms.Title)
                .Include(ms => ms.Composer)
                .Include(ms => ms.MusicSheetGenres).ThenInclude(msg => msg.Genre)
                .Include(ms => ms.MusicSheetInstruments).ThenInclude(msi => msi.Instrument)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.MusicSheets.CountAsync();
        }

        public async Task<MusicSheet?> GetByIdAsync(int id)
        {
            return await _context.MusicSheets
                .Include(ms => ms.Title)
                .Include(ms => ms.Composer)
                .Include(ms => ms.MusicSheetGenres).ThenInclude(msg => msg.Genre)
                .Include(ms => ms.MusicSheetInstruments).ThenInclude(msi => msi.Instrument)
                .FirstOrDefaultAsync(ms => ms.Id == id);
        }

        public async Task AddAsync(MusicSheet musicSheet)
        {
            _context.MusicSheets.Add(musicSheet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MusicSheet musicSheet)
        {
            _context.MusicSheets.Update(musicSheet);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MusicSheet musicSheet)
        {
            _context.MusicSheets.Remove(musicSheet);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Genre>> GetGenresByNameAsync(List<string> genreNames)
        {
            return await _context.Genres
                .Where(g => genreNames.Contains(g.Name))
                .ToListAsync();
        }

        public async Task<List<Instrument>> GetInstrumentsByNameAsync(List<string> instrumentNames)
        {
            return await _context.Instruments
                .Where(i => instrumentNames.Contains(i.Name))
                .ToListAsync();
        }

        public async Task<List<MusicSheet>> GetMusicSheetsByUserIdAsync(int userId)
        {
            return await _context.MusicSheets
                .Where(ms => ms.UserId == userId)
                .Include(ms => ms.MusicSheetGenres)
                .ThenInclude(msg => msg.Genre)
                .Include(ms => ms.MusicSheetInstruments)
                .ThenInclude(msi => msi.Instrument)
                .ToListAsync();
        }
    }
}

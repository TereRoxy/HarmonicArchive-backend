using Bogus;
using HarmonicArchiveBackend.Data;
using HarmonicArchiveBackend.Models;
using System.Collections.Generic;

namespace HarmonicArchiveBackend.Services
{
    public static class MusicSheetGenerator
    {
        public static List<MusicSheetDto> GenerateMusicSheets(int count)
        {
            // Define a Faker for the MusicSheetDto
            var musicSheetDtoFaker = new Faker<MusicSheetDto>()
                .RuleFor(dto => dto.Title, f => f.Lorem.Sentence(3)) // Generate a random title
                .RuleFor(dto => dto.Composer, f => f.Name.FullName()) // Generate a random composer name
                .RuleFor(dto => dto.Year, f => f.Date.Past(50).Year) // Generate a random year in the past 50 years
                .RuleFor(dto => dto.Key, f => f.PickRandom(new[] { "C", "G", "Dm", "Am", "F" })) // Generate a random key
                .RuleFor(dto => dto.Genres, f => f.Make(2, () => f.Music.Genre())) // Generate a list of 2 random genres
                .RuleFor(dto => dto.Instruments, f => f.Make(2, () => f.PickRandom(new[] { "Piano", "Guitar", "Violin", "Drums", "Flute" }))) // Generate a list of 2 random instruments
                .RuleFor(dto => dto.MusicFileUrl, f => "string"); // Set MusicFile to null (file uploads are not handled here

            // Generate the specified number of MusicSheetDto objects
            return musicSheetDtoFaker.Generate(count);
        }
    }
}
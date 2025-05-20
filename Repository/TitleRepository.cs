using HarmonicArchiveBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HarmonicArchiveBackend.Repository
{
    public class TitleRepository
    {
        private readonly ApplicationDbContext _context;

        public TitleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Title>> GetAllAsync()
        {
            return await _context.Titles.ToListAsync();
        }

        public async Task<Title?> GetByIdAsync(int id)
        {
            return await _context.Titles.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(Title title)
        {
            _context.Titles.Add(title);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Title title)
        {
            _context.Titles.Update(title);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Title title)
        {
            _context.Titles.Remove(title);
            await _context.SaveChangesAsync();
        }
    }
}

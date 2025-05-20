using HarmonicArchiveBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HarmonicArchiveBackend.Repository
{
    public class ComposerRepository
    {
        private readonly ApplicationDbContext _context;

        public ComposerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Composer>> GetAllAsync()
        {
            return await _context.Composers.ToListAsync();
        }

        public async Task<Composer?> GetByIdAsync(int id)
        {
            return await _context.Composers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Composer composer)
        {
            _context.Composers.Add(composer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Composer composer)
        {
            _context.Composers.Update(composer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Composer composer)
        {
            _context.Composers.Remove(composer);
            await _context.SaveChangesAsync();
        }
    }
}

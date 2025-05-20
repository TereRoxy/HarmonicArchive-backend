using HarmonicArchiveBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HarmonicArchiveBackend.Repository
{
    public class InstrumentRepository
    {
        private readonly ApplicationDbContext _context;

        public InstrumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Instrument>> GetAllAsync()
        {
            return await _context.Instruments.ToListAsync();
        }

        public async Task<Instrument?> GetByIdAsync(int id)
        {
            return await _context.Instruments.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task AddAsync(Instrument instrument)
        {
            _context.Instruments.Add(instrument);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Instrument instrument)
        {
            _context.Instruments.Update(instrument);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Instrument instrument)
        {
            _context.Instruments.Remove(instrument);
            await _context.SaveChangesAsync();
        }
    }
}

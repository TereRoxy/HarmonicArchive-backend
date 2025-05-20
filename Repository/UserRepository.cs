//using HarmonicArchiveBackend.Models;
//using Microsoft.EntityFrameworkCore;

//namespace HarmonicArchiveBackend.Repository
//{
//    public class UserRepository
//    {
//        private readonly ApplicationDbContext _context;

//        public UserRepository(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<User?> GetByUsernameAsync(string username)
//        {
//            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
//        }

//        public async Task AddAsync(User user)
//        {
//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<List<User>> GetAllAsync()
//        {
//            return await _context.Users.ToListAsync();
//        }
//    }
//}

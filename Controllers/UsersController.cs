using HarmonicArchiveBackend.Data;
using HarmonicArchiveBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net; // Add this at the top

namespace HarmonicArchiveBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                return Unauthorized();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok(new { message = "Logged in" });
        }

        [HttpGet("current")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userIdClaim.Value));
            if (user == null)
                return NotFound();

            return Ok(new { userId = user.Id, username = user.Username, email = user.Email });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/Users


        [HttpPost]
        public async Task<ActionResult<object>> CreateUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username || u.Email == user.Email))
                return Conflict("Username or email already exists.");

            // Hash the password before saving
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "normal";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role
            });
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Validate the old password
            if (!string.IsNullOrWhiteSpace(updatedUser.OldPassword) &&
                !BCrypt.Net.BCrypt.Verify(updatedUser.OldPassword, user.Password))
            {
                return BadRequest(new { message = "Old password is incorrect." });
            }

            // Update fields
            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;

            // Update the password if a new password is provided
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Users/5/musicsheets
        [HttpGet("{id}/musicsheets")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserMusicSheets(int id)
        {
            var musicSheets = await _context.MusicSheets
                .Where(ms => ms.UserId == id)
                .Select(ms => new {
                    ms.Id,
                    Title = ms.Title.Name,
                    Composer = ms.Composer.Name,
                    ms.Year,
                    ms.Key,
                    ms.MusicFilePath
                })
                .ToListAsync();

            return Ok(musicSheets);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return Ok(new { message = "Logged out" });
        }
    }
}

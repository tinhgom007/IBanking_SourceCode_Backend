using src.Entities;
using src.Interfaces.IRepositories;
using src.Data;
using Microsoft.EntityFrameworkCore;

namespace src.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateStudent(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> FindUserById(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> FindUserByUserName(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task UpdateFailedLoginCount(Guid userId, bool isCount)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (isCount) { 
                user.FailedLoginCount += 1;
                user.LastFailedLoginAt = DateTime.UtcNow;

                if (user.FailedLoginCount >= 5)
                {
                    user.IsLocked = true;
                }
            }
            else
            {
                user.FailedLoginCount = 0;
                user.LastFailedLoginAt = null;
            }
            await _context.SaveChangesAsync();
        }
    }
}

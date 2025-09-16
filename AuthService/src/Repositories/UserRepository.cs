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

        public Task<User> CreateNewStudent(User user)
        {
            throw new NotImplementedException();
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
    }
}

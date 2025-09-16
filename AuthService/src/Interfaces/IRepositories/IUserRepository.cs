using src.Entities;

namespace src.Interfaces.IRepositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> CreateNewStudent(User user);
        Task<User> FindUserById(Guid userId);
        Task<User> FindUserByUserName(string userName);
    }
}

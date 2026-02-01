using EasyStore.Domain.Models;
using EasyStore.Data.Entities;

namespace EasyStore.Data.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailAlreadyUsed(string email);
}

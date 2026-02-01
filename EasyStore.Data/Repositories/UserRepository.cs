using Microsoft.EntityFrameworkCore;
using EasyStore.Data.Entities;
using EasyStore.Data.Interfaces;

namespace EasyStore.Data.Repositories;
public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    private readonly AppDbContext _context = context;
    public async Task<bool> IsEmailAlreadyUsed(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.IsDeleted == false);
    }
}
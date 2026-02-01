using EasyStore.Common.Requests.Auth;
using EasyStore.Common.Requests.Users;
using EasyStore.Common.Responses.Users;
using EasyStore.Core.Exceptions;
using EasyStore.Core.StaticClasses;
using EasyStore.Data.Entities;
using EasyStore.Data.Interfaces;
using EasyStore.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EasyStore.Domain.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IEnumerable<UserResponse>?> GetAsync()
    {
        IEnumerable<User> users = await userRepository.GetAllAsync();
        return users.Select(user => new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Names = user.Names,
            Phone = user.Phone,
            Role = user.Role
        });
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        User user = await userRepository.GetByIdAsync(id) ?? throw new AppException("User not found.").SetStatusCode(404);
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Names = user.Names,
            Phone = user.Phone,
            Role = user.Role
        };
    }

    public async Task<UserResponse?> CreateCashierAsync(RegisterUserRequest request)
    {
        if (await userRepository.IsEmailAlreadyUsed(request.Email))
        {
            throw new AppException("Email is already in use.").SetStatusCode(409);
        }

        User user = new User
        {
            Email = request.Email,
            Names = request.Names,
            Phone = request.Phone,
            Role = "Cashier"
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        User? createdUser = await userRepository.UpdateAsync(user);

        return new UserResponse
        {
            Id = createdUser!.Id,
            Email = createdUser.Email,
            Names = createdUser.Names,
            Phone = createdUser.Phone,
            Role = createdUser.Role
        };
    }

    public async Task<UserResponse?> UpdateAsync(UpdateUserRequest request)
    {
        User userBeforeUpdate = await userRepository.GetByIdAsync(request.Id) ?? throw new AppException("User not found.").SetStatusCode(404);

        userBeforeUpdate.Email = request.Email;
        userBeforeUpdate.Names = request.Names;
        userBeforeUpdate.Phone = request.Phone;

        User? updatedUser = await userRepository.UpdateAsync(userBeforeUpdate);

        return new UserResponse
        {
            Id = updatedUser!.Id,
            Email = updatedUser.Email,
            Names = updatedUser.Names,
            Phone = updatedUser.Phone,
            Role = updatedUser.Role
        };
    }

    public async Task<bool> DeleteAsync(Guid id) => await userRepository.DeleteAsync(id);

    public async Task<bool> PromoteToAdminAsync(RoleChangeRequest request) => await ChangeRoleAsync(request, Roles.Admin);

    public async Task<bool> DemoteToRegisteredCustomerAsync(RoleChangeRequest request) => await ChangeRoleAsync(request, "Customer");

    private async Task<bool> ChangeRoleAsync(RoleChangeRequest request, string toRole)
    {
        User user = await userRepository.GetByIdAsync(request.UserId) ?? throw new AppException("User not found.").SetStatusCode(404);
        user.Role = toRole;
        await userRepository.UpdateAsync(user);
        return true;
    }
}

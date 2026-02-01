using EasyStore.Common.Requests.Auth;
using EasyStore.Common.Requests.Users;
using EasyStore.Common.Responses.Users;

namespace EasyStore.Domain.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>?> GetAsync();
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<UserResponse?> CreateCashierAsync(RegisterUserRequest request);
    Task<UserResponse?> UpdateAsync(UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> PromoteToAdminAsync(RoleChangeRequest request);
    Task<bool> DemoteToRegisteredCustomerAsync(RoleChangeRequest request);
}

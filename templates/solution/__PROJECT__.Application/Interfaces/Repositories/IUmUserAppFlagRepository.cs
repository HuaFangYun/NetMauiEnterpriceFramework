using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Application.Interfaces.Repositories;

public interface IUmUserAppFlagRepository
{
    Task<um_user_app_flag?> GetByUserIdAsync(string userId);
    Task AddAsync(um_user_app_flag entity);
    Task UpdateAsync(um_user_app_flag entity);
}

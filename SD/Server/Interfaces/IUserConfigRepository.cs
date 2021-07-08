using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface IUserConfigRepository
    {
        Task<UserConfigModel> GetUserConfigs(string userId);
        Task<bool> SetUserConfigs(UserConfigModel userConfigModel);
    }
}

using sd.Api.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class UserLablesService
    {
        private readonly IUserConfigRepository _userConfigRepository;
        public UserLablesService(IUserConfigRepository userConfigRepository)
        {
            _userConfigRepository = userConfigRepository;
        }

        public async Task<IEnumerable<string>> GetUserLabels(string userId)
        {
            var user = await _userConfigRepository.GetUserConfigs(userId);
            return ConvertToList(user.Labels);
        }

        public async Task<bool> SetUserLabels(string userId, string lablesStr)
        {
           var lables= ConvertToList(lablesStr);
            
            foreach (var item in lables)
            {
                lablesStr += "," + item;
            }

            var userConfigs = await _userConfigRepository.GetUserConfigs(userId);

            userConfigs.Labels = lablesStr;
            await _userConfigRepository.SetUserConfigs(userConfigs);
            return true;
        }

        private IEnumerable<string> ConvertToList(string lablesStr)
        {
            List<string> lables = new List<string>();
            if (!string.IsNullOrWhiteSpace(lablesStr))
            {
                string[] lablesArray = lablesStr.Split(",");

                foreach (var lable in lablesArray)
                {
                    if (!string.IsNullOrWhiteSpace(lable) && lable != "null")
                    {
                        if (!lables.Contains(lable))
                        {
                            lables.Add(lable);
                        }
                    }
                }
            }
            return lables;
        }
    }
}

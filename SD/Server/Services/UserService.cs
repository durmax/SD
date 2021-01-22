using System;
using System.Collections.Generic;
using SD.Shared;

namespace sd.Api.Services
{
    public class UserService
    {
        public Dictionary<string, Tuple<string, string>> GetRelationships(UserModel CurrentUser, List<UserModel> users)
        {
            string relation = null;
            Dictionary<string, Tuple<string, string>> res = new Dictionary<string, Tuple<string, string>>();

            if (!string.IsNullOrWhiteSpace(CurrentUser.UserId) && CurrentUser.UserId != "0")
            {
                users.RemoveAll(u => u.UserId == CurrentUser.UserId); //remove Sercher from list
            }

            foreach (var user in users)
            {
                if (CurrentUser.UserId != null)
                {
                    if (user.Friends == null) user.Friends = new List<string>();
                    if (user.FriendRequests == null) user.FriendRequests = new List<string>();

                    if (user.Friends.Contains(CurrentUser.UserId))
                    {
                        relation = "Friends";
                    }

                    else if (user.FriendRequests.Contains(CurrentUser.UserId))
                    {
                        relation = "CrrRequest";
                    }

                    else if (CurrentUser.FriendRequests.Contains(user.UserId))
                    {
                        relation = "UserRequest";
                    }
                }

                var userT = Tuple.Create(user.Name, relation);
                res.Add(user.UserId, userT);

                relation = null;
            }
            return res;
        }
    }
}

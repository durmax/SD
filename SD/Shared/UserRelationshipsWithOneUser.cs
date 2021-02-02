using System;
using System.Collections.Generic;
using System.Text;

namespace SD.Shared
{
    public class UserRelationshipsWithOneUser
    {
        public string UserId;
        public string RelationalUserName;
        public List<Relation> Relations;
        public UserRelationshipsWithOneUser(string userId, string userName, List<Relation> relations)
        {
            UserId = userId;
            RelationalUserName = userName;
            Relations = relations;
        }

    }
}

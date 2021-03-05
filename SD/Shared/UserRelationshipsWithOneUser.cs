using System.Collections.Generic;

namespace SD.Shared
{
    public class UserRelationshipsWithOneUser
    {
        public string UserId { get; set; }
        public string RelationalUserName { get; set; }
        public Relation Relation { get; set; }

        public UserRelationshipsWithOneUser(string userId, string userName, Relation relation)
        {
            UserId = userId;
            RelationalUserName = userName;
            Relation = relation;
        }

    }
}

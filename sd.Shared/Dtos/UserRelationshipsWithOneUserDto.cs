
namespace sd.Shared
{
    public class UserRelationshipsWithOneUserDto
    {
        public string UserId { get; set; }
        public string RelationalUserName { get; set; }
        public Relation Relation { get; set; }
       
        public UserRelationshipsWithOneUserDto()
        {
        }

        public UserRelationshipsWithOneUserDto(string userId, string userName, Relation relation)
        {
            UserId = userId;
            RelationalUserName = userName;
            Relation = relation;
        }

    }
}

using System.ComponentModel.DataAnnotations;

namespace SD.Shared
{
    public class Relationship
    {
        [Required]
        public string UserId1 { get; set; }

        [Required]
        public Reletion Reletion { get; set; }

        [Required]
        public string UserId2 { get; set; }

    }

    public enum Reletion
    {
        Block, 
        Follower,
        FriendRequest,
        Friend,
    }

}

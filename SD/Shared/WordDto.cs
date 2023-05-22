using System;
using System.ComponentModel.DataAnnotations;

namespace SD.Shared
{
    public class WordDto
    {
        //[Required]
        public string WordId { get; set; }
        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string WordLang { get; set; }
        public string ToLang { get; set; }
        public string Explain { get; set; }
        public ShareWith ShareWith { get; set; }
        public bool IsILiked { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int Score { get; set; }
    }

    public enum ShareWith
    {
        OnlyMe = 0,
        Friends = 1,
        Public = 2,
    }
}

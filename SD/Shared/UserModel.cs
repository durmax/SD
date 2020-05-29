using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SD.Shared
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The Name must be at least 3 and at max 20 characters long.", MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string EmailRegCode { get; set; }
        public bool IsEmailReg { get; set; }
        public int AccountLavel { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The Password must be at least 6 characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string FirstLang { get; set; }
        public List<string> KnownLangs { get; set; }
        public List<string> LearnLangs { get; set; }

        public List<string> FriendRequests { get; set; }
        public List<string> Friends { get; set; }

        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }
}

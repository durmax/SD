
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SD.Shared
{
    public class WordModel
    {
        [Key]
        public string WordId { get; set; }
        public string UserId;
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(100)]
        public String Title { get; set; }

        [StringLength(10)]
        public string Language { get; set; }
        [StringLength(10)]
        public string Type { get; set; }
        public string Explain { get; set; }

        [StringLength(20)]
        public string Box { get; set; }
        public string Level { get; set; }

        [StringLength(20)]
        public string Category { get; set; }
    }

}

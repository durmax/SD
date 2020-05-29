using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SD.Shared
{
    public class OtherPageModel
    {
        [Key]
        public string OtherPageId { get; set; }
        public string Host { get; set; }
        public string PageType { get; set; }
        public string PrimLangs { get; set; }
        public string SecLangs { get; set; }
        public int Eval { get; set; }

        public string ApiPath { get; set; }

        public string Note { get; set; }

        public string Pattern { get; set; }
    }

}

using System.Collections.Generic;
using sd.Shared;

namespace sd.Client.Services
{
    public class KnownLangsService
    {
        public string LangsStr { get; set; }

        public List<string> KnownLangs = new();

        public void AddKnownLang(string lang)
        {
            KnownLangs = new List<string>();

            if (!string.IsNullOrEmpty(lang) && !KnownLangs.Contains(lang))
            {
                LangsStr += "," + lang;
            }

            KnownLangs.AddRange(ListStringConverter.ToList(LangsStr));
        }
    }
}

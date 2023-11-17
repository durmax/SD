using System.Collections.Generic;
using SD.Shared;

namespace SD.Client.Services
{
    public class KnownLangsService
    {
        public string LangsStr { get; set; }

        public List<string> KnownLangs = new List<string>();

        public void AddKnownLang(string lang)
        {
            KnownLangs = new List<string>();

            if (!KnownLangs.Contains(lang))
            {
                LangsStr += "," + lang;
            }

            KnownLangs.AddRange(ListStringConverter.ToList(LangsStr));
        }
    }
}

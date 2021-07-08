using System.Collections.Generic;
using SD.Shared;

namespace SD.Client.Services
{
    public class KnownLangsService
    {
        public string LangsStr { get; set; }
        public List<string> KnownLangs = new List<string>();

        public void GetLangsFromLocalAsync( string wLang, string tLang)
        {
            LangsStr += "," + wLang + "," + tLang;

            KnownLangs.AddRange(ListStringConverter.ToList(LangsStr));
        }
    }
}

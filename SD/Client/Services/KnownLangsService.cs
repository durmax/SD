using System.Collections.Generic;

namespace SD.Client.Services
{
    public class KnownLangsService
    {
        public string LangsStr { get; set; }
        public List<string> KnownLangs = new List<string>();

        public void GetLangsFromLocalAsync( string wLang, string tLang)
        {
            LangsStr += "," + wLang + "," + tLang;

            if (!string.IsNullOrWhiteSpace(LangsStr))
            {
                string[] langArray = LangsStr.Split(",");

                foreach (var lan in langArray)
                {
                    if (!string.IsNullOrWhiteSpace(lan) && lan != "null")
                    {
                        if (!KnownLangs.Contains(lan))
                        {
                            KnownLangs.Add(lan);
                        }
                    }
                }
            }
        }
    }
}

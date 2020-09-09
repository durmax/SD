using System.Collections.Generic;

namespace SD.Client.Services
{
    public class KnownLangsService
    {
        public List<string> KnownLangs = new List<string>();

        public void GetLangsFromLocalAsync(string langsStr, string wLang, string tLang)
        {
            langsStr += "," + wLang + "," + tLang;
            if (!string.IsNullOrWhiteSpace(langsStr))
            {
                string[] langArray = langsStr.Split(",");

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

using System.Collections.Generic;

namespace SD.Shared
{
    static public class ListStringConverter
    {
       static public List<string> ToList(string str)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrWhiteSpace(str))
            {
                string[] lablesArray = str.Split(",");

                foreach (var lable in lablesArray)
                {
                    if (!string.IsNullOrWhiteSpace(lable) && lable != "null")
                    {
                        if (!list.Contains(lable))
                        {
                            list.Add(lable);
                        }
                    }
                }
            }
            return list;
        }

        static public string ToString(IEnumerable<string> list)
        {
            string str= "";

            if (list!=null)
            {
                foreach (var item in list)
                {
                    str += "," + item;
                }
            }
            return str;
        }
    }
}

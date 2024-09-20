
namespace SD.Client.Models
{
    public class LinkModel
    {
        public string FLangCode { get; set; }
        public string TLangCode { get; set; }
        public string FLangName { get; set; }
        public string TLangName { get; set; }
        public string Word { get; set; }

        ///////////

        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }
    }
}

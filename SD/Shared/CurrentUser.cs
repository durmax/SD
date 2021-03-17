namespace SD.Shared
{
    public class CurrentUser
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool isAuthenticated { get; set; }
        public bool isAuthTested { get; set; }
    }
}

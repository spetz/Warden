namespace Warden.Web.Framework
{
    public class GeneralSettings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string EncrypterKey { get; set; }
        public string JsonFormatDate { get; set; }
        public string AuthCookieName { get; set; }
        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }
    }
}
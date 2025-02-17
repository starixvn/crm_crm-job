namespace Fanex.CRM.BackgroundJob.Models
{
    public class CustomerInfo
    {
        public int CustId { get; set; }
        public string Username { get; set; }
        public int LanguageId { get; set; }
        public int CountryId { get; set; }
        public int ProvinceId { get; set; }
        public int SiteId { get; set; }
        public int RoleId { get; set; }
        public int TitleId { get; set; }
        public int LicenseeType { get; set; }
    }
}
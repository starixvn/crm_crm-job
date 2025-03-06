namespace CRM.Models.Common;

public class SiteDbConfig
{
    public int SiteDBGroupID { get; set; }
    public string LinkedServerName { get; set; }
    public string ClientAccessName { get; set; }
    public string AGIP { get; set; }
    public string Environment { get; set; }
    public DateTime FDTime { get; set; }
}
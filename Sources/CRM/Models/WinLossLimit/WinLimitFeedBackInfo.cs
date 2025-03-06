namespace CRM.Models.WinLossLimit;

public class WinLimitFeedBackInfo : CustomerWinLimit
{
    public string Currency { get; set; }
    public string UserName { get; set; }
    public string Site { get; set; }
    public string SiteLabel { get; set; }
    public int HeadlineId { get; set; }
    public string HeadlineName { get; set; }
    public string RelatedCustId { get; set; }
    public string ProductName { get; set; }
    public bool IsVip { get; set; }
}
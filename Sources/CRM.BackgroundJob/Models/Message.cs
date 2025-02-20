namespace CRM.BackgroundJob.Models;

public class Message
{
    public int MessageId { get; set; }
    public int RequestId { get; set; }
    public string Content_en { get; set; }
    public string Content_cs { get; set; }
    public string Content_th { get; set; }
    public string Content_vn { get; set; }
    public int VoidingHistoryCollectionId { get; set; }
}
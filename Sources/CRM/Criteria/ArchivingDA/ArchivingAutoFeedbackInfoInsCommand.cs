namespace CRM.Criteria.ArchivingDA;

using Fanex.Data.Repository;

public class ArchivingAutoFeedbackInfoInsCommand : NonQueryCommand
{
    public int BatchSize { get; set; }
    public int MaxLogFBId { get; set; }
    public int BatchSizeAccount { get; set; }
    public string Environment { get; set; }
    public int ServiceID { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public override string GetSettingKey() => "Acc_CRM_ArchivedAccount_AutoFBInfoIns";

    public override bool IsValid() => true;
}
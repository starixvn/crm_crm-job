namespace CRM.Criteria.VoidingSettledTickets;

using Fanex.Data.Repository;

public class VoidingAutoFeedbackInfoInsCommand : NonQueryCommand
{
    public string Message { get; set; }
    public string Environment { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Acc_CRM_VoidingAutoFeedbackInfo_Ins";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(Message);
}
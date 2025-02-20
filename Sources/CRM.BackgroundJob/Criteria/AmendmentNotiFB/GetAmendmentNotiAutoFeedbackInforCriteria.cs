namespace CRM.BackgroundJob.Criteria.AmendmentNotiFB;

using Fanex.Data.Repository;
public class GetAmendmentNotiAutoFeedbackInforCriteria : CriteriaBase
{
    public int BatchSize { get; set; }
    public override string GetSettingKey() => "Acc_CRM_AmendmentNoti_AutoFeedbackInfoSel";

    public override bool IsValid() => true;
}
namespace CRM.Criteria.GenerateResultAmendment;

public class InsertGenResultAmendmentCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string Environment { get; set; }

    public string JSRemotingRef { get; set; }

    public override string GetSettingKey() => "MNC_ResultAmendment_GenNotification";

    public override bool IsValid() => true;
}
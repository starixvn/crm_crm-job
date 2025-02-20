namespace CRM.BackgroundJob.Criteria.AbnormalBet;

using CRM.Models;

public class ImportAbnormalBetCommand : BaseNonQueryCommand
{
    public string Message { get; set; }

    public string VoidedCollectionInfo { get; set; }

    public int MaxVoidingHistoryCollectionId { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public override string GetSettingKey() => "Acc_CRM_AutoFB_AbnormalBets_Import";

    public override bool IsValid() => true;
}
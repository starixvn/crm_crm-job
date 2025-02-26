namespace CRM.Criteria.ChatBot;

using Fanex.Data.Repository;

public class GetImportMessageCriteria : CriteriaBase
{
    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ChatBot_crmMessageImport_Sel";

    public override bool IsValid() => true;
}
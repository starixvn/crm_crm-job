namespace CRM.Criteria.AutoFeedback;

using System;
using Fanex.Data.Repository;

public class ImportVoidingMessageCriteria : CriteriaBase
{
    public string MessageIds { get; set; }
    public int BatchSize { get; set; }
    public string Environment { get; set; }
    public DateTime LastDateUAT { get; set; }

    public override string GetSettingKey() => "Acc_CRM_VoidingMessageIns";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIds) && BatchSize > 0;
}
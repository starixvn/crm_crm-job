namespace CRM.Criteria.AutoFeedback;

using Fanex.Data.Repository;

public class EditSystemSettingCommand : NonQueryCommand
{
    public string Settings { get; set; }
    public string ServiceName { get; set; }

    public override string GetSettingKey() => "Acc_CRM_SystemSettingByServiceUpd";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(Settings);
}
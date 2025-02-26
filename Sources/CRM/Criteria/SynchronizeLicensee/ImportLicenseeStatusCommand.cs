namespace CRM.Criteria.SynchronizeLicensee;

using Fanex.Data.Repository;

public class ImportLicenseeStatusCommand : NonQueryCommand
{
    public string Licensees { get; set; }
    public string Environment { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public override string GetSettingKey() => "Acc_CRM_OICLicenseeIns";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(Licensees);
}
namespace CRM.Models.WinLossLimit;

using AbnormalBet;

public class WinLossSystemSetting : AbnormalBetSystemSetting
{
    public bool IsCredit { get; set; }
    public bool IsDeposit { get; set; }
}
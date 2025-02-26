namespace CRM.Criteria.RefundSettledTicket;

public class ImportRefundSettledTicketCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string Message { get; set; }

    public string MessageContent { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public override string GetSettingKey() => "Acc_CRM_AutoFB_RefundSettledTickets_Import";

    public override bool IsValid() => true;
}
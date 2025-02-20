namespace CRM.BackgroundJob.Criteria.RefundSettledTicket;

using CRM.Models;

public class ImportRefundSettledTicketCommand : BaseNonQueryCommand
{
    public string Message { get; set; }

    public string MessageContent { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public override string GetSettingKey() => "Acc_CRM_AutoFB_RefundSettledTickets_Import";

    public override bool IsValid() => true;
}
namespace CRM.Criteria.ChatBot_ResultAmendment;

using Fanex.Data.Repository;

public class ImportMessageVoidSettledCommand : NonQueryCommand
{
    public string Message { get; set; }
    public override string GetSettingKey() => "Acc_CRM_VodingMessage_Ins";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(Message);
}
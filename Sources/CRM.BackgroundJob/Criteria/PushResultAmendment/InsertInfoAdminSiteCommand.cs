namespace CRM.BackgroundJob.Criteria.PushResultAmendment;

using Fanex.Data.Repository;

public class InsertInfoAdminSiteCommand : NonQueryCommand
{
    public string Men {  get; set; }

    public string Mch { get; set; }

    public bool Member {  get; set; }

    public bool Agent { get; set; }

    public bool Ma {  get; set; }

    public bool Sma { get; set; }

    public int Creator {  get; set; }

    public int NewMessageID {  get; set; }

    public string Remark { get; set; }

    public override string GetSettingKey() => "Dot_Admin_message_new_insert";

    public override bool IsValid() => true;
}
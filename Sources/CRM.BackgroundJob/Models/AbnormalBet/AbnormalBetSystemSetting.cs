namespace CRM.BackgroundJob.Models.AbnormalBet;

using System;

public class AbnormalBetSystemSetting
{
    public string JobStatus { get; set; }
    public string JobStatusUAT { get; set; }
    public int BatchSize { get; set; }
    public bool ConfigUAT { get; set; }
    public bool ConfigPRO { get; set; }
    public bool ServiceFlag { get; set; }
    public long LastLogId { get; set; }
    public string LastLogSId { get; set; }
    public bool IncludeUUSCurrency { get; set; }
    public DateTime LastRuntime { get; set; }
    public DateTime LastDateUAT { get; set; }
    public DateTime LastRuntimeUAT { get; set; }
    public DateTime SubmitDate { get; set; }
    public int BatchSizeGetMsg { get; set; }
    public int MaxMsgTargetId { get; set; }
    public int Seq { get; set; }
    public int MaxVoidingHistoryCollectionId { get; set; }
    public int MaxVoidingHistoryCollectionIdUAT { get; set; }
    public int LastVoidingHistoryCollectionId { get; set; }
    public int LastMsgTargetId { get; set; }
}
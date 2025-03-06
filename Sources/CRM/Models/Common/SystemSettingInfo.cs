namespace CRM.Models.Common;

using System;

public class SystemSettingInfo
{
    public DateTime LastRuntime { get; set; }

    public string JobStatus { get; set; }

    public int BatchSize { get; set; }

    public int LastMsgTargetId { get; set; }

    public bool IncludeUUSCurrency { get; set; }

    public bool ServiceFlag { get; set; }

    public int LastVoidingHistoryCollectionId { get; set; }

    public string LastLogSId { get; set; }
}
namespace CRM.BackgroundJob.Criteria.AutoFeedback;

using System;
using Fanex.Data;
using Fanex.Data.Repository;

public class CreateFeedbackCommand : NonQueryCommand
{
    public int CreatorId { get; set; }
    public int UserId { get; set; }
    public int CustId { get; set; }
    public string Username { get; set; }
    public DateTime FeedbackDate { get; set; }
    public DateTime ResolutionDate { get; set; }
    public string CommunicationId { get; set; }
    public int LanguageId { get; set; }
    public int ResponseId { get; set; }

    [SpParam(SqlParamName = "Product")]
    public string ProductXml { get; set; }

    [SpParam(SqlParamName = "Category")]
    public string CategoryXml { get; set; }

    public int StatusId { get; set; }
    public string StationId { get; set; }
    public int CountryId { get; set; }
    public int ProvinceId { get; set; }
    public int RoleId { get; set; }
    public int SiteId { get; set; }
    public int LicenseeType { get; set; }
    public int TitleId { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public string Solution { get; set; }
    public string RelatedCustId { get; set; }
    public string AutoField { get; set; }
    public string IP { get; set; } = string.Empty;
    public int NoInquiries { get; set; }
    public bool IsOverwrite { get; set; }
    public string MessageId { get; set; }
    public string RequestIds { get; set; }
    public string ListOrigCustIds { get; set; }
    public int VoidingHistoryCollectionId { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Acc_CRM_CustomerFeedbackIns";

    public override bool IsValid() => true;
}
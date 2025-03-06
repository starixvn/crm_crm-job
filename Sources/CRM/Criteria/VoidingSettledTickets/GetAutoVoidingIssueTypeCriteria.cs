namespace CRM.Criteria.VoidingSettledTickets;

using System.Collections.Generic;
using System.Text;
using CRM.Constants;
using Fanex.Data;
using Fanex.Data.Repository;

public class GetAutoVoidingIssueTypeCriteria : CriteriaBase
{
    private const string XmlTemplate = "<r MessageId='{0}' />";

    [SpParam(Ignored = true)]
    public List<int> VoidingMessages { get; set; }

    public string MessageIDs
    {
        get
        {
            if (VoidingMessages is null || VoidingMessages.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            foreach (var messageId in VoidingMessages)
            {
                stringBuilder.AppendFormat(XmlTemplate, messageId);
            }
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            return stringBuilder.ToString();
        }
    }

    public override string GetSettingKey() => "Acc_CRM_AutoVoidingIssueType_Sel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIDs);
}
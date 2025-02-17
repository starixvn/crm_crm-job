
namespace CRM.BackgroundJob.Models.AbnormalBet
{
    using System.Xml.Serialization;

    [XmlRoot("ROOT")]
    public class DuplicateTemplateFeedback
    {
        public string Description { get; set; }
        public string EffectMemberTicketDesc { get; set; }
    }
}

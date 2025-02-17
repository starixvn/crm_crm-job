namespace CRM.BackgroundJob.Models.AmendmentNotiFB
{
    using System.Xml.Serialization;

    [XmlRoot("ROOT")]
    public class AmendmentDescriptionTemplate
    {
        public string Sportsbook { get; set; }
        public string VirtualSports2 { get; set; }
        public string VirtualSports180186 { get; set; }
        public string VirtualSports181To185 { get; set; }
        public string NumberGame { get; set; }
        public string SportsbookOutrightEventType2 { get; set; }
        public string SportsbookOutrightEventType9 { get; set; }
        public string NoResult { get; set; }
    }
}

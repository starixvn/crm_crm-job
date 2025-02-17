namespace Fanex.CRM.BackgroundJob.Models
{
    using System.Collections.Generic;

    public class SettingItem
    {
        public int SettingId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string ExtraInfo { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int ParentId { get; set; }
        public IEnumerable<int> ParentIds { get; set; } = new List<int>();
    }
}
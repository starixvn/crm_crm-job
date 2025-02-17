namespace CRM.BackgroundJob.Core.Models.BotServices
{
    using CRM.BackgroundJob.Core.Enumerables;

    public class NotificationMessage
    {
        public int NotificationId { get; set; }
        public long BotId { get; set; }
        public int ConversationGroupId { get; set; }
        public int ChatTypeId { get; set; }
        public string ConversationId { get; set; }
        public string Content { get; set; }
        public BotChannel ChannelId { get; set; } = BotChannel.Letstalk;

        public ChatType ChatType
        {
            get
            {
                return (ChatType)ChatTypeId;
            }
        }
    }
}
namespace CRM.BackgroundJob.Core.Enumerables
{
    using System.ComponentModel;

    public enum ChatType
    {
        [Description("Group")]
        Group = 1,

        [Description("Personal")]
        Personal = 2
    }
}
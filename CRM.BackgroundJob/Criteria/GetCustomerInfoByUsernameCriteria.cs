namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.Data.Repository;

    public class GetCustomerInfoByUsernameCriteria : CriteriaBase
    {
        public string Username { get; set; }
        public bool IsGuest { get; set; }
        public short UserRole { get; } = BackgroundJobConstants.UserRole;
        public string SiteLabel { get; set; } = string.Empty;

        #pragma warning disable S3052
        public bool IsCheckPermission { get; set; } = false;
        #pragma warning restore S3052

        public override string GetSettingKey() => "Acc_CRM_CustomerSearch";

        public override bool IsValid() => true;
    }
}
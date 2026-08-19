namespace Domain.Common.Settings
{
    public class MongoSettings
    {
        public const string SectionName = "MongoSettings";

        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string AuditLogsCollection { get; set; } = "payment_audit_logs";
    }
}

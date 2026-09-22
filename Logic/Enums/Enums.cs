namespace Logic.Enums
{
    public class Enums
    {
        public static class EResultValue
        {
            public const int Fail = 0;
            public const int Success = 1;
        }

        public static class ERole
        {
            public const long Complainant = 1;
            public const long SapsOfficer = 2;
            public const long StationCommander = 3;
            public const long SystemAdministrator = 4;

            public static string GetName(long roleId) => roleId switch
            {
                Complainant => nameof(Complainant),
                SapsOfficer => nameof(SapsOfficer),
                StationCommander => nameof(StationCommander),
                SystemAdministrator => nameof(SystemAdministrator),
                _ => "Unknown"
            };
        }

        public static class EReportStatus
        {
            public const long Submitted = 1;
            public const long UnderReview = 2;
            public const long Converted = 3;
            public const long Rejected = 4;

            public static string GetName(long statusId) => statusId switch
            {
                Submitted => nameof(Submitted),
                UnderReview => nameof(UnderReview),
                Converted => nameof(Converted),
                Rejected => nameof(Rejected),
                _ => "Unknown"
            };

            // Bootstrap "text-bg-*" utility class for rendering this status as a badge.
            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Submitted => "text-bg-primary",
                UnderReview => "text-bg-warning",
                Converted => "text-bg-success",
                Rejected => "text-bg-danger",
                _ => "text-bg-secondary"
            };
        }

        public static class EDocketStatus
        {
            public const long Open = 1;
            public const long UnderInvestigation = 2;
            public const long Referred = 3;
            public const long Archived = 4;
            public const long Closed = 5;

            public static string GetName(long statusId) => statusId switch
            {
                Open => nameof(Open),
                UnderInvestigation => nameof(UnderInvestigation),
                Referred => nameof(Referred),
                Archived => nameof(Archived),
                Closed => nameof(Closed),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Open => "text-bg-primary",
                UnderInvestigation => "text-bg-warning",
                Referred => "text-bg-info",
                Archived => "text-bg-secondary",
                Closed => "text-bg-success",
                _ => "text-bg-secondary"
            };
        }

        public static class EAffidavitStatus
        {
            public const long Submitted = 1;
            public const long Stamped = 2;
            public const long Rejected = 3;

            public static string GetName(long statusId) => statusId switch
            {
                Submitted => nameof(Submitted),
                Stamped => nameof(Stamped),
                Rejected => nameof(Rejected),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Submitted => "text-bg-primary",
                Stamped => "text-bg-success",
                Rejected => "text-bg-danger",
                _ => "text-bg-secondary"
            };
        }

        public static class EEmergencyAlertStatus
        {
            public const long New = 1;
            public const long Acknowledged = 2;

            public static string GetName(long statusId) => statusId switch
            {
                New => nameof(New),
                Acknowledged => nameof(Acknowledged),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                New => "text-bg-danger",
                Acknowledged => "text-bg-success",
                _ => "text-bg-secondary"
            };
        }

        public static class EEscalationStatus
        {
            public const long Open = 1;
            public const long InProgress = 2;
            public const long Resolved = 3;

            public static string GetName(long statusId) => statusId switch
            {
                Open => nameof(Open),
                InProgress => nameof(InProgress),
                Resolved => nameof(Resolved),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Open => "text-bg-danger",
                InProgress => "text-bg-warning",
                Resolved => "text-bg-success",
                _ => "text-bg-secondary"
            };
        }

        // Keys for the admin's system-wide report exports (Logic/BusinessLogic/AdminReportLogic.cs).
        // String-keyed rather than numeric since these identify a query/export
        // kind, not a persisted status column.
        public static class EReportExportType
        {
            public const string Users = "Users";
            public const string Stations = "Stations";
            public const string PreliminaryReports = "PreliminaryReports";
            public const string Dockets = "Dockets";
            public const string Affidavits = "Affidavits";
            public const string EmergencyAlerts = "EmergencyAlerts";
            public const string Escalations = "Escalations";
            public const string Referrals = "Referrals";
            public const string AuditLogs = "AuditLogs";
            public const string Backups = "Backups";
        }

        // Ids of the seeded rows in the EmailTemplates table (see
        // Database/SQL/MainScript/MainScript.sql) — kept as named constants,
        // same convention as every other *Status class, so callers never
        // hardcode a raw template id.
        public static class EEmailTemplates
        {
            public const long Welcome = 1;
            public const long PasswordReset = 2;
            public const long CaseNotification = 3;
        }

        public static class EBackupStatus
        {
            public const long InProgress = 1;
            public const long Success = 2;
            public const long Failed = 3;

            public static string GetName(long statusId) => statusId switch
            {
                InProgress => nameof(InProgress),
                Success => nameof(Success),
                Failed => nameof(Failed),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                InProgress => "text-bg-warning",
                Success => "text-bg-success",
                Failed => "text-bg-danger",
                _ => "text-bg-secondary"
            };
        }
    }
}

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
            public const long PendingClosure = 6;

            public static string GetName(long statusId) => statusId switch
            {
                Open => nameof(Open),
                UnderInvestigation => nameof(UnderInvestigation),
                Referred => nameof(Referred),
                Archived => nameof(Archived),
                Closed => nameof(Closed),
                PendingClosure => nameof(PendingClosure),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Open => "text-bg-primary",
                UnderInvestigation => "text-bg-warning",
                Referred => "text-bg-info",
                Archived => "text-bg-secondary",
                Closed => "text-bg-success",
                PendingClosure => "text-bg-dark",
                _ => "text-bg-secondary"
            };

            public static bool IsFinal(long statusId) => statusId is Closed or Archived;

            public static IReadOnlyList<long> GetManualTransitions(long fromStatusId, bool isCommander) => fromStatusId switch
            {
                Open => new[] { UnderInvestigation },
                PendingClosure when isCommander => new[] { UnderInvestigation },
                Closed when isCommander => new[] { UnderInvestigation, Archived },
                _ => Array.Empty<long>()
            };

            public static bool RequiresReason(long fromStatusId) => fromStatusId is PendingClosure or Closed;
        }

        public static class EReferralStatus
        {
            public const long Pending = 1;
            public const long Accepted = 2;
            public const long Declined = 3;
            public const long Cancelled = 4;

            public static string GetName(long statusId) => statusId switch
            {
                Pending => nameof(Pending),
                Accepted => nameof(Accepted),
                Declined => nameof(Declined),
                Cancelled => nameof(Cancelled),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Pending => "text-bg-warning",
                Accepted => "text-bg-success",
                Declined => "text-bg-danger",
                _ => "text-bg-secondary"
            };
        }

        public static class EEvidenceReviewStatus
        {
            public const long Accepted = 1;
            public const long Pending = 2;
            public const long Dismissed = 3;

            public static string GetName(long statusId) => statusId switch
            {
                Accepted => nameof(Accepted),
                Pending => "Pending Review",
                Dismissed => nameof(Dismissed),
                _ => "Unknown"
            };

            public static string GetBadgeClass(long statusId) => statusId switch
            {
                Accepted => "text-bg-success",
                Pending => "text-bg-warning",
                Dismissed => "text-bg-secondary",
                _ => "text-bg-secondary"
            };
        }

        public static class EDiaryEntryType
        {
            public const long SceneVisit = 1;
            public const long StatementTaken = 2;
            public const long EvidenceCollected = 3;
            public const long SuspectIdentified = 4;
            public const long Arrest = 5;
            public const long ComplainantContacted = 6;
            public const long CourtAppearance = 7;
            public const long Other = 8;

            public static readonly long[] All = { SceneVisit, StatementTaken, EvidenceCollected, SuspectIdentified, Arrest, ComplainantContacted, CourtAppearance, Other };

            public static string GetName(long typeId) => typeId switch
            {
                SceneVisit => "Scene visited",
                StatementTaken => "Statement taken",
                EvidenceCollected => "Evidence collected",
                SuspectIdentified => "Suspect identified",
                Arrest => "Arrest made",
                ComplainantContacted => "Complainant contacted",
                CourtAppearance => "Court appearance",
                Other => "Other action",
                _ => "Unknown"
            };

            public static string GetIcon(long typeId) => typeId switch
            {
                SceneVisit => "bi-geo-alt",
                StatementTaken => "bi-file-earmark-text",
                EvidenceCollected => "bi-box-seam",
                SuspectIdentified => "bi-person-bounding-box",
                Arrest => "bi-person-lock",
                ComplainantContacted => "bi-telephone",
                CourtAppearance => "bi-bank",
                _ => "bi-journal-text"
            };
        }

        public static class EDocketClosureOutcome
        {
            public const long Solved = 1;
            public const long Withdrawn = 2;
            public const long Unfounded = 3;
            public const long Undetected = 4;
            public const long ReferredToCourt = 5;

            public static readonly long[] All = { Solved, Withdrawn, Unfounded, Undetected, ReferredToCourt };

            public static string GetName(long outcomeId) => outcomeId switch
            {
                Solved => "Solved — suspect identified/charged",
                Withdrawn => "Withdrawn by complainant",
                Unfounded => "Unfounded — no offence committed",
                Undetected => "Undetected — no further leads",
                ReferredToCourt => "Referred to court (NPA)",
                _ => "Unknown"
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

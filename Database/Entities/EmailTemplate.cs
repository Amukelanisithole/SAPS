namespace Database.Entities
{
    // A DB-editable email — an administrator (or the seed script) can change
    // wording/branding without a code change or redeploy. Subject and Body
    // both support {Token} placeholders, filled in by EmailTemplateService
    // at send time.
    public class EmailTemplate
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        // Comma-separated list of the {Token} names this template expects —
        // documentation for whoever edits the row, not parsed at runtime.
        public string Placeholders { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}

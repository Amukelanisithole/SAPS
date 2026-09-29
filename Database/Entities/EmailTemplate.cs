namespace Database.Entities
{
    public class EmailTemplate
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Placeholders { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}

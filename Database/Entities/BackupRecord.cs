namespace Database.Entities
{
    public class BackupRecord
    {
        public long Id { get; set; }
        public DateTime BackupDate { get; set; }
        public long StatusId { get; set; }
        public long PerformedBy { get; set; }
        public bool RestorationTested { get; set; }

        public User? NavPerformedByUser { get; set; }
    }
}

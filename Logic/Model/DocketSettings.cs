namespace Logic.Model
{
    public class DocketSettings
    {
        public int FeedbackOverdueDays { get; set; } = 14;

        public int OverdueAlertRepeatDays { get; set; } = 7;

        public int CheckIntervalMinutes { get; set; } = 60;
    }
}

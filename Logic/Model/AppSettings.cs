namespace Logic.Model
{
    // Lets background/Logic-layer code (e.g. NotificationLogic, which has no
    // HttpContext to build an absolute URL from) construct links back into
    // the site for use inside emails.
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;

        // Absolute path uploaded files (e.g. affidavit identity-verification
        // photos, stamped PDFs) are written under. Resolved to a default
        // inside the web project's content root by Program.cs when left
        // blank, so Logic-layer code never needs to know about hosting.
        public string UploadsRootPath { get; set; } = string.Empty;
    }
}

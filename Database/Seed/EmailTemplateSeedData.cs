using Database.Entities;

namespace Database.Seed
{
    // Mirrors the EmailTemplates rows inserted by Database/SQL/MainScript/MainScript.sql
    // (same ids, same content) — kept as C# here too so Program.cs can bring an
    // already-provisioned database in sync without that script being re-run by hand.
    // Ids 1/2/3 correspond to Logic.Enums.Enums.EEmailTemplates.
    public static class EmailTemplateSeedData
    {
        // Simple stroke-based icons (viewBox 0 0 56 56), same drawing style for
        // all three so the emails read as one family.
        private const string EnvelopeCheckIcon = """
            <rect x="6" y="12" width="44" height="32" rx="5"></rect>
            <path d="M6 17l22 14L50 17"></path>
            <path d="M38 6l-4 4 8 8 10-12"></path>
            """;

        private const string LockIcon = """
            <rect x="14" y="26" width="28" height="22" rx="4"></rect>
            <path d="M20 26v-8a8 8 0 0116 0v8"></path>
            """;

        private const string DocumentIcon = """
            <path d="M16 8h18l10 10v30a2 2 0 01-2 2H16a2 2 0 01-2-2V10a2 2 0 012-2z"></path>
            <path d="M34 8v10h10"></path>
            <path d="M20 30h16"></path>
            <path d="M20 38h16"></path>
            """;

        // Table-based layout (the robust choice for email clients), a
        // gradient icon badge, a large rounded CTA, a plain-text fallback
        // link, and a support contact line — one shared shell so all three
        // emails read as one family, parameterized per-template below.
        private static string BuildEmail(string iconSvg, string title, string bodyHtml) => $$"""
            <!DOCTYPE html>
            <html lang="en" xmlns="http://www.w3.org/1999/xhtml">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width,initial-scale=1" />
              <title>{{title}} - SAPS Crime Pre-Reporting</title>
              <style>
                @media (prefers-color-scheme: dark) {
                  body, .email-bg { background-color:#060b16 !important; color:#e7ebf2 !important; }
                  .card { background:#0f1b30 !important; border-color:rgba(255,255,255,.08) !important; }
                  .muted { color:#93a0b4 !important; }
                }
                a[x-apple-data-detectors] { color:inherit !important; text-decoration:none !important; }
                @media only screen and (max-width:620px) {
                  .wrapper { width:100% !important; }
                  .card { padding:28px 22px !important; }
                  h1 { font-size:22px !important; }
                }
              </style>
            </head>
            <body style="margin:0; padding:0; background:#f5f7fa; font-family:Segoe UI, Arial, sans-serif;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" class="email-bg" style="background-color:#f5f7fa;">
                <tr>
                  <td align="center" style="padding:40px 16px;">
                    <table role="presentation" width="600" cellpadding="0" cellspacing="0" class="wrapper card" style="max-width:600px; width:100%; background:#ffffff; border:1px solid #e5e9f0; border-radius:20px; overflow:hidden; box-shadow:0 4px 12px -2px rgba(0,0,0,0.08);">
                      <tr>
                        <td style="padding:48px 46px 40px;">
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                              <td align="center" style="padding:0 0 20px;">
                                <img src="{LogoUrl}" alt="SAPS" width="76" style="max-width:76px; display:block; height:auto; border-radius:10px;" />
                              </td>
                            </tr>
                          </table>
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                              <td align="center" style="padding:0 0 22px;">
                                <div style="width:88px; height:88px; border-radius:50%; background:linear-gradient(140deg,#1b4b91 0%,#0b2545 100%); text-align:center; line-height:88px; box-shadow:0 4px 14px rgba(27,75,145,0.35);">
                                  <svg width="44" height="44" viewBox="0 0 56 56" fill="none" stroke="#ffffff" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" style="display:inline-block; vertical-align:middle;">
                                    {{iconSvg}}
                                  </svg>
                                </div>
                              </td>
                            </tr>
                          </table>
                          <h1 style="margin:0 0 16px; font-size:26px; text-align:center; font-weight:600; color:#1b4b91;">{{title}}</h1>
                          {{bodyHtml}}
                          <hr style="border:none; height:1px; background:#e5e9f0; margin:28px 0 22px;" />
                          <p class="muted" style="margin:0 0 6px; font-size:13px; color:#64748b; text-align:center;">Need help? Contact us at</p>
                          <p style="margin:0; font-size:13px; text-align:center;"><a href="mailto:support@saps.local" style="color:#1b4b91; text-decoration:none; font-weight:600;">support@saps.local</a></p>
                          <div style="margin-top:32px; text-align:center;">
                            <p class="muted" style="margin:0 0 4px; font-size:11px; color:#93a0b4;">This is an automated message &mdash; please do not reply directly.</p>
                            <p class="muted" style="margin:0; font-size:11px; color:#93a0b4;">SAPS Crime Pre-Reporting System. All rights reserved.</p>
                          </div>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        public static List<EmailTemplate> GetSeedTemplates() =>
        [
            new EmailTemplate
            {
                Name = "Welcome",
                Description = "Sent once, immediately after a new account is registered — doubles as the email-verification message.",
                Subject = "Welcome to SAPS Crime Pre-Reporting, {FullName}!",
                Body = BuildEmail(EnvelopeCheckIcon, "Verify Your Email", """
                    <p style="margin:0 0 14px; font-size:16px; line-height:1.55; color:#3c4657;">Hello {FullName},</p>
                    <p style="margin:0 0 16px; font-size:15px; line-height:1.55; color:#3c4657;">
                      Welcome to <strong style="color:#1b4b91;">SAPS Crime Pre-Reporting</strong>! Please verify your email address to activate full access to your account.
                    </p>
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                      <tr>
                        <td align="center" style="padding:16px 0 28px;">
                          <a href="{VerificationLink}" target="_blank" style="text-decoration:none; display:inline-block; background:linear-gradient(95deg,#1b4b91,#0b2545); color:#ffffff; font-weight:600; padding:16px 36px; font-size:15px; border-radius:14px; box-shadow:0 4px 14px rgba(27,75,145,0.40);">VERIFY EMAIL</a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 12px; font-size:14px; line-height:1.55; color:#3c4657;">If the button does not work, paste this link into your browser:</p>
                    <p style="margin:0 0 20px; font-size:13px; word-break:break-all;"><a href="{VerificationLink}" style="color:#1b4b91; text-decoration:none;">{VerificationLink}</a></p>
                    <p style="margin:0 0 18px; font-size:14px; line-height:1.55; color:#3c4657;">This link expires in 24 hours. Once verified, <a href="{LoginLink}" style="color:#1b4b91;">log in to your account</a> to submit a preliminary report, get an instant timestamped reference number, and track your case securely.</p>
                    <p style="margin:0; font-size:13px; line-height:1.55; color:#64748b;">Not an emergency service &mdash; if anyone is in immediate danger, contact your local emergency services directly.</p>
                    """),
                Placeholders = "{FullName},{LoginLink},{VerificationLink},{LogoUrl}",
                IsActive = true
            },
            new EmailTemplate
            {
                Name = "Password Reset",
                Description = "Sent when a user requests a password reset link.",
                Subject = "Reset your SAPS Crime Pre-Reporting password",
                Body = BuildEmail(LockIcon, "Reset Your Password", """
                    <p style="margin:0 0 14px; font-size:16px; line-height:1.55; color:#3c4657;">Hello {FullName},</p>
                    <p style="margin:0 0 16px; font-size:15px; line-height:1.55; color:#3c4657;">
                      We received a request to reset your password. Click the button below to choose a new one.
                    </p>
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                      <tr>
                        <td align="center" style="padding:16px 0 28px;">
                          <a href="{ResetLink}" target="_blank" style="text-decoration:none; display:inline-block; background:linear-gradient(95deg,#1b4b91,#0b2545); color:#ffffff; font-weight:600; padding:16px 36px; font-size:15px; border-radius:14px; box-shadow:0 4px 14px rgba(27,75,145,0.40);">RESET PASSWORD</a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 12px; font-size:14px; line-height:1.55; color:#3c4657;">If the button does not work, paste this link into your browser:</p>
                    <p style="margin:0 0 20px; font-size:13px; word-break:break-all;"><a href="{ResetLink}" style="color:#1b4b91; text-decoration:none;">{ResetLink}</a></p>
                    <p style="margin:0; font-size:14px; line-height:1.55; color:#3c4657;">This link expires in {ExpiryHours} hour(s). If you did not request this, you can safely ignore this email &mdash; your password will not change.</p>
                    """),
                Placeholders = "{FullName},{ResetLink},{ExpiryHours},{LogoUrl}",
                IsActive = true
            },
            new EmailTemplate
            {
                Name = "Case Notification",
                Description = "Sent for any case event: docket status change, referral, escalation lodged/resolved, or a new case message.",
                Subject = "Update on your SAPS case",
                Body = BuildEmail(DocumentIcon, "Case Update", """
                    <p style="margin:0 0 14px; font-size:16px; line-height:1.55; color:#3c4657;">Hello {FullName},</p>
                    <p style="margin:0 0 16px; font-size:15px; line-height:1.55; color:#3c4657;">{Message}</p>
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                      <tr>
                        <td align="center" style="padding:16px 0 28px;">
                          <a href="{CaseLink}" target="_blank" style="text-decoration:none; display:inline-block; background:linear-gradient(95deg,#1b4b91,#0b2545); color:#ffffff; font-weight:600; padding:16px 36px; font-size:15px; border-radius:14px; box-shadow:0 4px 14px rgba(27,75,145,0.40);">VIEW CASE</a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0 0 12px; font-size:14px; line-height:1.55; color:#3c4657;">If the button does not work, paste this link into your browser:</p>
                    <p style="margin:0 0 20px; font-size:13px; word-break:break-all;"><a href="{CaseLink}" style="color:#1b4b91; text-decoration:none;">{CaseLink}</a></p>
                    <p style="margin:0; font-size:14px; line-height:1.55; color:#3c4657;">Log in to your account for the full case history and to respond if needed.</p>
                    """),
                Placeholders = "{FullName},{Message},{CaseLink},{LogoUrl}",
                IsActive = true
            }
        ];
    }
}

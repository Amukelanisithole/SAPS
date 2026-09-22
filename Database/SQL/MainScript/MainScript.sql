BEGIN TRANSACTION

--Disable ALL Constraints Throughout the Database
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- =============================================
-- Roles  (fixed lookup table matching Logic.Enums.Enums.ERole constants —
-- not an IDENTITY column, the application always inserts/reads by these exact ids)
-- =============================================
INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (1, N'Complainant')
INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (2, N'SapsOfficer')
INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (3, N'StationCommander')
INSERT INTO [dbo].[Roles] ([Id], [Name]) VALUES (4, N'SystemAdministrator')

-- =============================================
-- Users  — seeds the first System Administrator account so there is always
-- a way into the system (self-registration only ever creates Complainants).
-- Default password: ChangeMe123!  — CHANGE THIS before any real deployment.
-- =============================================
SET IDENTITY_INSERT [dbo].[Users] ON
INSERT INTO [dbo].[Users] ([Id], [FullName], [IdNumber], [Email], [PhoneNumber], [PasswordHash], [RoleId], [StationId], [CreatedDate], [LastLogin])
VALUES (1, N'System Administrator', N'0000000000000', N'admin@saps.local', N'0000000000', N'$2a$11$ZsKePs8JaWvuBznKJOAwCO9rO/6iITsilQqP4hvX0pN7qjNCyzYri', 4, NULL, GETUTCDATE(), NULL)
SET IDENTITY_INSERT [dbo].[Users] OFF

-- =============================================
-- EmailTemplates  (aligns with Logic.Enums.Enums.EEmailTemplates constants)
-- Id 1: Welcome            (EEmailTemplates.Welcome = 1) — also the verify-email message
-- Id 2: Password Reset     (EEmailTemplates.PasswordReset = 2)
-- Id 3: Case Notification  (EEmailTemplates.CaseNotification = 3)
-- Subject and Body both support {Token} placeholders, substituted by
-- Logic.BusinessLogic.EmailTemplateService at send time. Body content here
-- must be kept in sync with Database/Seed/EmailTemplateSeedData.cs, which
-- Program.cs uses to upsert these same rows on every app startup.
-- =============================================
SET IDENTITY_INSERT [dbo].[EmailTemplates] ON

INSERT INTO [dbo].[EmailTemplates] ([Id], [Name], [Description], [Subject], [Body], [Placeholders], [IsActive])
VALUES (
    1,
    N'Welcome',
    N'Sent once, immediately after a new account is registered — doubles as the email-verification message.',
    N'Welcome to SAPS Crime Pre-Reporting, {FullName}!',
    N'<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Verify Your Email - SAPS Crime Pre-Reporting</title>
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
                        <rect x="6" y="12" width="44" height="32" rx="5"></rect>
                        <path d="M6 17l22 14L50 17"></path>
                        <path d="M38 6l-4 4 8 8 10-12"></path>
                      </svg>
                    </div>
                  </td>
                </tr>
              </table>
              <h1 style="margin:0 0 16px; font-size:26px; text-align:center; font-weight:600; color:#1b4b91;">Verify Your Email</h1>
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
</html>',
    N'{FullName},{LoginLink},{VerificationLink},{LogoUrl}',
    1
)

INSERT INTO [dbo].[EmailTemplates] ([Id], [Name], [Description], [Subject], [Body], [Placeholders], [IsActive])
VALUES (
    2,
    N'Password Reset',
    N'Sent when a user requests a password reset link.',
    N'Reset your SAPS Crime Pre-Reporting password',
    N'<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Reset Your Password - SAPS Crime Pre-Reporting</title>
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
                        <rect x="14" y="26" width="28" height="22" rx="4"></rect>
                        <path d="M20 26v-8a8 8 0 0116 0v8"></path>
                      </svg>
                    </div>
                  </td>
                </tr>
              </table>
              <h1 style="margin:0 0 16px; font-size:26px; text-align:center; font-weight:600; color:#1b4b91;">Reset Your Password</h1>
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
</html>',
    N'{FullName},{ResetLink},{ExpiryHours},{LogoUrl}',
    1
)

INSERT INTO [dbo].[EmailTemplates] ([Id], [Name], [Description], [Subject], [Body], [Placeholders], [IsActive])
VALUES (
    3,
    N'Case Notification',
    N'Sent for any case event: docket status change, referral, escalation lodged/resolved, or a new case message.',
    N'Update on your SAPS case',
    N'<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Case Update - SAPS Crime Pre-Reporting</title>
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
                        <path d="M16 8h18l10 10v30a2 2 0 01-2 2H16a2 2 0 01-2-2V10a2 2 0 012-2z"></path>
                        <path d="M34 8v10h10"></path>
                        <path d="M20 30h16"></path>
                        <path d="M20 38h16"></path>
                      </svg>
                    </div>
                  </td>
                </tr>
              </table>
              <h1 style="margin:0 0 16px; font-size:26px; text-align:center; font-weight:600; color:#1b4b91;">Case Update</h1>
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
</html>',
    N'{FullName},{Message},{CaseLink},{LogoUrl}',
    1
)

SET IDENTITY_INSERT [dbo].[EmailTemplates] OFF

--Re-Enable All Constraints
EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

--If Everything Is Successful Commit The Transaction
COMMIT TRANSACTION

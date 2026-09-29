using Logic.BusinessLogicInterface;
using Logic.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketPdfService : IDocketPdfService
    {
        public byte[] Generate(DocketPdfModel model)
        {
            var docket = model.Docket;
            var report = docket.NavReport;
            var complainant = report?.NavComplainant;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(9.5f));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(title =>
                            {
                                title.Item().Text("SOUTH AFRICAN POLICE SERVICE").FontSize(14).Bold();
                                title.Item().Text("CASE DOCKET").FontSize(12).SemiBold();
                            });
                            row.ConstantItem(180).AlignRight().Column(refs =>
                            {
                                refs.Item().AlignRight().Text($"Docket #{docket.Id}").Bold();
                                refs.Item().AlignRight().Text($"CAS: {docket.CasNumber ?? "Not assigned"}");
                                refs.Item().AlignRight().Text(EDocketStatus.GetName(docket.StatusId)).FontColor(Colors.Blue.Darken2);
                            });
                        });
                        col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Element(c => Section(c, "Case Summary", body => KeyValues(body, new[]
                        {
                            ("Status", EDocketStatus.GetName(docket.StatusId)),
                            ("Station", docket.NavStation?.StationName ?? "-"),
                            ("Opened", Fmt(docket.OpenedDate)),
                            ("Last Updated", Fmt(docket.LastUpdated)),
                            ("Opened By", docket.NavReviewingOfficer?.FullName ?? "-"),
                            ("Investigating Officer", docket.NavInvestigatingOfficer?.FullName ?? "Unassigned"),
                            ("Report Reference", report?.ReferenceNumber ?? "-"),
                        })));

                        col.Item().Element(c => Section(c, "Complainant", body => KeyValues(body, new[]
                        {
                            ("Full Name", complainant?.FullName ?? "-"),
                            ("ID Number", complainant?.IdNumber ?? "-"),
                            ("Phone", complainant?.PhoneNumber ?? "-"),
                            ("Email", complainant?.Email ?? "-"),
                        })));

                        if (report != null)
                        {
                            col.Item().Element(c => Section(c, "Incident (Preliminary Report)", body =>
                            {
                                body.Item().Element(b => KeyValues(b, new[]
                                {
                                    ("Incident Type", report.IncidentType),
                                    ("Incident Date", Fmt(report.IncidentDate)),
                                    ("Location", report.IncidentLocation),
                                    ("Submitted", Fmt(report.SubmissionTimestamp)),
                                }));
                                body.Item().PaddingTop(4).Text("Description").Bold();
                                body.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(report.IncidentDescription);
                                if (!string.IsNullOrWhiteSpace(report.SupportingInfo))
                                {
                                    body.Item().PaddingTop(4).Text("Supporting Information").Bold();
                                    body.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(report.SupportingInfo);
                                }
                            }));
                        }

                        if (docket.ClosureOutcomeId != null)
                        {
                            col.Item().Element(c => Section(c, docket.StatusId == EDocketStatus.PendingClosure ? "Closure (Awaiting Commander Approval)" : "Closure", body =>
                            {
                                body.Item().Element(b => KeyValues(b, new[]
                                {
                                    ("Outcome", EDocketClosureOutcome.GetName(docket.ClosureOutcomeId.Value)),
                                    ("Closed", docket.ClosedDate != null ? Fmt(docket.ClosedDate.Value) : "Pending approval"),
                                    ("Closed By", docket.NavClosedBy?.FullName ?? "-"),
                                }));
                                body.Item().PaddingTop(4).Text("Closure Notes").Bold();
                                body.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(6).Text(docket.ClosureNotes ?? "-");
                            }));
                        }

                        col.Item().Element(c => Section(c, "Status History", body => Table(body,
                            new[] { "Date", "From", "To", "By", "Reason" },
                            new float[] { 2.2f, 2, 2, 2.2f, 4 },
                            model.StatusHistory.Select(h => new[]
                            {
                                Fmt(h.ChangedDate),
                                h.FromStatusId != null ? EDocketStatus.GetName(h.FromStatusId.Value) : "-",
                                EDocketStatus.GetName(h.ToStatusId),
                                h.NavChangedBy?.FullName ?? "-",
                                h.Reason ?? "-"
                            }))));

                        col.Item().Element(c => Section(c, "Investigation Diary", body => Table(body,
                            new[] { "Date", "Action", "By", "Notes", "Shared" },
                            new float[] { 2.2f, 2.3f, 2.2f, 5.5f, 1.2f },
                            model.Diary.Select(e => new[]
                            {
                                Fmt(e.ActionDate),
                                EDiaryEntryType.GetName(e.EntryTypeId),
                                e.NavAuthor?.FullName ?? "-",
                                e.Notes,
                                e.SharedWithComplainant ? "Yes" : "No"
                            }))));

                        col.Item().Element(c => Section(c, "Referrals", body => Table(body,
                            new[] { "Date", "From", "To", "Reason", "Outcome" },
                            new float[] { 2.2f, 2.3f, 2.3f, 3.5f, 2.5f },
                            model.Referrals.Select(r => new[]
                            {
                                Fmt(r.ReferralDate),
                                r.NavFromStation?.StationName ?? $"Station #{r.FromStationId}",
                                r.NavToStation?.StationName ?? $"Station #{r.ToStationId}",
                                r.Reason,
                                EReferralStatus.GetName(r.StatusId) + (r.ResponseNote != null ? $": {r.ResponseNote}" : "")
                            }))));

                        col.Item().Element(c => Section(c, "Escalations", body => Table(body,
                            new[] { "Date", "Description", "Status" },
                            new float[] { 2.2f, 6, 2 },
                            model.Escalations.Select(e => new[]
                            {
                                Fmt(e.EscalationDate),
                                e.Description,
                                EEscalationStatus.GetName(e.StatusId)
                            }))));

                        col.Item().Element(c => Section(c, "Case Messages", body => Table(body,
                            new[] { "Date", "From", "Message" },
                            new float[] { 2.2f, 2.8f, 6 },
                            model.Comments.Select(m => new[]
                            {
                                Fmt(m.CreatedDate),
                                $"{m.AuthorFullName} ({m.AuthorRoleName})",
                                m.Message
                            }))));

                        var evidence = model.Evidence
                            .Select((item, i) => (Item: item, Label: $"E{i + 1}", Image: TryLoadImage(item.ImageBytes)))
                            .ToList();

                        col.Item().Element(c => Section(c, "Evidence", body =>
                        {
                            if (evidence.Count == 0)
                            {
                                body.Item().Text("None recorded.").Italic().FontColor(Colors.Grey.Darken1);
                                return;
                            }

                            foreach (var entry in evidence)
                            {
                                var e = entry.Item.Evidence;
                                body.Item().ShowEntire().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).Row(row =>
                                {
                                    row.ConstantItem(150).Column(photoCol =>
                                    {
                                        if (entry.Image != null)
                                        {
                                            photoCol.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(2)
                                                .MaxWidth(146).MaxHeight(140).Image(entry.Image).FitArea();
                                        }
                                        else
                                        {
                                            var isPdf = e.ContentType == "application/pdf";
                                            photoCol.Item().Height(70).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4)
                                                .AlignMiddle().AlignCenter().Text(isPdf ? "PDF document\n(held on the system)" : "Image unavailable")
                                                .FontSize(8).FontColor(Colors.Grey.Darken1).AlignCenter();
                                        }
                                        photoCol.Item().PaddingTop(2).Text($"Evidence {entry.Label}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                                    });

                                    row.RelativeItem().PaddingLeft(10).Column(details =>
                                    {
                                        details.Spacing(2);
                                        details.Item().Text(t =>
                                        {
                                            t.Span($"{entry.Label}  ").Bold().FontColor(Colors.Blue.Darken2);
                                            t.Span(e.Description).SemiBold();
                                        });
                                        details.Item().Text($"File: {e.OriginalFileName} ({FileSize(e.FileSizeBytes)})").FontSize(8.5f);
                                        details.Item().Text(e.SubmittedByComplainant
                                                ? $"Submitted by complainant {e.NavUploadedBy?.FullName ?? "-"} on {Fmt(e.UploadedDate)}"
                                                : $"Uploaded by {e.NavUploadedBy?.FullName ?? "-"} on {Fmt(e.UploadedDate)}")
                                            .FontSize(8.5f);
                                        if (e.SubmittedByComplainant && e.ReviewedDate != null)
                                        {
                                            details.Item().Text($"Accepted into evidence by {e.NavReviewedBy?.FullName ?? "-"} on {Fmt(e.ReviewedDate.Value)}")
                                                .FontSize(8.5f);
                                        }
                                        if (entry.Image != null)
                                        {
                                            details.Item().Text("Full-size photograph in the Evidence Photographs annexure.")
                                                .FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
                                        }
                                    });
                                });
                            }
                        }, minSpace: 200));

                        var photos = evidence.Where(p => p.Image != null).ToList();
                        if (photos.Count > 0)
                        {
                            col.Item().PageBreak();
                            col.Item().Text("Annexure — Evidence Photographs").FontSize(12).Bold();
                            foreach (var photo in photos)
                            {
                                col.Item().ShowEntire().Column(photoCol =>
                                {
                                    photoCol.Item().Text($"{photo.Label} — {photo.Item.Evidence.Description}").SemiBold();
                                    photoCol.Item().Text($"{photo.Item.Evidence.OriginalFileName} · uploaded {Fmt(photo.Item.Evidence.UploadedDate)} by {photo.Item.Evidence.NavUploadedBy?.FullName ?? "-"}")
                                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                                    photoCol.Item().PaddingTop(4).MaxHeight(330).AlignCenter().Image(photo.Image!).FitArea();
                                });
                            }
                        }
                    });

                    page.Footer().Column(footer =>
                    {
                        footer.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                        footer.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text($"CONFIDENTIAL — generated by {model.GeneratedByName} on {Fmt(model.GeneratedAt)}")
                                .FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            row.ConstantItem(80).AlignRight().Text(x =>
                            {
                                x.DefaultTextStyle(s => s.FontSize(7.5f).FontColor(Colors.Grey.Darken1));
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void Section(IContainer container, string title, Action<ColumnDescriptor> body, float minSpace = 70)
        {
            container.EnsureSpace(minSpace).Column(col =>
            {
                col.Item().Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(6).Text(title).Bold().FontSize(10.5f);
                col.Item().PaddingTop(5).PaddingHorizontal(2).Column(body);
            });
        }

        private static void KeyValues(IContainer container, IEnumerable<(string Key, string Value)> rows)
        {
            container.Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(130);
                    c.RelativeColumn();
                });
                foreach (var (key, value) in rows)
                {
                    t.Cell().PaddingVertical(2).Text(key).FontColor(Colors.Grey.Darken2);
                    t.Cell().PaddingVertical(2).Text(value);
                }
            });
        }

        private static void KeyValues(ColumnDescriptor col, IEnumerable<(string Key, string Value)> rows) =>
            col.Item().Element(c => KeyValues(c, rows));

        private static void Table(ColumnDescriptor col, string[] headers, float[] widths, IEnumerable<string[]> rows)
        {
            var rowList = rows.ToList();
            if (rowList.Count == 0)
            {
                col.Item().Text("None recorded.").Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    foreach (var w in widths)
                    {
                        c.RelativeColumn(w);
                    }
                });

                t.Header(header =>
                {
                    foreach (var h in headers)
                    {
                        header.Cell().Element(HeaderCell).Text(h).Bold().FontSize(8.5f);
                    }
                });

                foreach (var row in rowList)
                {
                    foreach (var cell in row)
                    {
                        t.Cell().Element(BodyCell).Text(cell).FontSize(8.5f);
                    }
                }
            });
        }

        private static Image? TryLoadImage(byte[]? bytes)
        {
            if (bytes == null) return null;
            try
            {
                return Image.FromBinaryData(bytes);
            }
            catch
            {
                return null;
            }
        }

        private static string Fmt(DateTime utc) => utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

        private static string FileSize(long bytes) =>
            bytes >= 1024 * 1024 ? $"{bytes / (1024.0 * 1024):0.0} MB" : $"{Math.Max(1, bytes / 1024)} KB";

        private static IContainer HeaderCell(IContainer container) =>
            container.Background(Colors.Grey.Lighten4).Padding(3).BorderBottom(1).BorderColor(Colors.Grey.Darken1);

        private static IContainer BodyCell(IContainer container) =>
            container.Padding(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
    }
}

using Database.Entities;
using Logic.BusinessLogicInterface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logic.BusinessLogic
{
    public class AffidavitPdfService : IAffidavitPdfService
    {
        public byte[] Generate(Affidavit affidavit, byte[]? verificationPhotoBytes)
        {
            var stationName = affidavit.NavNearestStation?.StationName ?? "SAPS Station";
            var stationAddress = affidavit.NavNearestStation?.Address ?? "Address unavailable";
            var stampedDate = affidavit.StampedDate ?? DateTime.UtcNow;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("SOUTH AFRICAN POLICE SERVICE").FontSize(14).Bold();
                        col.Item().AlignCenter().Text("AFFIDAVIT / STATEMENT").FontSize(12).SemiBold();
                        col.Item().AlignCenter().Text($"Reference: {affidavit.ReferenceNumber}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text(t =>
                        {
                            t.Span("I, ");
                            t.Span(affidavit.FullName).Bold();
                            t.Span(", ID Number ");
                            t.Span(affidavit.IdNumber).Bold();
                            t.Span(", Age ");
                            t.Span(affidavit.Age.ToString()).Bold();
                        });

                        col.Item().Text($"Working address: {affidavit.WorkingAddress}");
                        col.Item().Text($"Tel: (w) {affidavit.TelWork ?? "-"}   (h) {affidavit.TelHome ?? "-"}   (cell) {affidavit.TelCell}");

                        col.Item().PaddingTop(6).Text("Declare under oath / confirm as follows:").Bold();
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Text(affidavit.DeclarationText);

                        col.Item().Text(affidavit.ObjectionToOath
                            ? "I am familiar with and understand the contents of this declaration. I have an objection to taking the prescribed oath."
                            : "I am familiar with and understand the contents of this declaration. I have no objection to taking the prescribed oath. I consider the prescribed oath as binding on my conscience.");

                        col.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text($"Place: {stationName}");
                            row.RelativeItem().Text($"Date: {affidavit.SubmissionTimestamp:yyyy-MM-dd}");
                            row.RelativeItem().Text($"Time: {affidavit.SubmissionTimestamp:HH:mm}");
                        });

                        col.Item().Text(t =>
                        {
                            t.Span("Signature: ").Bold();
                            t.Span($"Submitted electronically by {affidavit.FullName} via the SAPS Crime Pre-Reporting System — identity confirmed by ID number and a live photo capture at submission.");
                        });

                        if (verificationPhotoBytes != null)
                        {
                            col.Item().PaddingTop(4).Column(photoCol =>
                            {
                                photoCol.Item().Text("Identity verification photo (captured at submission):").FontSize(8).FontColor(Colors.Grey.Darken1);
                                photoCol.Item().MaxWidth(140).MaxHeight(140).Image(verificationPhotoBytes).FitArea();
                            });
                        }

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        col.Item().Text("I certify that the above statement was taken from the deponent named above and that the deponent acknowledged that he/she knows and understands the contents of the statement. The statement was sworn to/affirmed before me and the deponent's identity was confirmed electronically in my presence via the SAPS Crime Pre-Reporting System.");

                        col.Item().Row(row =>
                        {
                            row.RelativeItem(2).Text($"At: {stationName}, {stationAddress}");
                            row.RelativeItem().Text($"on {stampedDate:yyyy-MM-dd}");
                            row.RelativeItem().Text($"at {stampedDate:HH:mm}");
                        });

                        col.Item().PaddingTop(14).Row(row =>
                        {
                            row.RelativeItem().Column(officerCol =>
                            {
                                officerCol.Item().Text("Commissioner of Oaths").Italic().FontSize(9).FontColor(Colors.Grey.Darken1);
                                officerCol.Item().Text($"Force Number: {affidavit.StampedByOfficerForceNumber ?? "-"}");
                                officerCol.Item().Text($"Rank: {affidavit.StampedByOfficerRank ?? "-"}");
                                officerCol.Item().Text($"Name: {affidavit.StampedByOfficerName ?? "-"}");
                            });

                            row.ConstantItem(170).Border(2).BorderColor(Colors.Blue.Darken2).Padding(10).Column(stampCol =>
                            {
                                stampCol.Item().AlignCenter().Text("SAPS").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                                stampCol.Item().AlignCenter().Text(stationName).FontSize(9).Bold();
                                stampCol.Item().AlignCenter().Text("ELECTRONICALLY CERTIFIED").FontSize(8).FontColor(Colors.Blue.Darken2);
                                stampCol.Item().AlignCenter().Text(affidavit.ReferenceNumber).FontSize(8);
                                stampCol.Item().AlignCenter().Text($"{stampedDate:yyyy-MM-dd HH:mm}").FontSize(8);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated by the SAPS Crime Pre-Reporting System — page ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}

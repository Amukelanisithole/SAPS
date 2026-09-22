using Logic.BusinessLogicInterface;
using Logic.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logic.BusinessLogic
{
    // Renders any ReportTableDTO as a landscape, paginated table — one
    // generic layout for every report kind, since they only differ in
    // column count/content, not structure. QuestPDF.Settings.License is set
    // once at startup (see Program.cs).
    public class PdfExportService : IPdfExportService
    {
        public byte[] Generate(ReportTableDTO table)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("SAPS Crime Pre-Reporting System").FontSize(14).Bold();
                        col.Item().Text($"{table.Title} — generated {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Table(t =>
                    {
                        t.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in table.Headers)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        t.Header(header =>
                        {
                            foreach (var h in table.Headers)
                            {
                                header.Cell().Element(HeaderCell).Text(h).Bold();
                            }
                        });

                        foreach (var row in table.Rows)
                        {
                            foreach (var cell in row)
                            {
                                t.Cell().Element(BodyCell).Text(cell);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer HeaderCell(IContainer container) =>
            container.Background(Colors.Grey.Lighten3).Padding(4).BorderBottom(1).BorderColor(Colors.Grey.Darken1);

        private static IContainer BodyCell(IContainer container) =>
            container.Padding(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
    }
}

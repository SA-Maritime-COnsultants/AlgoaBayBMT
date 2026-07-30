using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using AlgoaBayBMT.Emergency.OilSpill.DTOs;
using AlgoaBayBMT.Emergency.OilSpill.Models;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;

namespace AlgoaBayBMT.Emergency.OilSpill.Services
{
    public interface IOilSpillExportService
    {
        /// <summary>Generates a SITREP PDF for the run, optionally embedding a captured map image.</summary>
        OilSpillExportFile GenerateSitrepPdf(OilSpillRunSummary summary, byte[]? mapSnapshotPng = null);

        /// <summary>
        /// Generates a SITREP PDF that also embeds the linked IMS/ICS forms (ICS-209 status summary
        /// plus any linked ICS-201 / ICS-214).
        /// </summary>
        OilSpillExportFile GenerateSitrepPdf(
            OilSpillRunSummary summary,
            IReadOnlyList<IncidentForm> linkedForms,
            byte[]? mapSnapshotPng = null);

        /// <summary>Generates a concise run-summary PDF.</summary>
        OilSpillExportFile GenerateRunSummaryPdf(OilSpillRunSummary summary);

        /// <summary>Generates a run-summary DOCX document.</summary>
        OilSpillExportFile GenerateRunSummaryDocx(OilSpillRunSummary summary);

        /// <summary>Generates a standalone PDF for a single IMS/ICS form.</summary>
        OilSpillExportFile GenerateFormPdf(IncidentForm form);

        /// <summary>
        /// Generates a single consolidated ICS-214 Activity Log PDF for an operational period from
        /// the individual ICS-214 entry records (one record per entry).
        /// </summary>
        OilSpillExportFile GenerateConsolidatedActivityLogPdf(
            string incidentName,
            int operationalPeriodId,
            IReadOnlyList<IncidentForm> activityEntries);

        /// <summary>
        /// Bundles the SITREP PDF, every individual ICS form PDF, and the run summary PDF into a
        /// single ZIP archive ("Export All").
        /// </summary>
        OilSpillExportFile GenerateIncidentPackageZip(
            OilSpillRunSummary summary,
            IReadOnlyList<IncidentForm> forms,
            byte[]? mapSnapshotPng = null);
    }

    /// <summary>
    /// Produces incident SITREP and model-run summary documents using Syncfusion PDF / DocIO.
    /// </summary>
    public sealed class OilSpillExportService : IOilSpillExportService
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public OilSpillExportFile GenerateSitrepPdf(OilSpillRunSummary summary, byte[]? mapSnapshotPng = null)
            => GenerateSitrepPdf(summary, Array.Empty<IncidentForm>(), mapSnapshotPng);

        public OilSpillExportFile GenerateSitrepPdf(
            OilSpillRunSummary summary,
            IReadOnlyList<IncidentForm> linkedForms,
            byte[]? mapSnapshotPng = null)
        {
            using var document = new PdfDocument();
            var page = document.Pages.Add();
            var graphics = page.Graphics;
            var pageWidth = page.GetClientSize().Width;

            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
            var headingFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);

            var navy = new PdfSolidBrush(new PdfColor(13, 27, 62));
            var teal = new PdfSolidBrush(new PdfColor(0, 128, 128));
            var black = PdfBrushes.Black;

            float y = 0f;

            // --- Header band ---
            graphics.DrawRectangle(navy, new RectangleF(0, 0, pageWidth, 50));
            graphics.DrawString("OIL SPILL SITUATION REPORT (SITREP)",
                titleFont, PdfBrushes.White, new PointF(15, 14));
            y = 65f;

            graphics.DrawString($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC",
                bodyFont, black, new PointF(15, y));
            y += 22f;

            // --- Incident details ---
            y = DrawHeading(graphics, "Incident Details", headingFont, teal, y);
            var incidentGrid = new PdfGrid();
            incidentGrid.Columns.Add(2);
            AddRow(incidentGrid, "Spill name", summary.Spill.SpillName);
            AddRow(incidentGrid, "Product type", summary.Spill.ProductType.ToString());
            AddRow(incidentGrid, "Source type", summary.Spill.SourceType.ToString());
            AddRow(incidentGrid, "Status", summary.Spill.Status.ToString());
            AddRow(incidentGrid, "Estimated volume (m3)", summary.Spill.EstimatedVolume.ToString("N1", Inv));
            AddRow(incidentGrid, "Spill start", summary.Spill.SpillStartTime.ToString("dd MMM yyyy HH:mm", Inv));
            AddRow(incidentGrid, "Origin (lat, lon)",
                $"{summary.Spill.Latitude.ToString("F4", Inv)}, {summary.Spill.Longitude.ToString("F4", Inv)}");
            y = DrawGrid(incidentGrid, page, 15, y) + 12f;

            // --- Model run summary ---
            y = DrawHeading(graphics, "Model Run Summary", headingFont, teal, y);
            var runGrid = new PdfGrid();
            runGrid.Columns.Add(2);
            AddRow(runGrid, "Run name", summary.Run.RunName);
            AddRow(runGrid, "Start time", summary.Run.StartTime.ToString("dd MMM yyyy HH:mm", Inv));
            AddRow(runGrid, "Duration (h)", summary.Run.DurationHours.ToString("N1", Inv));
            AddRow(runGrid, "Time step (min)", summary.Run.TimeStepMinutes.ToString(Inv));
            AddRow(runGrid, "Wind", $"{summary.Run.WindSpeed.ToString("N1", Inv)} m/s @ {summary.Run.WindDirection.ToString("N0", Inv)} deg");
            AddRow(runGrid, "Current", $"{summary.Run.CurrentSpeed.ToString("N1", Inv)} m/s @ {summary.Run.CurrentDirection.ToString("N0", Inv)} deg");
            AddRow(runGrid, "Tide state", summary.Run.TideState.ToString());
            y = DrawGrid(runGrid, page, 15, y) + 12f;

            // --- Key results ---
            y = DrawHeading(graphics, "Key Results", headingFont, teal, y);
            var resultGrid = new PdfGrid();
            resultGrid.Columns.Add(2);
            AddRow(resultGrid, "Max slick area (km2)", summary.MaxAreaSqKm.ToString("N3", Inv));
            AddRow(resultGrid, "Total drift distance (km)", summary.TotalDriftDistanceKm.ToString("N2", Inv));
            AddRow(resultGrid, "Shoreline impact",
                summary.HasShorelineImpact
                    ? $"YES - {summary.ShorelineImpactTime:dd MMM yyyy HH:mm}"
                    : "No impact predicted");
            y = DrawGrid(resultGrid, page, 15, y) + 12f;

            // --- Response measure timeline ---
            y = DrawHeading(graphics, "Response Measure Timeline", headingFont, teal, y);
            if (summary.Actions.Count == 0)
            {
                graphics.DrawString("No response measures recorded.", bodyFont, black, new PointF(15, y));
                y += 18f;
            }
            else
            {
                var actionGrid = new PdfGrid();
                actionGrid.Columns.Add(4);
                var header = actionGrid.Rows.Add();
                header.Cells[0].Value = "Type";
                header.Cells[1].Value = "Start";
                header.Cells[2].Value = "End";
                header.Cells[3].Value = "Description";
                foreach (var action in summary.Actions.OrderBy(a => a.StartTime))
                {
                    var row = actionGrid.Rows.Add();
                    row.Cells[0].Value = action.ActionType.ToString();
                    row.Cells[1].Value = action.StartTime.ToString("dd MMM HH:mm", Inv);
                    row.Cells[2].Value = action.EndTime?.ToString("dd MMM HH:mm", Inv) ?? "Ongoing";
                    row.Cells[3].Value = action.Description;
                }
                ApplyGridStyle(actionGrid);
                y = DrawGrid(actionGrid, page, 15, y) + 12f;
            }

            // --- Map snapshot ---
            if (mapSnapshotPng is { Length: > 0 })
            {
                if (y > page.GetClientSize().Height - 220)
                {
                    page = document.Pages.Add();
                    graphics = page.Graphics;
                    y = 15f;
                }

                y = DrawHeading(graphics, "Map Snapshot", headingFont, teal, y);
                using var imageStream = new MemoryStream(mapSnapshotPng);
                var image = new PdfBitmap(imageStream);
                var imgWidth = page.GetClientSize().Width - 30;
                var imgHeight = imgWidth * image.Height / image.Width;
                graphics.DrawImage(image, new RectangleF(15, y, imgWidth, imgHeight));
                y += imgHeight + 12f;
            }

            // --- Linked IMS/ICS forms ---
            foreach (var form in linkedForms.OrderBy(f => f.FormType))
            {
                if (y > page.GetClientSize().Height - 160)
                {
                    page = document.Pages.Add();
                    graphics = page.Graphics;
                    y = 15f;
                }

                y = DrawHeading(graphics, $"{FormTitle(form.FormType)} (linked)", headingFont, teal, y);
                var formGrid = BuildFormGrid(form);
                ApplyGridStyle(formGrid);
                y = DrawGrid(formGrid, page, 15, y) + 12f;
            }

            using var output = new MemoryStream();
            document.Save(output);
            var fileName = $"SITREP_{Sanitize(summary.Spill.SpillName)}_{summary.Run.Id}.pdf";
            return new OilSpillExportFile(fileName, "application/pdf", output.ToArray());
        }

        public OilSpillExportFile GenerateRunSummaryPdf(OilSpillRunSummary summary)
        {
            using var document = new PdfDocument();
            var page = document.Pages.Add();
            var graphics = page.Graphics;
            var pageWidth = page.GetClientSize().Width;

            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
            var headingFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);

            var navy = new PdfSolidBrush(new PdfColor(13, 27, 62));
            var teal = new PdfSolidBrush(new PdfColor(0, 128, 128));

            graphics.DrawRectangle(navy, new RectangleF(0, 0, pageWidth, 45));
            graphics.DrawString("MODEL RUN SUMMARY", titleFont, PdfBrushes.White, new PointF(15, 13));
            float y = 60f;

            y = DrawHeading(graphics, "Run Metadata", headingFont, teal, y);
            var metaGrid = new PdfGrid();
            metaGrid.Columns.Add(2);
            AddRow(metaGrid, "Spill", summary.Spill.SpillName);
            AddRow(metaGrid, "Product", summary.Spill.ProductType.ToString());
            AddRow(metaGrid, "Run name", summary.Run.RunName);
            AddRow(metaGrid, "Created", summary.Run.CreatedAt.ToString("dd MMM yyyy HH:mm", Inv));
            y = DrawGrid(metaGrid, page, 15, y) + 12f;

            y = DrawHeading(graphics, "Parameters", headingFont, teal, y);
            var paramGrid = new PdfGrid();
            paramGrid.Columns.Add(2);
            AddRow(paramGrid, "Duration (h)", summary.Run.DurationHours.ToString("N1", Inv));
            AddRow(paramGrid, "Time step (min)", summary.Run.TimeStepMinutes.ToString(Inv));
            AddRow(paramGrid, "Wind", $"{summary.Run.WindSpeed.ToString("N1", Inv)} m/s @ {summary.Run.WindDirection.ToString("N0", Inv)} deg");
            AddRow(paramGrid, "Current", $"{summary.Run.CurrentSpeed.ToString("N1", Inv)} m/s @ {summary.Run.CurrentDirection.ToString("N0", Inv)} deg");
            AddRow(paramGrid, "Tide", summary.Run.TideState.ToString());
            y = DrawGrid(paramGrid, page, 15, y) + 12f;

            y = DrawHeading(graphics, "Key Results", headingFont, teal, y);
            var resultGrid = new PdfGrid();
            resultGrid.Columns.Add(2);
            AddRow(resultGrid, "Max slick area (km2)", summary.MaxAreaSqKm.ToString("N3", Inv));
            AddRow(resultGrid, "Total drift distance (km)", summary.TotalDriftDistanceKm.ToString("N2", Inv));
            AddRow(resultGrid, "Shoreline impact",
                summary.HasShorelineImpact
                    ? $"YES - {summary.ShorelineImpactTime:dd MMM yyyy HH:mm}"
                    : "No impact predicted");
            y = DrawGrid(resultGrid, page, 15, y) + 12f;

            y = DrawHeading(graphics, "Response Effectiveness Notes", headingFont, teal, y);
            var notesFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            graphics.DrawString(BuildEffectivenessNotes(summary), notesFont, PdfBrushes.Black,
                new RectangleF(15, y, pageWidth - 30, 120));

            using var output = new MemoryStream();
            document.Save(output);
            var fileName = $"RunSummary_{Sanitize(summary.Spill.SpillName)}_{summary.Run.Id}.pdf";
            return new OilSpillExportFile(fileName, "application/pdf", output.ToArray());
        }

        public OilSpillExportFile GenerateRunSummaryDocx(OilSpillRunSummary summary)
        {
            using var document = new WordDocument();
            var section = document.AddSection();
            section.PageSetup.Margins.All = 54f;

            var title = section.AddParagraph();
            title.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            var titleText = title.AppendText("Model Run Summary");
            titleText.CharacterFormat.FontSize = 18;
            titleText.CharacterFormat.Bold = true;
            titleText.CharacterFormat.TextColor = Color.FromArgb(13, 27, 62);

            AddDocHeading(section, "Run Metadata");
            AddDocLine(section, "Spill", summary.Spill.SpillName);
            AddDocLine(section, "Product", summary.Spill.ProductType.ToString());
            AddDocLine(section, "Run name", summary.Run.RunName);
            AddDocLine(section, "Created", summary.Run.CreatedAt.ToString("dd MMM yyyy HH:mm", Inv));

            AddDocHeading(section, "Parameters");
            AddDocLine(section, "Duration (h)", summary.Run.DurationHours.ToString("N1", Inv));
            AddDocLine(section, "Time step (min)", summary.Run.TimeStepMinutes.ToString(Inv));
            AddDocLine(section, "Wind", $"{summary.Run.WindSpeed.ToString("N1", Inv)} m/s @ {summary.Run.WindDirection.ToString("N0", Inv)} deg");
            AddDocLine(section, "Current", $"{summary.Run.CurrentSpeed.ToString("N1", Inv)} m/s @ {summary.Run.CurrentDirection.ToString("N0", Inv)} deg");
            AddDocLine(section, "Tide", summary.Run.TideState.ToString());

            AddDocHeading(section, "Key Results");
            AddDocLine(section, "Max slick area (km2)", summary.MaxAreaSqKm.ToString("N3", Inv));
            AddDocLine(section, "Total drift distance (km)", summary.TotalDriftDistanceKm.ToString("N2", Inv));
            AddDocLine(section, "Shoreline impact",
                summary.HasShorelineImpact
                    ? $"YES - {summary.ShorelineImpactTime:dd MMM yyyy HH:mm}"
                    : "No impact predicted");

            AddDocHeading(section, "Response Effectiveness Notes");
            section.AddParagraph().AppendText(BuildEffectivenessNotes(summary));

            using var output = new MemoryStream();
            document.Save(output, FormatType.Docx);
            var fileName = $"RunSummary_{Sanitize(summary.Spill.SpillName)}_{summary.Run.Id}.docx";
            return new OilSpillExportFile(
                fileName,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                output.ToArray());
        }

        public OilSpillExportFile GenerateFormPdf(IncidentForm form)
        {
            using var document = new PdfDocument();
            var page = document.Pages.Add();
            var graphics = page.Graphics;
            var pageWidth = page.GetClientSize().Width;

            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
            var navy = new PdfSolidBrush(new PdfColor(13, 27, 62));

            graphics.DrawRectangle(navy, new RectangleF(0, 0, pageWidth, 45));
            graphics.DrawString(FormTitle(form.FormType).ToUpperInvariant(),
                titleFont, PdfBrushes.White, new PointF(15, 13));
            float y = 60f;

            var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            graphics.DrawString(
                $"Status: {form.Status} | Linked to SITREP: {(form.IsLinkedToSitrep ? "Yes" : "No")} | " +
                $"Updated: {(form.LastUpdatedAt ?? form.CreatedAt):dd MMM yyyy HH:mm} UTC",
                bodyFont, PdfBrushes.Black, new PointF(15, y));
            y += 22f;

            var grid = BuildFormGrid(form);
            ApplyGridStyle(grid);
            DrawGrid(grid, page, 15, y);

            using var output = new MemoryStream();
            document.Save(output);
            var fileName = $"{form.FormType}_{form.Id}.pdf";
            return new OilSpillExportFile(fileName, "application/pdf", output.ToArray());
        }

        public OilSpillExportFile GenerateConsolidatedActivityLogPdf(
            string incidentName,
            int operationalPeriodId,
            IReadOnlyList<IncidentForm> activityEntries)
        {
            using var document = new PdfDocument();
            var page = document.Pages.Add();
            var graphics = page.Graphics;
            var pageWidth = page.GetClientSize().Width;

            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
            var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            var navy = new PdfSolidBrush(new PdfColor(13, 27, 62));

            graphics.DrawRectangle(navy, new RectangleF(0, 0, pageWidth, 45));
            graphics.DrawString("ICS-214 CONSOLIDATED ACTIVITY LOG",
                titleFont, PdfBrushes.White, new PointF(15, 13));
            float y = 60f;

            graphics.DrawString(
                $"Incident: {incidentName}  |  Operational period: {operationalPeriodId}  |  " +
                $"Entries: {activityEntries.Count}",
                bodyFont, PdfBrushes.Black, new PointF(15, y));
            y += 24f;

            var grid = new PdfGrid();
            grid.Columns.Add(4);
            var header = grid.Rows.Add();
            header.Cells[0].Value = "Date/Time";
            header.Cells[1].Value = "Person";
            header.Cells[2].Value = "Activity";
            header.Cells[3].Value = "Notes";

            var entries = activityEntries
                .Select(f => Deserialize<Ics214EntryPayload>(f.JsonData) ?? new Ics214EntryPayload())
                .OrderBy(e => e.Timestamp);

            foreach (var entry in entries)
            {
                var row = grid.Rows.Add();
                row.Cells[0].Value = entry.Timestamp.ToString("dd MMM HH:mm", Inv);
                row.Cells[1].Value = entry.PersonName;
                row.Cells[2].Value = entry.Activity;
                row.Cells[3].Value = entry.Notes;
            }

            ApplyGridStyle(grid);
            DrawGrid(grid, page, 15, y);

            using var output = new MemoryStream();
            document.Save(output);
            var fileName = $"ICS214_Consolidated_OP{operationalPeriodId}_{Sanitize(incidentName)}.pdf";
            return new OilSpillExportFile(fileName, "application/pdf", output.ToArray());
        }

        public OilSpillExportFile GenerateIncidentPackageZip(
            OilSpillRunSummary summary,
            IReadOnlyList<IncidentForm> forms,
            byte[]? mapSnapshotPng = null)
        {
            var linked = forms.Where(f => f.IsLinkedToSitrep).ToList();
            var sitrep = GenerateSitrepPdf(summary, linked, mapSnapshotPng);
            var runSummary = GenerateRunSummaryPdf(summary);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteZipEntry(archive, sitrep.FileName, sitrep.Content);
                WriteZipEntry(archive, runSummary.FileName, runSummary.Content);

                foreach (var form in forms.OrderBy(f => f.FormType))
                {
                    var formPdf = GenerateFormPdf(form);
                    WriteZipEntry(archive, $"forms/{formPdf.FileName}", formPdf.Content);
                }
            }

            var packageName = $"IncidentPackage_{Sanitize(summary.Spill.SpillName)}_{summary.Run.Id}.zip";
            return new OilSpillExportFile(packageName, "application/zip", zipStream.ToArray());
        }

        private static void WriteZipEntry(ZipArchive archive, string entryName, byte[] content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            entryStream.Write(content, 0, content.Length);
        }

        private static string FormTitle(IncidentFormType type) => type switch
        {
            IncidentFormType.ICS201 => "ICS-201 Incident Briefing",
            IncidentFormType.ICS204 => "ICS-204 Assignment List",
            IncidentFormType.ICS209 => "ICS-209 Incident Status Summary",
            IncidentFormType.ICS214 => "ICS-214 Activity Log",
            _ => type.ToString()
        };

        private static PdfGrid BuildFormGrid(IncidentForm form)
        {
            var grid = new PdfGrid();
            grid.Columns.Add(2);

            switch (form.FormType)
            {
                case IncidentFormType.ICS209:
                {
                    var p = Deserialize<Ics209Payload>(form.JsonData) ?? new Ics209Payload();
                    AddRow(grid, "Incident name", p.IncidentName);
                    AddRow(grid, "Commander", p.Commander);
                    AddRow(grid, "Product type", p.ProductType);
                    AddRow(grid, "Status", p.Status);
                    AddRow(grid, "Estimated volume (m3)", p.EstimatedVolume.ToString("N1", Inv));
                    AddRow(grid, "Origin (lat, lon)",
                        $"{p.Latitude.ToString("F4", Inv)}, {p.Longitude.ToString("F4", Inv)}");
                    AddRow(grid, "Report time", p.ReportTime.ToString("dd MMM yyyy HH:mm", Inv));
                    AddRow(grid, "Situation summary", p.SituationSummary);
                    AddRow(grid, "Max slick area (km2)", p.MaxSlickAreaSqKm.ToString("N3", Inv));
                    AddRow(grid, "Total drift distance (km)", p.TotalDriftDistanceKm.ToString("N2", Inv));
                    AddRow(grid, "Shoreline impact",
                        p.HasShorelineImpact
                            ? $"YES - {p.ShorelineImpactTime:dd MMM yyyy HH:mm}"
                            : "No impact predicted");
                    AddRow(grid, "Response measures", p.ResponseMeasureCount.ToString(Inv));
                    AddRow(grid, "Planned actions", p.PlannedActions);
                    break;
                }
                case IncidentFormType.ICS201:
                {
                    var p = Deserialize<Ics201Payload>(form.JsonData) ?? new Ics201Payload();
                    AddRow(grid, "Incident name", p.IncidentName);
                    AddRow(grid, "Commander", p.Commander);
                    AddRow(grid, "Incident start", p.IncidentStartTime.ToString("dd MMM yyyy HH:mm", Inv));
                    AddRow(grid, "Source type", p.SourceType);
                    AddRow(grid, "Product type", p.ProductType);
                    AddRow(grid, "Estimated volume (m3)", p.EstimatedVolume.ToString("N1", Inv));
                    AddRow(grid, "Current situation", p.CurrentSituation);
                    AddRow(grid, "Initial objectives", p.InitialObjectives);
                    AddRow(grid, "Safety message", p.SafetyMessage);
                    break;
                }
                case IncidentFormType.ICS204:
                {
                    var p = Deserialize<Ics204Payload>(form.JsonData) ?? new Ics204Payload();
                    AddRow(grid, "Incident name", p.IncidentName);
                    AddRow(grid, "Division", p.Division);
                    AddRow(grid, "Branch / group", p.BranchOrGroup);
                    AddRow(grid, "Operations leader", p.OperationsLeader);
                    AddRow(grid, "Operational period start",
                        p.OperationalPeriodStart.ToString("dd MMM yyyy HH:mm", Inv));
                    AddRow(grid, "Operational period end",
                        p.OperationalPeriodEnd?.ToString("dd MMM yyyy HH:mm", Inv) ?? "Open");
                    AddRow(grid, "Resources", p.Resources);
                    AddRow(grid, "Assignment", p.Assignment);
                    AddRow(grid, "Special instructions", p.SpecialInstructions);
                    break;
                }
                case IncidentFormType.ICS214:
                {
                    // Each ICS-214 record is a single activity-log entry.
                    var entry = Deserialize<Ics214EntryPayload>(form.JsonData);
                    if (entry is not null && (!string.IsNullOrEmpty(entry.Activity) || !string.IsNullOrEmpty(entry.PersonName)))
                    {
                        AddRow(grid, "Incident name", entry.IncidentName);
                        AddRow(grid, "Date/Time", entry.Timestamp.ToString("dd MMM yyyy HH:mm", Inv));
                        AddRow(grid, "Person", entry.PersonName);
                        AddRow(grid, "Activity", entry.Activity);
                        AddRow(grid, "Notes", entry.Notes);
                        break;
                    }

                    // Backwards compatibility with legacy aggregate ICS-214 payloads.
                    var p = Deserialize<Ics214Payload>(form.JsonData) ?? new Ics214Payload();
                    AddRow(grid, "Incident name", p.IncidentName);
                    if (p.Entries.Count == 0)
                    {
                        AddRow(grid, "Activity log", "No entries recorded.");
                    }
                    else
                    {
                        foreach (var legacyEntry in p.Entries.OrderBy(e => e.Timestamp))
                        {
                            AddRow(grid,
                                legacyEntry.Timestamp.ToString("dd MMM HH:mm", Inv),
                                $"{legacyEntry.Activity} ({legacyEntry.PerformedBy})");
                        }
                    }
                    break;
                }
                default:
                    AddRow(grid, "Payload", form.JsonData);
                    break;
            }

            return grid;
        }

        private static T? Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private static string BuildEffectivenessNotes(OilSpillRunSummary summary)
        {
            if (summary.Actions.Count == 0)
            {
                return "No response measures were recorded for this incident.";
            }

            var booms = summary.Actions.Count(a => a.ActionType == OilSpillActionType.DeployBoom);
            var skimmers = summary.Actions.Count(a => a.ActionType == OilSpillActionType.Skimmer);
            var dispersants = summary.Actions.Count(a => a.ActionType == OilSpillActionType.Dispersant);
            var protection = summary.Actions.Count(a => a.ActionType == OilSpillActionType.ShorelineProtection);

            var notes =
                $"Response inventory: {booms} boom deployment(s), {skimmers} skimmer(s), " +
                $"{dispersants} dispersant application(s), and {protection} shoreline-protection measure(s). ";

            notes += summary.HasShorelineImpact
                ? "The model predicts shoreline contact; prioritise shoreline-protection and recovery near the impact zone."
                : "No shoreline contact is predicted under the modelled conditions; maintain offshore containment and recovery.";

            return notes;
        }

        private static float DrawHeading(
            PdfGraphics graphics, string text, PdfFont font, PdfBrush brush, float y)
        {
            graphics.DrawString(text, font, brush, new PointF(15, y));
            return y + 18f;
        }

        private static float DrawGrid(PdfGrid grid, PdfPage page, float x, float y)
        {
            var result = grid.Draw(page, new PointF(x, y));
            return result?.Bounds.Bottom ?? y;
        }

        private static void AddRow(PdfGrid grid, string label, string value)
        {
            var row = grid.Rows.Add();
            row.Cells[0].Value = label;
            row.Cells[1].Value = value;
        }

        private static void ApplyGridStyle(PdfGrid grid)
        {
            grid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
        }

        private static void AddDocHeading(IWSection section, string text)
        {
            var paragraph = section.AddParagraph();
            paragraph.ParagraphFormat.BeforeSpacing = 10f;
            var run = paragraph.AppendText(text);
            run.CharacterFormat.FontSize = 13;
            run.CharacterFormat.Bold = true;
            run.CharacterFormat.TextColor = Color.FromArgb(0, 128, 128);
        }

        private static void AddDocLine(IWSection section, string label, string value)
        {
            var paragraph = section.AddParagraph();
            var labelRun = paragraph.AppendText($"{label}: ");
            labelRun.CharacterFormat.Bold = true;
            paragraph.AppendText(value);
        }

        private static string Sanitize(string value)
        {
            var cleaned = new string(value.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? "spill" : cleaned;
        }
    }
}

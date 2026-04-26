using Microsoft.Extensions.Logging;

namespace GeeksHackingPortal.Api.Services;

public static class SchemaDifferenceLogger
{
    public static void Write(
        ILogger logger,
        SchemaDifferenceReport report,
        LogLevel differenceLevel = LogLevel.Information
    )
    {
        if (!report.HasDifferences)
        {
            logger.LogInformation("No pending database schema changes.");
            return;
        }

        logger.Log(
            differenceLevel,
            "Pending database schema changes detected across {TableCount} table(s).",
            report.Tables.Count
        );

        foreach (var table in report.Tables)
        {
            logger.Log(
                differenceLevel,
                "{TableName}: +{AddedColumns} ~{UpdatedColumns} -{DeletedColumns} remarks:{UpdatedRemarks}",
                table.TableName,
                table.AddColumns.Count,
                table.UpdateColumns.Count,
                table.DeleteColumns.Count,
                table.UpdateRemarks.Count
            );

            foreach (var message in GetDiffMessages(table))
            {
                logger.Log(differenceLevel, "  {Message}", message);
            }
        }

        if (!string.IsNullOrWhiteSpace(report.RawText))
        {
            logger.LogDebug(
                "Raw SqlSugar schema diff:{NewLine}{SchemaDiff}",
                Environment.NewLine,
                report.RawText
            );
        }
    }

    private static IEnumerable<string> GetDiffMessages(SchemaDifferenceTable table) =>
        table.Differences
            .Select(column => column.Message)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => message!);
}

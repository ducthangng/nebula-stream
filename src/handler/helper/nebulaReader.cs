using System.Runtime.InteropServices;
using System.Text.Json;
using Docker.DotNet.Models;
using Spectre.Console;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nebula.Helper;

public class NebulaReader
{
    public class ProjectState
    {
        public string ProjectID { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }

    public class ImageRecord
    {
        public string ProjectID { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ImageID { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public long Size { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    }

    public async Task<ProjectState?> GetProjectContextAsync()
    {
        string statePath = Path.Combine(".nebula", "state.json");

        if (!File.Exists(statePath))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] .nebula/state.json not found. Please use nebula init or read docs for more details.");
            return null;
        }

        try
        {
            string jsonContent = await File.ReadAllTextAsync(statePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<ProjectState>(jsonContent, options);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error parsing nebula.yml:[/] {ex.Message}");
            return null;
        }
    }

    public async Task SaveImageRecordAsync(string tag, long sizeBytes)
    {
        var state = await GetProjectContextAsync();
        if (state == null) return;

        string imagesPath = Path.Combine(".nebula", "images.json");
        List<ImageRecord> records = new();

        try
        {
            if (File.Exists(imagesPath))
            {
                string existingJson = await File.ReadAllTextAsync(imagesPath);
                records = JsonSerializer.Deserialize<List<ImageRecord>>(existingJson) ?? new();
            }

            records.Add(new ImageRecord
            {
                ProjectID = state.ProjectID,
                ProjectName = state.ProjectName,
                ImageID = tag,
                Tag = tag,
                Size = sizeBytes,
                CreatedAt = DateTime.Now
            });

            var options = new JsonSerializerOptions { WriteIndented = true };
            string updatedJson = JsonSerializer.Serialize(records, options);
            await File.WriteAllTextAsync(imagesPath, updatedJson);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to log image record:[/] {ex.Message}");
        }
    }

    private string FormatSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{Math.Round(size, 2)} {units[unitIndex]}";
    }

    private string GetRelativeTime(DateTime createdAt)
    {
        var span = DateTime.Now - createdAt;
        if (span.TotalDays >= 1)
        {
            return $"{(int)span.TotalDays} days ago";
        }
        if (span.TotalHours >= 1)
        {
            return $"{(int)span.TotalHours} hours ago";
        }
        if (span.TotalMinutes >= 1)
        {
            return $"{(int)span.TotalMinutes} minutes ago";
        }

        return "Just now";
    }

    private void DisplayImages(List<ImageRecord> records)
    {
        var table = new Table().Border(TableBorder.None);
        table.AddColumn("[bold]REPOSITORY[/]");
        table.AddColumn("[BOLD]TAG[/]");
        table.AddColumn("[BOLD]SIZE[/]");
        table.AddColumn("[BOLD]CREATED[/]");

        foreach (var record in records.OrderByDescending(r => r.CreatedAt))
        {
            table.AddRow(
                $"[blue]{record.ProjectName}[/]",
                record.Tag,
                FormatSize(record.Size),
                GetRelativeTime(record.CreatedAt)
            );
        }

        AnsiConsole.Write(table);
    }

    public async Task RetrieveImagesRecordAsync()
    {
        var pathName = Path.Combine(".nebula", "images.json");
        List<ImageRecord> records = new();

        if (File.Exists(pathName))
        {
            var content = await File.ReadAllTextAsync(pathName);
            records = JsonSerializer.Deserialize<List<ImageRecord>>(content) ?? new();
        }

        DisplayImages(records);
    }
}
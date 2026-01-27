using System.CommandLine;
using System.CommandLine.NamingConventionBinder; // Add this if SetHandler is missing
using Spectre.Console;

namespace Nebula.Handlers;

public static class CLIHandler {
    public static async Task<int> RunAsync(string[] args) {
        var rootCommand = new RootCommand("Nebula CLI");

        var helloCommand = new Command("hello", "Display a greeting!");

        helloCommand.SetHandler(() => {
            AnsiConsole.MarkupLine("[bold blue]Nebula:[/] Hello World!");
        });

        rootCommand.AddCommand(helloCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
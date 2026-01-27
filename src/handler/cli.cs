using System.CommandLine;
using System.CommandLine.NamingConventionBinder; // Add this if SetHandler is missing
using Spectre.Console;

namespace Nebula.Handlers;

public static class CLIHandler {
    public static async Task<int> RunAsync(string[] args) {
        var rootCommand = new RootCommand("Nebula CLI");

        var initCommand = new Command("init", "Initialize Nebula Project!");

        initCommand.SetHandler(() => {
            AnsiConsole.MarkupLine("[bold blue]Nebula:[/] init World!");
        });

        rootCommand.AddCommand(initCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
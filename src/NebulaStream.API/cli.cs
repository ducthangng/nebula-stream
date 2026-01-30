using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Spectre.Console;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Nebula.Handlers;

public static class CLIHandler {
    public static async Task<int> RunAsync(string[] args) {
        var rootCommand = new RootCommand("Nebula CLI");

        // --- 1. INIT COMMAND ---
        var initCommand = new Command("init", "Initialize Nebula Project!");
        initCommand.SetHandler(async () => await InitCommand());
        rootCommand.AddCommand(initCommand);

        // --- 2. DEPLOY COMMAND ---
        var deployCommand = new Command("deploy", "Deploy infrastructure to the cloud");
        deployCommand.SetHandler(async () => await DeployCommand());
        rootCommand.AddCommand(deployCommand);

        // --- 3. BUILD COMMAND ---
        var buildCommand = new Command("build", "Build Docker Image");
        buildCommand.SetHandler(async () => await BuildCommand());
        rootCommand.AddCommand(buildCommand);

        return await rootCommand.InvokeAsync(args);
    }

    public static async Task InitCommand() {
        AnsiConsole.MarkupLine("[bold blue]Initialize Nebula Project![/]");

        string fileName = "nebula.yaml";

        try {
            var assembly = Assembly.GetExecutingAssembly();

            string resourceName = "nebula_stream.default.yml";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null) {
                throw new Exception("Embeded default.yml not found");
            }

            using StreamReader reader = new StreamReader(stream);
            string content = await reader.ReadToEndAsync();

            if (!File.Exists(fileName)) {
                await File.WriteAllTextAsync(fileName, content);
                AnsiConsole.MarkupLineInterpolated($"[green]Success:[/] Created [bold]{fileName}[/] from template.");
            } else {
                AnsiConsole.MarkupLine("[yellow]Skip:[/] nebula.yaml already exists.");
            }
        } catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
        }
    }

    // @TODO
    public static async Task DeployCommand() {
        AnsiConsole.MarkupLine("[bold blue]Deploy Your Project![/]");
    }

    public static async Task BuildCommand() {
        AnsiConsole.MarkupLine("[bold blue]Build Your Project![/]");
        string dockerFile = "Dockerfile";
        // Check file Dockerfile exist or not
        if (!File.Exists(dockerFile)) {
            AnsiConsole.MarkupLine($"[red]Warning:[/] {dockerFile} does not exist in this folder.");
            return;
        }

        // Ready to build
        await AnsiConsole.Status()
            .StartAsync("Building Docker Image...", async ctx => {
                var startInfo = new ProcessStartInfo {
                    FileName = "docker",
                    Arguments = "build -t nebula-app .",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process {StartInfo = startInfo};

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(output)) {
                    AnsiConsole.MarkupLine($"[yellow]LOG:[/] {Markup.Escape(output)}");
                }

                if (!string.IsNullOrEmpty(error)) {
                    AnsiConsole.MarkupLine($"[red]ERR:[/] {Markup.Escape(error)}");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0) {
                    AnsiConsole.MarkupLine("[green]✔[/] Docker build successful!");
                } else {
                    AnsiConsole.MarkupLine("[red]✘[/] Docker build failed. Check your Dockerfile.");
                }
            });

        // Store in local

        // @TODO: Store in S3
    }
};

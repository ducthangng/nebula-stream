using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Spectre.Console;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Nebula.Entity;

namespace Nebula.Handlers;

public class CLIHandler {
    private readonly Registry _registry;

    public CLIHandler(Registry registry) {
        _registry = registry;
    }

    public async Task<int> RunAsync(string[] args) {
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

    public async Task InitCommand() {
        AnsiConsole.MarkupLine("[bold blue]Initialize Nebula Project![/]");

        string fileName = "nebula.yaml";

        try {
            var assembly = Assembly.GetExecutingAssembly();

            string resourceName = "nebula_stream.default.yml";

            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception("Embeded default.yml not found");

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
    public async Task DeployCommand() {
        AnsiConsole.MarkupLine("[bold blue]Deploy Your Project![/]");
    }

    public async Task BuildCommand() {
        AnsiConsole.MarkupLine("[bold blue]Build Your Project![/]");
        string dockerFile = "Dockerfile";
        string ecrURI = Environment.GetEnvironmentVariable("AWS_URI_DOCKER_REPO") ?? throw new Exception("Environment variable AWS_URI_DOCKER_REPO is missing!");
        string uniqueTag = Guid.NewGuid().ToString("n").Substring(0, 8);
        string tag = uniqueTag;

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
                    Arguments = $"build -t {ecrURI}:{tag} .",
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

                // @TODO: Store in ECR
                AnsiConsole.MarkupLine($"[yellow]Pushing to AWS ECR...[/]");
                await _registry.PushToECRAsync(ecrURI, tag);

                AnsiConsole.MarkupLine("[bold green]DONE![/] Your image is now in the cloud.");
            });

       
    }
};

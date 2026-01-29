using System.CommandLine;
using Spectre.Console;
using System.Reflection;
using System.Diagnostics;
using Nebula.ECR;
using Nebula.Helper;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace Nebula.Handlers;


public class CLIHandler
{
    private readonly Registry _registry;
    private readonly NebulaReader _nebulaReader;
    public CLIHandler(Registry registry)
    {
        _registry = registry;
        _nebulaReader = new NebulaReader();
    }

    public async Task<int> RunAsync(string[] args)
    {
        var rootCommand = new RootCommand("Nebula CLI");

        // --- 1. INIT COMMAND ---
        var initCommand = new Command("init", "Initialize Nebula Project!");
        var nameOption = new Option<string?>(
            aliases: new[] { "--name", "-n" },
            description: "The name of the project"
        );
        var forceOption = new Option<bool>(
            aliases: new[] { "--force" },
            description: "Force overiding the nebula folder"
        );
        initCommand.AddOption(nameOption);
        initCommand.AddOption(forceOption);
        initCommand.SetHandler(async (nameProject, force) => await InitCommand(nameProject, force), nameOption, forceOption);
        rootCommand.AddCommand(initCommand);

        // --- 2. DEPLOY COMMAND ---
        var deployCommand = new Command("deploy", "Deploy infrastructure to the cloud");
        deployCommand.SetHandler(async () => await DeployCommand());
        rootCommand.AddCommand(deployCommand);

        // --- 3. BUILD COMMAND ---
        var buildCommand = new Command("build", "Build Docker Image");
        buildCommand.SetHandler(async () => await BuildCommand());
        rootCommand.AddCommand(buildCommand);

        // --- 4. RETRIEVE IMAGES COMMAND ---
        var imagesCommand = new Command("images", "Retrieve images");
        imagesCommand.SetHandler(async () => await RetrieveImagesCommand());
        rootCommand.AddCommand(imagesCommand);

        return await rootCommand.InvokeAsync(args);
    }

    public async Task InitCommand(string? nameProject, bool force)
    {
        AnsiConsole.MarkupLine("[bold blue]Initialize Nebula Project![/]");

        string projectName = nameProject ?? AnsiConsole.Ask<string>("Project Name:");
        string id = Guid.NewGuid().ToString();

        string fileName = "nebula.yml";
        string folderName = ".nebula";

        if (!Directory.Exists(folderName) || force)
        {
            DirectoryInfo di = Directory.CreateDirectory(folderName);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                di.Attributes |= FileAttributes.Hidden;
            }

            var state = new NebulaReader.ProjectState
            {
                ProjectID = id,
                ProjectName = projectName
            };

            var options = new JsonSerializerOptions { WriteIndented = true };

            string statePath = Path.Combine(".nebula", "state.json");
            string jsonContent = JsonSerializer.Serialize(state, options);
            await File.WriteAllTextAsync(statePath, jsonContent);

            AnsiConsole.MarkupLine("[green]✔[/] Created hidden .nebula folder.");
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();

            string resourceName = "nebula_stream.default.yml";

            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception("Embeded default.yml not found");

            using StreamReader reader = new StreamReader(stream);
            string content = await reader.ReadToEndAsync();

            if (!File.Exists(fileName))
            {
                await File.WriteAllTextAsync(fileName, content);
                AnsiConsole.MarkupLineInterpolated($"[green]Success:[/] Created [bold]{fileName}[/] from template.");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Skip:[/] nebula.yaml already exists.");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
        }
    }

    // @TODO
    public async Task DeployCommand()
    {
        AnsiConsole.MarkupLine("[bold blue]Deploy Your Project![/]");
    }

    public async Task RetrieveImagesCommand()
    {
        await _nebulaReader.RetrieveImagesRecordAsync();
    }

    public async Task BuildCommand()
    {
        AnsiConsole.MarkupLine("[bold blue]Build Your Project![/]");
        string dockerFile = "Dockerfile";
        string ecrURI = Environment.GetEnvironmentVariable("AWS_URI_DOCKER_REPO") ?? throw new Exception("Environment variable AWS_URI_DOCKER_REPO is missing!");
        string uniqueTag = Guid.NewGuid().ToString("n").Substring(0, 8);
        string tag = uniqueTag;

        NebulaReader.ProjectState? projectState = await _nebulaReader.GetProjectContextAsync();
        if (projectState == null)
        {
            AnsiConsole.WriteLine($"[red]Nebula.yml file does not exist");
            return;
        }

        Console.WriteLine($"Project name: {projectState.ProjectName}");

        // Check file Dockerfile exist or not
        if (!File.Exists(dockerFile))
        {
            AnsiConsole.MarkupLine($"[red]Warning:[/] {dockerFile} does not exist in this folder.");
            return;
        }

        // Ready to build
        await AnsiConsole.Status()
            .StartAsync("Building Docker Image...", async ctx =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"build -t {ecrURI}:{tag} .",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = startInfo };

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(output))
                {
                    AnsiConsole.MarkupLine($"[yellow]LOG:[/] {Markup.Escape(output)}");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    AnsiConsole.MarkupLine($"[red]ERR:[/] {Markup.Escape(error)}");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    AnsiConsole.MarkupLine("[green]✔[/] Docker build successful!");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]✘[/] Docker build failed. Check your Dockerfile.");
                }

                // @TODO: Store in ECR
                AnsiConsole.MarkupLine($"[yellow]Pushing to AWS ECR...[/]");
                await _registry.PushToECRAsync(ecrURI, tag);
                var sizeInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"image inspect --format=\"{{{{.Size}}}}\" {ecrURI}:{tag}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var sizeProcess = new Process { StartInfo = sizeInfo };
                sizeProcess.Start();

                string sizeOutput = await sizeProcess.StandardOutput.ReadToEndAsync();
                await sizeProcess.WaitForExitAsync();

                if (long.TryParse(sizeOutput.Trim(), out long sizeInBytes))
                {
                    AnsiConsole.MarkupLine($"[grey]Image Size captured: {sizeInBytes} bytes[/]");
                    await _nebulaReader.SaveImageRecordAsync(tag, sizeInBytes);
                }

                var removeImageInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"rmi {ecrURI}:{tag}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var removeProcess = new Process { StartInfo = removeImageInfo };

                removeProcess.Start();

                var removeOutput = await removeProcess.StandardOutput.ReadToEndAsync();
                if (!String.IsNullOrEmpty(removeOutput))
                {
                    AnsiConsole.WriteLine($"[bold yellow]LOG[/]: {Markup.Escape(removeOutput)}");
                }
                await removeProcess.WaitForExitAsync();

                AnsiConsole.MarkupLine("[bold green]DONE![/] Your image is now in the cloud.");
            });
    }
};

using Amazon;
using Amazon.ECR;
using Amazon.ECR.Model;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Text;
using Spectre.Console;

namespace Nebula.ECR;

public class Registry
{
    private readonly AmazonECRClient _ecrClient;
    private readonly DockerClient _dockerClient;

    public Registry()
    {
        _ecrClient = new AmazonECRClient(RegionEndpoint.APSoutheast1);
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public async Task PushToECRAsync(string repositoryURI, string tag = "latest")
    {
        // Auth
        var authResponse = await _ecrClient.GetAuthorizationTokenAsync(new GetAuthorizationTokenRequest());
        var authData = authResponse.AuthorizationData[0];

        // Decode
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authData.AuthorizationToken));
        var password = decoded.Split(":")[1];

        await _dockerClient.Images.PushImageAsync(
            $"{repositoryURI}:{tag}",
            new ImagePushParameters(),
            new AuthConfig
            {
                Username = "AWS",
                Password = password,
                ServerAddress = authData.ProxyEndpoint
            },
            new Progress<JSONMessage>(msg => Console.WriteLine($"[Docker] {msg.Status}"))
        );
    }

    public async Task<bool> ImageExistsLocallyAsync(string repositoryURI, string tag)
    {
        var fullImageName = $"{repositoryURI}:{tag}";

        var parameters = new ImagesListParameters
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>
        {
            {
                "reference", new Dictionary<string, bool>
                {
                    { fullImageName, true }
                }
            }
        }
        };

        var images = await _dockerClient.Images.ListImagesAsync(parameters);
        return images.Count > 0;
    }

    public async Task PullFromECRAsync(string repoURI, string tag = "latest")
    {
        var exist = await ImageExistsLocallyAsync(repoURI, tag);
        if (exist)
        {
            return;
        }


        var authResponse = await _ecrClient.GetAuthorizationTokenAsync(new GetAuthorizationTokenRequest());
        var authData = authResponse.AuthorizationData[0];

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authData.AuthorizationToken));
        var password = decoded.Split(":")[1];

        await _dockerClient.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = repoURI,
                Tag = tag
            },
            new AuthConfig
            {
                Username = "AWS",
                Password = password,
                ServerAddress = authData.ProxyEndpoint
            },
            new Progress<JSONMessage>(msg => AnsiConsole.MarkupLine($"[grey][[Docker]] {msg.Status} {msg.ProgressMessage}[/]"))
        );
    }
}
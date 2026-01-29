using Amazon;
using Amazon.ECR;
using Amazon.ECR.Model;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Text;

namespace Nebula.Entity;

public class Registry {
    private readonly AmazonECRClient _ecrClient;
    private readonly DockerClient _dockerClient;

    public Registry() {
        _ecrClient = new AmazonECRClient(RegionEndpoint.APSoutheast1);
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public async Task PushToECRAsync(string repositoryURI, string tag = "latest") {
        // Auth
        var authResponse = await _ecrClient.GetAuthorizationTokenAsync(new GetAuthorizationTokenRequest());
        var authData = authResponse.AuthorizationData[0];

        // Decode
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authData.AuthorizationToken));
        var password = decoded.Split(":")[1];

        await _dockerClient.Images.PushImageAsync(
            $"{repositoryURI}:{tag}",
            new ImagePushParameters(),
            new AuthConfig{
                Username = "AWS",
                Password = password,
                ServerAddress = authData.ProxyEndpoint
            },
            new Progress<JSONMessage>(msg => Console.WriteLine($"[Docker] {msg.Status}"))
        );

    }
}
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using OneClickStream.Configuration;
using OneClickStream.PostData;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OneClickStream.Services
{
  public class OutputsSerrvice : AzureMediaBaseService
  {
    #region Constructors

    public OutputsSerrvice(ConfigWrapper config) : base(config)
    {
    }

    #endregion Constructors

    #region Methods

    public async Task<OutputsPostResultData> CreateOutputs(string uniqueness)
    {
      this.InitializeNames(uniqueness);

      string playerPath = string.Empty;
      StringBuilder sb = new StringBuilder();

      try
      {
        var client = await CreateMediaServicesClientAsync(this.config);

        Asset asset = await this.CreateLiveOutput(this.config, client, sb);
        StreamingEndpoint streamingEndpoint = await this.SetupStreamingEndpoint(this.config, client, asset, sb);
        ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(this.config.ResourceGroup, this.config.AccountName, this.streamingLocatorName);
        this.GetStreamingPaths(streamingEndpoint, paths, out StringBuilder stringBuilder, out playerPath, out bool hasStreamingPaths, sb);

        if (hasStreamingPaths)
        {
          sb.AppendLine(stringBuilder.ToString());
          sb.AppendLine("Open the following URL to playback the published,recording LiveOutput in the Azure Media Player");
          sb.AppendLine($"\t https://ampdemo.azureedge.net/?url={playerPath}&heuristicprofile=lowlatency");
          sb.AppendLine();

          sb.AppendLine("Continue experimenting with the stream until you are ready to finish.");
        }
        else
        {
          sb.AppendLine("No Streaming Paths were detected.  Has the Stream been started?");
        }
      }
      catch (ApiErrorException e)
      {
        sb.AppendLine("Hit ApiErrorException");
        sb.AppendLine($"\tCode: {e.Body.Error.Code}");
        sb.AppendLine($"\tCode: {e.Body.Error.Message}");
        sb.AppendLine();
        sb.AppendLine("Exiting, cleanup may be necessary...");
      }

      return new OutputsPostResultData() { Id = uniqueness, EndpointDataSource = playerPath, Log = sb.ToString()  };
    }

    private async Task<Asset> CreateLiveOutput(ConfigWrapper config, IAzureMediaServicesClient client, StringBuilder sb)
    {
      sb.AppendLine($"Creating an asset named {this.assetName}");
      sb.AppendLine();
      Asset asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName, this.assetName, new Asset());

      string manifestName = "output";
      sb.AppendLine($"Creating a live output named {this.liveOutputName}");
      sb.AppendLine();

      LiveOutput liveOutput = new LiveOutput(assetName: asset.Name, manifestName: manifestName, archiveWindowLength: TimeSpan.FromMinutes(10));
      liveOutput = await client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName, this.liveEventName, this.liveOutputName, liveOutput);

      return asset;
    }

    private void GetStreamingPaths(StreamingEndpoint streamingEndpoint, ListPathsResponse paths, out StringBuilder stringBuilder, out string playerPath, out bool hasStreamingPaths, StringBuilder sb)
    {
      sb.AppendLine("The urls to stream the output from a client:");
      sb.AppendLine();
      stringBuilder = new StringBuilder();
      playerPath = string.Empty;
      for (int i = 0; i < paths.StreamingPaths.Count; i++)
      {
        UriBuilder uriBuilder = new UriBuilder
        {
          Scheme = "https",
          Host = streamingEndpoint.HostName
        };

        if (paths.StreamingPaths[i].Paths.Count > 0)
        {
          uriBuilder.Path = paths.StreamingPaths[i].Paths[0];
          stringBuilder.AppendLine($"\t{paths.StreamingPaths[i].StreamingProtocol}-{paths.StreamingPaths[i].EncryptionScheme}");
          stringBuilder.AppendLine($"\t\t{uriBuilder.ToString()}");
          stringBuilder.AppendLine();

          if (paths.StreamingPaths[i].StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
          {
            playerPath = uriBuilder.ToString();
          }
        }
      }

      hasStreamingPaths = stringBuilder.Length > 0;
    }

    private async Task<StreamingEndpoint> SetupStreamingEndpoint(ConfigWrapper config, IAzureMediaServicesClient client, Asset asset, StringBuilder sb)
    {
      sb.AppendLine($"Creating a streaming locator named {this.streamingLocatorName}");
      sb.AppendLine();

      StreamingLocator locator = new StreamingLocator(assetName: asset.Name, streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly);
      locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName, this.streamingLocatorName, locator);

      StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(config.ResourceGroup, config.AccountName, this.streamingEndpointName);

      if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
      {
        sb.AppendLine("Streaming Endpoint was Stopped, restarting now..");
        await client.StreamingEndpoints.StartAsync(config.ResourceGroup, config.AccountName, this.streamingEndpointName);
      }

      return streamingEndpoint;
    }

    #endregion Methods
  }
}
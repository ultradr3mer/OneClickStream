using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using OneClickStream.Configuration;
using OneClickStream.Data;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OneClickStream.Services
{
  public class OutputsService : AzureMediaBaseService
  {
    #region Constructors

    public OutputsService(ConfigWrapper config) : base(config)
    {
    }

    #endregion Constructors

    #region Methods

    public async Task<OutputsPostResultData> CreateOutputs(string uniqueness)
    {
      this.InitializeNames(uniqueness);

      StringBuilder sb = new StringBuilder();

      try
      {
        IAzureMediaServicesClient client = await CreateMediaServicesClientAsync(this.config);

        var liveEvent = await client.LiveEvents.GetAsync(this.config.ResourceGroup, this.config.AccountName, liveEventName);

        Asset asset = await this.CreateLiveOutput(this.config, client, sb);
        StreamingEndpoint streamingEndpoint = await this.SetupStreamingEndpoint(this.config, client, asset, sb);
      }
      catch (ApiErrorException e)
      {
        sb.AppendLine("Hit ApiErrorException");
        sb.AppendLine($"\tCode: {e.Body.Error.Code}");
        sb.AppendLine($"\tCode: {e.Body.Error.Message}");
        sb.AppendLine();
        sb.AppendLine("Exiting, cleanup may be necessary...");
      }

      return new OutputsPostResultData() { Id = uniqueness, Log = sb.ToString() };
    }

    public async Task<GetStreamUrlsData> GetPaths(string uniqueness)
    {
      this.InitializeNames(uniqueness);

      string playerPath = string.Empty;
      StringBuilder sb = new StringBuilder();

      try
      {
        IAzureMediaServicesClient client = await CreateMediaServicesClientAsync(this.config);

        StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(this.config.ResourceGroup, this.config.AccountName, this.streamingEndpointName);
        ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(this.config.ResourceGroup, this.config.AccountName, this.streamingLocatorName);
        bool hasStreamingPaths = this.GetStreamingPaths(streamingEndpoint, paths, out playerPath, out StringBuilder sbPaths);

        if (hasStreamingPaths)
        {
          sb.AppendLine(sbPaths.ToString());
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

      return new GetStreamUrlsData() { Id = uniqueness, EndpointDataSource = playerPath, Log = sb.ToString() };
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

    private bool GetStreamingPaths(StreamingEndpoint streamingEndpoint, ListPathsResponse paths, out string playerPath, out StringBuilder sb)
    {
      sb = new StringBuilder();
      sb.AppendLine("The urls to stream the output from a client:");
      sb.AppendLine();
      playerPath = string.Empty;
      bool hasStreamingPaths = false;

      foreach (StreamingPath path in paths.StreamingPaths)
      {
        UriBuilder uriBuilder = new UriBuilder
        {
          Scheme = "https",
          Host = streamingEndpoint.HostName
        };

        if (path.Paths.Count > 0)
        {
          uriBuilder.Path = path.Paths[0];
          sb.AppendLine($"\t{path.StreamingProtocol}-{path.EncryptionScheme}");
          sb.AppendLine($"\t\t{uriBuilder}");
          sb.AppendLine();

          if (path.StreamingProtocol == StreamingPolicyStreamingProtocol.Dash)
          {
            playerPath = uriBuilder.ToString();
            hasStreamingPaths = true;
          }
        }
      }

      return hasStreamingPaths;
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
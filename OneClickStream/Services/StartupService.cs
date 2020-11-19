using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using OneClickStream.Configuration;
using OneClickStream.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickStream.Services
{
  public class StartupService : AzureMediaBaseService
  {
    #region Constructors

    public StartupService(ConfigWrapper config) : base(config)
    {
    }

    #endregion Constructors

    #region Methods

    public async Task<StartupPostResultData> Startup()
    {
      StringBuilder sb = new StringBuilder();
      this.InitializeNames(Guid.NewGuid().ToString().Substring(0, 13));
      StartupPostResultData resultData = null;

      try
      {
        var client = await CreateMediaServicesClientAsync(this.config);

        MediaService mediaService = await client.Mediaservices.GetAsync(this.config.ResourceGroup, this.config.AccountName);
        LiveEvent liveEvent = await this.CreateLiveEvent(this.liveEventName, client, mediaService, this.config, sb);
        resultData = this.SetupStream(liveEvent, sb);
      }
      catch (ApiErrorException e)
      {
        sb.AppendLine("Hit ApiErrorException");
        sb.AppendLine($"\tCode: {e.Body.Error.Code}");
        sb.AppendLine($"\tCode: {e.Body.Error.Message}");
        sb.AppendLine();
        sb.AppendLine("Exiting, cleanup may be necessary...");
      }

      resultData.Log = sb.ToString();
      resultData.Id = this.uniqueness;

      return resultData;
    }

    private async Task<LiveEvent> CreateLiveEvent(string liveEventName, IAzureMediaServicesClient client, MediaService mediaService, ConfigWrapper config, StringBuilder sb)
    {
      sb.AppendLine($"Creating a live event named {liveEventName}");
      sb.AppendLine();

      IPRange allAllowIPRange = new IPRange(
          name: "AllowAll",
          address: "0.0.0.0",
          subnetPrefixLength: 0
      );

      LiveEventInputAccessControl liveEventInputAccess = new LiveEventInputAccessControl
      {
        Ip = new IPAccessControl(
                  allow: new IPRange[]
                  {
                    allAllowIPRange
                  }
              )
      };

      LiveEventPreview liveEventPreview = new LiveEventPreview
      {
        AccessControl = new LiveEventPreviewAccessControl(
              ip: new IPAccessControl(
                  allow: new IPRange[]
                  {
                    allAllowIPRange
                  }
              )
          )
      };

      LiveEvent liveEvent = new LiveEvent(
          location: mediaService.Location,
          description: "Sample LiveEvent for testing",
          //vanityUrl: false,
          encoding: new LiveEventEncoding(
                      encodingType: LiveEventEncodingType.None,
                      presetName: null
                  ),
          input: new LiveEventInput(LiveEventInputProtocol.RTMP, liveEventInputAccess),
          preview: liveEventPreview,
          streamOptions: new List<StreamOptionsFlag?>()
          {
            StreamOptionsFlag.LowLatency
          }
      );

      sb.AppendLine($"Creating the LiveEvent, be patient this can take time...");
      var result = await client.LiveEvents.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName, liveEvent, autoStart: true);
      
      return result;
    }

    private StartupPostResultData SetupStream(LiveEvent liveEvent, StringBuilder sb)
    {
      string ingestUrl = liveEvent.Input.Endpoints.First().Url;
      sb.AppendLine($"The ingest url to configure the on premise encoder with is:");
      sb.AppendLine($"\t{ingestUrl}");
      sb.AppendLine();

      var previewSource = liveEvent.Preview.Endpoints.First().Url;
      sb.AppendLine($"The preview url is:");
      sb.AppendLine($"\t{previewSource}");
      sb.AppendLine();

      sb.AppendLine($"Open the live preview in your browser and use the Azure Media Player to monitor the preview playback:");
      sb.AppendLine($"\thttps://ampdemo.azureedge.net/?url={previewSource}&heuristicprofile=lowlatency");
      sb.AppendLine();

      sb.AppendLine("Start the live stream now, sending the input to the ingest url and verify that it is arriving with the preview url.");
      return new StartupPostResultData() { PreviewSource = previewSource, IngestUrl = ingestUrl };
    }

    #endregion Methods
  }
}
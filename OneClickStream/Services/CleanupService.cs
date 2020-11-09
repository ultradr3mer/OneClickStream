using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using OneClickStream.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace OneClickStream.Services
{
  public class CleanupService : AzureMediaBaseService
  {
    #region Constructors

    public CleanupService(ConfigWrapper config) : base(config)
    {
    }

    #endregion Constructors

    #region Methods

    public async Task Cleanup(string uniqueness)
    {
      this.InitializeNames(uniqueness);

      StringBuilder sb = new StringBuilder();

      IAzureMediaServicesClient client = null;
      try
      {
        client = await CreateMediaServicesClientAsync(config);

        bool deleted = await this.CleanupLiveEventAndOutputAsync(client, config.ResourceGroup, config.AccountName, this.liveEventName, sb);
        if (deleted)
        {
          sb.AppendLine("The LiveOutput and LiveEvent are now deleted.  The event is available as an archive and can still be streamed.");
        }
        else
        {
          sb.AppendLine("No LiveEvent were detected.  Has the Stream been started?");
          sb.AppendLine("Cleaning up and Exiting...");
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
      finally
      {
        await this.CleanupLiveEventAndOutputAsync(client, config.ResourceGroup, config.AccountName, this.liveEventName, sb);
        await this.CleanupLocatorandAssetAsync(client, config.ResourceGroup, config.AccountName, this.streamingLocatorName, this.assetName, sb);
      }
    }

    private async Task<bool> CleanupLiveEventAndOutputAsync(IAzureMediaServicesClient client, string resourceGroup, string accountName, string liveEventName, StringBuilder sb)
    {
      bool deleted = false;

      try
      {
        LiveEvent liveEvent = await client.LiveEvents.GetAsync(resourceGroup, accountName, liveEventName);

        if (liveEvent != null)
        {
          if (liveEvent.ResourceState == LiveEventResourceState.Running)
          {
            // If the LiveEvent is running, stop it and have it remove any LiveOutputs
            await client.LiveEvents.StopAsync(resourceGroup, accountName, liveEventName, removeOutputsOnStop: true);
          }

          // Delete the LiveEvent
          await client.LiveEvents.DeleteAsync(resourceGroup, accountName, liveEventName);

          deleted = true;
        }
      }
      catch (ApiErrorException e)
      {
        sb.AppendLine("CleanupLiveEventAndOutputAsync -- Hit ApiErrorException");
        sb.AppendLine($"\tCode: {e.Body.Error.Code}");
        sb.AppendLine($"\tCode: {e.Body.Error.Message}");
        sb.AppendLine();
      }

      return deleted;
    }

    private async Task CleanupLocatorandAssetAsync(IAzureMediaServicesClient client, string resourceGroup, string accountName, string streamingLocatorName, string assetName, StringBuilder sb)
    {
      try
      {
        // Delete the Streaming Locator
        await client.StreamingLocators.DeleteAsync(resourceGroup, accountName, streamingLocatorName);

        // Delete the Archive Asset
        await client.Assets.DeleteAsync(resourceGroup, accountName, assetName);
      }
      catch (ApiErrorException e)
      {
        sb.AppendLine("CleanupLocatorandAssetAsync -- Hit ApiErrorException");
        sb.AppendLine($"\tCode: {e.Body.Error.Code}");
        sb.AppendLine($"\tCode: {e.Body.Error.Message}");
        sb.AppendLine();
      }
    }

    #endregion Methods
  }
}
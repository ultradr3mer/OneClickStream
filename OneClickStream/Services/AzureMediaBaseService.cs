using Microsoft.Azure.Management.Media;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using OneClickStream.Configuration;
using System;
using System.Threading.Tasks;

namespace OneClickStream.Services
{
  public class AzureMediaBaseService
  {
    #region Fields

    protected readonly ConfigWrapper config;

    protected string assetName;

    protected string liveEventName;

    protected string liveOutputName;

    protected string streamingEndpointName;

    protected string streamingLocatorName;

    protected string uniqueness;

    #endregion Fields

    #region Constructors

    public AzureMediaBaseService(ConfigWrapper config)
    {
      this.config = config;
    }

    public void InitializeNames(string uniqueness)
    {
      // Creating a unique suffix so that we don't have name collisions if you run the sample
      // multiple times without cleaning up.
      this.uniqueness = uniqueness;
      this.liveEventName = "liveevent-" + this.uniqueness;
      this.assetName = "archiveAsset" + this.uniqueness;
      this.liveOutputName = "liveOutput" + this.uniqueness;
      this.streamingLocatorName = "streamingLocator" + this.uniqueness;
      this.streamingEndpointName = "default";
    }

    /// <summary>
    /// Creates the AzureMediaServicesClient object based on the credentials
    /// supplied in local configuration file.
    /// </summary>
    /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
    /// <returns></returns>
    // <CreateMediaServicesClient>
    protected static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
    {
      ServiceClientCredentials credentials = await GetCredentialsAsync(config);

      return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
      {
        SubscriptionId = config.SubscriptionId,
      };
    }

    private static async Task<ServiceClientCredentials> GetCredentialsAsync(ConfigWrapper config)
    {
      // Use ApplicationTokenProvider.LoginSilentWithCertificateAsync or UserTokenProvider.LoginSilentAsync to get a token using service principal with certificate
      //// ClientAssertionCertificate
      //// ApplicationTokenProvider.LoginSilentWithCertificateAsync

      // Use ApplicationTokenProvider.LoginSilentAsync to get a token using a service principal with symetric key
      ClientCredential clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
      return await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
    }

    #endregion Constructors
  }
}
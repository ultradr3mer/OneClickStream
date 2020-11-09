using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneClickStream.Configuration
{
  public class ConfigWrapper
  {
    #region Fields

    private readonly IConfiguration config;

    #endregion Fields

    #region Constructors

    public ConfigWrapper(IConfiguration config)
    {
      this.config = config;
    }

    #endregion Constructors

    #region Properties

    public string AadClientId => this.config["AadClientId"];

    public Uri AadEndpoint => new Uri(this.config["AadEndpoint"]);

    public string AadSecret => this.config["AadSecret"];

    public string AadTenantId => this.config["AadTenantId"];

    public string AccountName => this.config["AccountName"];

    public Uri ArmAadAudience => new Uri(this.config["ArmAadAudience"]);

    public Uri ArmEndpoint => new Uri(this.config["ArmEndpoint"]);

    public string ConfigFile => this.config["ConfigFile"];

    public string Location => this.config["Location"];

    public string ObsExecutable => this.config["ObsExecutable"];

    public string ResourceGroup => this.config["ResourceGroup"];

    public string SubscriptionId => this.config["SubscriptionId"];

    #endregion Properties
  }
}

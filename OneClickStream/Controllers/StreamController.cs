using Microsoft.AspNetCore.Mvc;
using OneClickStream.GetData;
using OneClickStream.PostData;
using OneClickStream.Properties;
using OneClickStream.Services;
using System.Threading.Tasks;

namespace OneClickStream.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class StreamController : ControllerBase
  {
    #region Fields

    private readonly CleanupService cleanupService;
    private readonly OutputsSerrvice outputsSerrvice;
    private readonly StartupService startupService;

    #endregion Fields

    #region Constructors

    public StreamController(StartupService startupService, OutputsSerrvice outputsSerrvice, CleanupService cleanupService)
    {
      this.startupService = startupService;
      this.outputsSerrvice = outputsSerrvice;
      this.cleanupService = cleanupService;
    }

    #endregion Constructors

    #region Methods

    [HttpPost("Cleanup")]
    public async Task<IActionResult> Cleanup(string id)
    {
      await this.cleanupService.Cleanup(id);
      return this.Ok();
    }

    [HttpPost("CreateOutputs")]
    public async Task<OutputsPostResultData> CreateOutputs(string id)
    {
      return await this.outputsSerrvice.CreateOutputs(id);
    }

    [HttpGet]
    public ContentResult Get(string source)
    {
      string html = Resources.AzureMediaPlayer;
      html = html.Replace("{source}", source);
      return base.Content(html, "text/html");
    }

    [HttpPost("Startup")]
    public async Task<StartupPostResultData> Startup()
    {
      return await this.startupService.Startup();
    }

    #endregion Methods
  }
}
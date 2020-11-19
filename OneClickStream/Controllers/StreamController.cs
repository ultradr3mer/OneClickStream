using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickStream.Data;
using OneClickStream.Properties;
using OneClickStream.Services;
using System.Threading.Tasks;

namespace OneClickStream.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class StreamController : ControllerBase
  {
    #region Fields

    private readonly CleanupService cleanupService;
    private readonly OutputsService outputsSerrvice;
    private readonly StartupService startupService;

    #endregion Fields

    #region Constructors

    public StreamController(StartupService startupService, OutputsService outputsSerrvice, CleanupService cleanupService)
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
    [AllowAnonymous]
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

    [HttpGet("CheckPreview")]
    public async Task<CheckPreviewGetData> CheckPreview(string id)
    {
      return await this.startupService.CheckPreview(id);
    }

    #endregion Methods
  }
}
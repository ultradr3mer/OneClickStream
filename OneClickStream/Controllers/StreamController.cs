using Microsoft.AspNetCore.Mvc;
using OneClickStream.Properties;

namespace OneClickStream.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class StreamController : ControllerBase
  {
    #region Methods

    [HttpGet]
    public ContentResult Get(string source)
    {
      string html = Resources.AzureMediaPlayer;
      html = html.Replace("{source}", source);
      return base.Content(html, "text/html");
    }

    #endregion Methods
  }
}
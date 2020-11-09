namespace OneClickStream.GetData
{
  public class StartupPostResultData
  {
    #region Properties

    public string Id { get; set; }
    public string Log { get; set; }
    public object PreviewSource { get; set; }
    public string IngestUrl { get; internal set; }

    #endregion Properties
  }
}
namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;

public class AbpDtmHttpOptions
{
    public AbpDtmHttpOptions()
    {
        Timeout = 30;
    }
    private string _appUrl;
    private string _publishEventsPath = "/dtm_boxes.DtmHttpService/PublishEvents";
    private string _queryPreparedPath = "/dtm_boxes.DtmHttpService/QueryPrepared";

    /// <summary>
    /// Use this token to invoke action APIs.
    /// </summary>
    /// <returns></returns>
    public string ActionApiToken { get; set; }

    public string AppUrl
    {
        get => _appUrl;
        set => _appUrl = value.RemovePostFix("/");
    }

    /// <summary>
    /// dtm server  address
    /// </summary>
    public string DtmUrl { get; set; }

    /// <summary>
    /// request timeout second default 30
    /// </summary>
    public int Timeout { get; set; }

    public string PublishEventsPath
    {
        get => _publishEventsPath;
        set => _publishEventsPath = value.EnsureStartsWith('/');
    }

    public string QueryPreparedPath
    {
        get => _queryPreparedPath;
        set => _queryPreparedPath = value.EnsureStartsWith('/');
    }

    public string GetPublishEventsAddress()
    {
        return $"{AppUrl}{PublishEventsPath}";
    }

    public string GetQueryPreparedAddress()
    {
        return $"{AppUrl}{QueryPreparedPath}";
    }
}
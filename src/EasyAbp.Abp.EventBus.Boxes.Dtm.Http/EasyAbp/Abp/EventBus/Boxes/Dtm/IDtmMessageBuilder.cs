namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public interface IDtmMessageBuilder
{
    /// <summary>
    /// build dtm message header
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    Task BuildMessageHeaders(Dictionary<string, string> headers);
}
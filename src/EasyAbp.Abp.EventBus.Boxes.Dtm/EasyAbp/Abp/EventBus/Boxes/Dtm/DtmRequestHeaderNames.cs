namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public static class DtmRequestHeaderNames
{
    public static string ActionApiToken { get; set; } = "ActionApiToken";

    public static string DbContextType { get; set; } = "DbContextType";

    public static string TenantId { get; set; } = "TenantId";

    public static string HashedConnectionString { get; set; } = "HashedConnectionString";
}
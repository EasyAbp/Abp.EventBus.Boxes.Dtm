namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public static class OutgoingEventInfoProperties
{
    public static string ConnectionStringName { get; set; } = "CSN";

    public static string TenantId { get; set; } = "TID";

    public static string HashedConnectionString { get; set; } = "HCS";
}
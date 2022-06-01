namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public static class DtmDbProperties
{
    public static string DbTablePrefix { get; set; } = "Dtm";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Dtm";
    public const string ConnectionString2Name = "Dtm2";
}

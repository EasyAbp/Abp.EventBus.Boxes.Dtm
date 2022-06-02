namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

public static class InboxBarrierProperties
{
    public static string Reason { get; set; } = "abp_inbox";
    
    public static string TransType { get; set; } = "abp_inbox";
    
    public static string BranchId { get; set; } = "00";
    
    public static string Op { get; set; } = "abp_inbox";
    
    public static string BarrierId { get; set; } = "01";
}
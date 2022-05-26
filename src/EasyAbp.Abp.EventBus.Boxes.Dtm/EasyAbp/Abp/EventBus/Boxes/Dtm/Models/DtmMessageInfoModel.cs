using Dtmgrpc;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Models;

public class DtmMessageInfoModel
{
    public string Gid { get; set; }
    
    public MsgGrpc Message { get; set; }

    public DtmMessageInfoModel(string gid, MsgGrpc message)
    {
        Gid = gid;
        Message = message;
    }
}
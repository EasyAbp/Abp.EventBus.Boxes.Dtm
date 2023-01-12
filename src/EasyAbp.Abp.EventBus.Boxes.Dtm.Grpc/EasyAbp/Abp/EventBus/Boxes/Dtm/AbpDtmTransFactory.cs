using System;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Distributed.Dtm.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[Dependency(ReplaceServices = true)]
public class AbpDtmTransFactory : IDtmTransFactory, ISingletonDependency
{
    protected IDtmgRPCClient RpcClient { get; }
    protected string ServerUrl { get; }
    protected IBranchBarrierFactory BranchBarrierFactory { get; }

    public AbpDtmTransFactory(
        IDtmgRPCClient rpcClient,
        IOptions<AbpDtmGrpcOptions> options,
        IBranchBarrierFactory branchBarrierFactory)
    {
        RpcClient = rpcClient;
        ServerUrl = options.Value.DtmGrpcUrl.RemovePreFix("http://").RemovePreFix("https://").RemovePostFix("/");
        BranchBarrierFactory = branchBarrierFactory;
    }
    
    public virtual SagaGrpc NewSagaGrpc(string gid)
    {
        throw new NotSupportedException();
    }

    public virtual MsgGrpc NewMsgGrpc(string gid)
    {
        return new MsgGrpc(RpcClient, BranchBarrierFactory, ServerUrl, gid);
    }

    public virtual TccGrpc NewTccGrpc(string gid)
    {
        throw new NotSupportedException();
    }
}
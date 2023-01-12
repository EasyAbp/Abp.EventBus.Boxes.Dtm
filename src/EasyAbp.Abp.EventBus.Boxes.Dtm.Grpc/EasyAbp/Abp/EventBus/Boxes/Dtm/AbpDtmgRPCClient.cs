using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DtmCommon;
using Dtmgrpc;
using EasyAbp.Abp.EventBus.Distributed.Dtm.Options;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[Dependency(ReplaceServices = true)]
public class AbpDtmgRPCClient : IDtmgRPCClient, ISingletonDependency
{
    private static readonly Marshaller<dtmgpb.DtmRequest> DtmRequestMarshaller =
        Marshallers.Create(r => r.ToByteArray(), data => dtmgpb.DtmRequest.Parser.ParseFrom(data));

    private static readonly Marshaller<Empty> DtmReplyMarshaller =
        Marshallers.Create(r => r.ToByteArray(), data => Empty.Parser.ParseFrom(data));

    private static readonly string DtmServiceName = "dtmgimp.Dtm";

    protected IJsonSerializer JsonSerializer { get; }
    protected AbpDtmGrpcOptions GrpcOptions { get; }
    protected AbpDtmEventBoxesOptions BoxesOptions { get; }

    public AbpDtmgRPCClient(
        IJsonSerializer jsonSerializer,
        IOptions<AbpDtmGrpcOptions> grpcOptions,
        IOptions<AbpDtmEventBoxesOptions> boxesOptions)
    {
        JsonSerializer = jsonSerializer;
        GrpcOptions = grpcOptions.Value;
        BoxesOptions = boxesOptions.Value;
    }

    public virtual async Task DtmGrpcCall(TransBase transBase, string operation)
    {
        var dtmRequest = BuildDtmRequest(transBase);

        var method = new Method<dtmgpb.DtmRequest, Empty>(MethodType.Unary, DtmServiceName, operation,
            DtmRequestMarshaller, DtmReplyMarshaller);

        using var channel = GrpcChannel.ForAddress(GrpcOptions.DtmGrpcUrl);

        var callOptions = new CallOptions()
            .WithDeadline(DateTime.UtcNow.AddMilliseconds(BoxesOptions.DtmTimeout));

        await channel.CreateCallInvoker().AsyncUnaryCall(method, string.Empty, callOptions, dtmRequest);
    }

    public virtual Task<string> GenGid()
    {
        throw new NotSupportedException();
    }

    public TransBase TransBaseFromGrpc(ServerCallContext context)
    {
        throw new NotSupportedException();
    }

    public Task RegisterBranch(TransBase tb, string branchId, ByteString bd, Dictionary<string, string> added,
        string operation)
    {
        throw new NotSupportedException();
    }

    public Task<TResponse> InvokeBranch<TRequest, TResponse>(TransBase tb, TRequest msg, string url, string branchId,
        string op) where TRequest : class, IMessage, new() where TResponse : class, IMessage, new()
    {
        throw new NotSupportedException();
    }

    private dtmgpb.DtmRequest BuildDtmRequest(TransBase transBase)
    {
        var transOptions = new dtmgpb.DtmTransOptions
        {
            WaitResult = transBase.WaitResult,
            TimeoutToFail = transBase.TimeoutToFail,
            RetryInterval = transBase.RetryInterval,
        };

        if (transBase.BranchHeaders != null)
        {
            transOptions.BranchHeaders.Add(transBase.BranchHeaders);
        }

        var dtmRequest = new dtmgpb.DtmRequest
        {
            Gid = transBase.Gid,
            TransType = transBase.TransType,
            TransOptions = transOptions,
            QueryPrepared = transBase.QueryPrepared ?? string.Empty,
            CustomedData = transBase.CustomData ?? string.Empty,
            Steps = transBase.Steps == null ? string.Empty : JsonSerializer.Serialize(transBase.Steps),
        };

        foreach (var item in transBase.BinPayloads ?? new List<byte[]>())
        {
            dtmRequest.BinPayloads.Add(ByteString.CopyFrom(item));
        }

        return dtmRequest;
    }
}
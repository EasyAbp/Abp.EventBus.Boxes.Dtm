using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtmcli;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using  System.Net.Http.Headers;
using System.Net.Http.Json;
using Dtmcli.DtmImp;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Http.EasyAbp.Abp.EventBus.Boxes.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IDtmClient))]
public class AbpDtmHttpClient : IDtmClient, ISingletonDependency
{
    protected IJsonSerializer JsonSerializer { get; }
    protected AbpDtmHttpOptions HttpOptions { get; }
    protected AbpDtmEventBoxesOptions BoxesOptions { get; }

    private readonly IHttpClientFactory _httpClientFactory;

    public AbpDtmHttpClient(
        IJsonSerializer jsonSerializer,
        IOptions<AbpDtmHttpOptions> httpOptions,
        IOptions<AbpDtmEventBoxesOptions> boxesOptions, IHttpClientFactory httpClientFactory)
    {
        JsonSerializer = jsonSerializer;
        _httpClientFactory = httpClientFactory;
        HttpOptions = httpOptions.Value;
        BoxesOptions = boxesOptions.Value;
    }
    public Task<string> GenGid(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task TransCallDtm(TransBase tb, object body, string operation, CancellationToken cancellationToken)
    {
        string requestUri ="/api/dtmsvr/" + operation;
        using var response = await _httpClientFactory.CreateClient("dtmClient").PostAsJsonAsync(requestUri, tb, cancellationToken);
        Utils.CheckStatus(response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
    }

    public Task TransRegisterBranch(TransBase tb, Dictionary<string, string> added, string operation, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> TransRequestBranch(TransBase tb, HttpMethod method, object body, string branchID, string op, string url,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public TransBase TransBaseFromQuery(IQueryCollection query)
    {
        throw new NotImplementedException();
    }
}
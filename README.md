# Abp.EventBus.Boxes.Dtm

[![ABP version](https://img.shields.io/badge/dynamic/xml?style=flat-square&color=yellow&label=abp&query=%2F%2FProject%2FPropertyGroup%2FAbpVersion&url=https%3A%2F%2Fraw.githubusercontent.com%2FEasyAbp%2FAbp.EventBus.Boxes.Dtm%2Fmain%2FDirectory.Build.props)](https://abp.io)
[![NuGet](https://img.shields.io/nuget/v/EasyAbp.Abp.EventBus.Boxes.Dtm.svg?style=flat-square)](https://www.nuget.org/packages/EasyAbp.Abp.EventBus.Boxes.Dtm)
[![NuGet Download](https://img.shields.io/nuget/dt/EasyAbp.Abp.EventBus.Boxes.Dtm.svg?style=flat-square)](https://www.nuget.org/packages/EasyAbp.Abp.EventBus.Boxes.Dtm)
[![Discord online](https://badgen.net/discord/online-members/S6QaezrCRq?label=Discord)](https://discord.gg/S6QaezrCRq)
[![GitHub stars](https://img.shields.io/github/stars/EasyAbp/Abp.EventBus.Boxes.Dtm?style=social)](https://www.github.com/EasyAbp/Abp.EventBus.Boxes.Dtm)

The [DTM](https://github.com/dtm-labs/dtm) implementation module of ABP distributed event boxes.

## Introduction

This implementation uses DTM's [2-phase messages](https://en.dtm.pub/practice/msg.html) to support ABP event boxes in the [multi-tenant & multi-database scene](https://github.com/abpframework/abp/issues/10036).

You should see the [DTM docs](https://en.dtm.pub/guide/start.html), which help to understand this module.

## Differences From the ABP's Default Event Boxes

|                        	                         |DTM 2-phase Message Boxes 	 |ABP 5.0+ Default Boxes                                  	 |
|:------------------------------------------------:|:--------------------------:|:--------------------------------------------------------:|
|Timeliness                                      	 |:heavy_check_mark:        	 |:x:                                                     	 |
|Less data transfer                              	 |:x:                       	 |:heavy_check_mark:                                      	 |
|Eventual consistency<br>(transactional UOW)     	 |:heavy_check_mark:        	 |:heavy_check_mark:                                      	 |
|Eventual consistency<br>(non-transactional UOW) 	 |:x:                       	 |:heavy_check_mark:<br>(consumers idempotent required)      |
|Native idempotency                              	 |:heavy_check_mark:        	 |:heavy_check_mark:                                      	 |
|Multi-tenant-database support                   	 |:heavy_check_mark:        	 |:x:                                                     	 |
|No additional external infrastructure           	 |:x:                       	 |:heavy_check_mark:                                      	 |
|Dashboard and Alarm                             	 |:heavy_check_mark:        	 |:x:                                                     	 |

## How Does the DTM Outbox Work?

You are publishing events using the ABP event outbox:
```csharp
await _distributedEventBus.PublishAsync(eto1, useOutbox: true);
await _distributedEventBus.PublishAsync(eto2, useOutbox: true);  // The useOutbox is true by default.
```
The DTM outbox collects them temporarily. Let's see what it will do when you complete the current unit of work:
```CSharp
// Code snippet for UnitOfWork.cs
protected override async Task CommitTransactionsAsync()
{
    // Step 1: inserting a record to the DTM barrier table within the current DB transaction,
    //         and then it sends a "prepare" request to the DTM server.
    await DtmMessageManager.InsertBarriersAndPrepareAsync(EventBag);

    // Step 2: committing the current DB transaction.
    await base.CommitTransactionsAsync();

    // Step 3: sending a "submit" request to the DTM server.
    OnCompleted(async () => await DtmMessageManager.SubmitAsync(EventBag));
}
```
Now, the DTM server has received a "submit" request. It invokes the app's `PublishEvents` service with the events' data, and the latter will publish the events to the MQ provider immediately.

[![](https://mermaid.ink/img/pako:eNqFk89uwjAMxl_FyhleoIdJaOzAAW2I7daL27gQLX-6ONmGEO--tCmjQLX11MQ_f_ni2EdRO0miEEwfkWxNS4U7j6a0kL6ggiZYvq7hOYbKfeddjMHZaCryef3G5OcPD4u2LeBROyZAC4o5Uo6nQAovMWCFTInZo91RJoADhmvu97QCFlLCqsPeWpkwmYSHjW2XloUk0CfZwFnkkn515somkwGwj1fovTq7v05Iiy35T_JFH2g9tejpQubg_MbnCJUTqn1l7pm7ujhjVICwJwgeLWMdlLOD3kDNfwudYY51TSRvJEfWprk_L82xSin_3XkFX0praJRVvO9NZ7_37zCuQNaesp2orpGK_L5Q9400YXtwM2i-xEp35-cWANf0TjrOEDPurlprvbkkdNiQpIwhqVI_6UOm15v5rf604XHlFv3tQTpLE2bH5Br9OyBnTeYmajEThrxBJdMkHrv0UiSDhkpRpF9JDUYdSlHaU0JjPwxPUgXnRdGgZpqJbii3B1uLIvhIZ2iY5oE6_QBrrEwD)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqFk89uwjAMxl_FyhleoIdJaOzAAW2I7daL27gQLX-6ONmGEO--tCmjQLX11MQ_f_ni2EdRO0miEEwfkWxNS4U7j6a0kL6ggiZYvq7hOYbKfeddjMHZaCryef3G5OcPD4u2LeBROyZAC4o5Uo6nQAovMWCFTInZo91RJoADhmvu97QCFlLCqsPeWpkwmYSHjW2XloUk0CfZwFnkkn515somkwGwj1fovTq7v05Iiy35T_JFH2g9tejpQubg_MbnCJUTqn1l7pm7ujhjVICwJwgeLWMdlLOD3kDNfwudYY51TSRvJEfWprk_L82xSin_3XkFX0praJRVvO9NZ7_37zCuQNaesp2orpGK_L5Q9400YXtwM2i-xEp35-cWANf0TjrOEDPurlprvbkkdNiQpIwhqVI_6UOm15v5rf604XHlFv3tQTpLE2bH5Br9OyBnTeYmajEThrxBJdMkHrv0UiSDhkpRpF9JDUYdSlHaU0JjPwxPUgXnRdGgZpqJbii3B1uLIvhIZ2iY5oE6_QBrrEwD)

<details>
<summary>See the more detailed sequence diagram</summary>

[![](https://mermaid.ink/img/pako:eNqtVclu20AM_RVCpwSwfY9QOHDrtDVQow2SIBdfKA1tDzKLOktcI8i_l6Ml8gqkQX2yxEfy8T3O6CUrraAszzz9jmRKmkpcOdQLA_wLMiiC6f0cfsZQ2D8whMc1BpBLCGsOfIbg0Hgsg7QGSqu1DEGaFUgPXtnNdVMGY7Am6oJc8_zgyQ3H40lV5fBFWU-AhlN8pCbOAQ5PMWCBnhizRrOiBgE-YNjHvdHLYSIEzBLsoRIME1y4fXGX0ppCAuiZTPBNkT59r-fMMMkAWMcLdE527PcT-OGO3DO5vA5Ujip01COb4PCA5w5UnKhaK3OMOdKlFry2YseHo977LB-RU5bW8WjKslVBahqNRieyahbfaTsApgBbG2FNjq5hBhs03NaCPCHSp8KNLzYyrGteHjXBt9kU0NfPi2xXpQxcWjsfwLMhl-22qLDDI63SEn3o1D-lw5tXLYV6vL5En9ilDLvp2kwfy5JIdDp3PQ6UOw8-qnu_cywumHwFV5ft-YCvk9mPm2mSSfA6s4oiVkqWvJ6wkmIAzirFO1pg-bTT4ZydM5aORnCfjqUm7nq4Do2AkkumliyMD1IptsCW5D0f1hHM0T0lg1Lnvi0pPpg8zSkH_mngM-r-Vwc_JOp5jxvBPiJ_onDowKmNOTpov2KhpF-3dxPY9oplnGafcEWH6s1v-6QEbROl1iQkD662fcb8dnjY5307P2kmENbQO9ToFqmu7f0yqnaVDHfJBpkmp1EK_ty8pMAiY96ar4Gc_wpaYlRhkS3MK0NjfYHfCBmsy_Il8jIOsvQhuduaMsuDi9SB2k9Wi3r9C14gN5U)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqtVclu20AM_RVCpwSwfY9QOHDrtDVQow2SIBdfKA1tDzKLOktcI8i_l6Ml8gqkQX2yxEfy8T3O6CUrraAszzz9jmRKmkpcOdQLA_wLMiiC6f0cfsZQ2D8whMc1BpBLCGsOfIbg0Hgsg7QGSqu1DEGaFUgPXtnNdVMGY7Am6oJc8_zgyQ3H40lV5fBFWU-AhlN8pCbOAQ5PMWCBnhizRrOiBgE-YNjHvdHLYSIEzBLsoRIME1y4fXGX0ppCAuiZTPBNkT59r-fMMMkAWMcLdE527PcT-OGO3DO5vA5Ujip01COb4PCA5w5UnKhaK3OMOdKlFry2YseHo977LB-RU5bW8WjKslVBahqNRieyahbfaTsApgBbG2FNjq5hBhs03NaCPCHSp8KNLzYyrGteHjXBt9kU0NfPi2xXpQxcWjsfwLMhl-22qLDDI63SEn3o1D-lw5tXLYV6vL5En9ilDLvp2kwfy5JIdDp3PQ6UOw8-qnu_cywumHwFV5ft-YCvk9mPm2mSSfA6s4oiVkqWvJ6wkmIAzirFO1pg-bTT4ZydM5aORnCfjqUm7nq4Do2AkkumliyMD1IptsCW5D0f1hHM0T0lg1Lnvi0pPpg8zSkH_mngM-r-Vwc_JOp5jxvBPiJ_onDowKmNOTpov2KhpF-3dxPY9oplnGafcEWH6s1v-6QEbROl1iQkD662fcb8dnjY5307P2kmENbQO9ToFqmu7f0yqnaVDHfJBpkmp1EK_ty8pMAiY96ar4Gc_wpaYlRhkS3MK0NjfYHfCBmsy_Il8jIOsvQhuduaMsuDi9SB2k9Wi3r9C14gN5U)

[![](https://mermaid.ink/img/pako:eNp1VMtu2zAQ_JUFTy1g6wOEwoFbp62ABjk4QS66rMW1TUQkVXKV1Ajy711Sit_RSdTODmdnSL2pxmtSpYr0tyfX0MLgJqCtHcjDhluCxcMd3Pe88v9gCk9bZDBr4K0UvgMHdBEbNt5B4601zKRhjaYlfTOQYM_e9XZFYVg_RgrT2WzedSX8aH0kQAcmxp6GuhSkvEDGFUYSzBbdhgYEREY-xe3FlTDXGqoEe-w0Jh3oxg_L1DYQaaAXchwHkkP7yZ6VE5EMmOsrDMF8qD9tkMWSwguFMhe6QB0GOiCH4vRM5xFUX2HNzlxiLnzJducgjlIY-UbUdG_0AP45r_7cLiYQfCsByWTN84FcsCmbEu47CpgjHYK8mOd08icU5rUPYlfr3UZOjaWiKK50ZSm_aTcBGQt2voctBbqBCl7RySgezBXjv63C7Mur4W2eNaIl-FUtAGNe1-rYeQUhHeTIECXkr9ed26c7bpDFH4R-YuHYFfumIdLHqZz5cR34qX2VSKUCHtLFsiRBnkcqR38MIjkhUiObtpWRfUMxGrcp4A7DczIk5ZpCVRNlKVg0Wq72WxJQK2G1YlApr5rW2Ldcq9q9C7TP1-VWG_ZBlWtsI01UurbLnWtUyaGnD9D4exhR7_8BefBo_w)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNp1VMtu2zAQ_JUFTy1g6wOEwoFbp62ABjk4QS66rMW1TUQkVXKV1Ajy711Sit_RSdTODmdnSL2pxmtSpYr0tyfX0MLgJqCtHcjDhluCxcMd3Pe88v9gCk9bZDBr4K0UvgMHdBEbNt5B4601zKRhjaYlfTOQYM_e9XZFYVg_RgrT2WzedSX8aH0kQAcmxp6GuhSkvEDGFUYSzBbdhgYEREY-xe3FlTDXGqoEe-w0Jh3oxg_L1DYQaaAXchwHkkP7yZ6VE5EMmOsrDMF8qD9tkMWSwguFMhe6QB0GOiCH4vRM5xFUX2HNzlxiLnzJducgjlIY-UbUdG_0AP45r_7cLiYQfCsByWTN84FcsCmbEu47CpgjHYK8mOd08icU5rUPYlfr3UZOjaWiKK50ZSm_aTcBGQt2voctBbqBCl7RySgezBXjv63C7Mur4W2eNaIl-FUtAGNe1-rYeQUhHeTIECXkr9ed26c7bpDFH4R-YuHYFfumIdLHqZz5cR34qX2VSKUCHtLFsiRBnkcqR38MIjkhUiObtpWRfUMxGrcp4A7DczIk5ZpCVRNlKVg0Wq72WxJQK2G1YlApr5rW2Ldcq9q9C7TP1-VWG_ZBlWtsI01UurbLnWtUyaGnD9D4exhR7_8BefBo_w)
   
</details>

> If you are still confused about how it ensures eventual consistency, see DTM's [2-phase messages doc](https://en.dtm.pub/practice/msg.html) for more information.

## How Does the DTM Inbox Work?

Unlike ABP's default implementation, the DTM inbox gets an event from MQ and handles it at once. After the handlers finish their work, the inbox inserts a barrier within the current DB transaction. Finally, it commits the transaction and returns ACK to MQ.

All the incoming events have a unique MessageId. Events with the same MessageId only are handled once since we cannot insert a barrier with a duplicate gid (MessageId).

[![](https://mermaid.ink/img/pako:eNp9UstuwjAQ_JWVz_ADUUtFAakI5YDaYy6beAmW4jW115QK8e91HgWh0vhke2dmZ0d7VpXTpDIV6DMSV7Q0WHu0BUM6YqQhWH7ksObSnfpPjOI42pJ8_86309nsislgSY05kgdkoCOxPJV-9mVkDwiRTeoCOYWANa11L3DltjooWGKgDFYnEyRxSvTeJLlOojYanm_8l0FgIE3vjbwOTHYCOxf5Qbv54XAzLHvqHfe4VLsz9IasUxpXVBpQQ8AjQbVHrin8NdPJL_pqB9U36Xuvg3iIVUWkaTSZNQfybTRteSye0XQe2HrcbuGsNdLNLR45YCXG8bh2T_l3moTOtxnMFxs1UZa8RaPTDp5bXKFSJ0uFytJV0w5jI4Uq-JKg8aBRaKWNOK-yHTaBJqrdx_dvrlQmPtIvaNjjAXX5AUn_9To)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNp9UstuwjAQ_JWVz_ADUUtFAakI5YDaYy6beAmW4jW115QK8e91HgWh0vhke2dmZ0d7VpXTpDIV6DMSV7Q0WHu0BUM6YqQhWH7ksObSnfpPjOI42pJ8_86309nsislgSY05kgdkoCOxPJV-9mVkDwiRTeoCOYWANa11L3DltjooWGKgDFYnEyRxSvTeJLlOojYanm_8l0FgIE3vjbwOTHYCOxf5Qbv54XAzLHvqHfe4VLsz9IasUxpXVBpQQ8AjQbVHrin8NdPJL_pqB9U36Xuvg3iIVUWkaTSZNQfybTRteSye0XQe2HrcbuGsNdLNLR45YCXG8bh2T_l3moTOtxnMFxs1UZa8RaPTDp5bXKFSJ0uFytJV0w5jI4Uq-JKg8aBRaKWNOK-yHTaBJqrdx_dvrlQmPtIvaNjjAXX5AUn_9To)


<details>
<summary>See the more detailed sequence diagram</summary>

[![](https://mermaid.ink/img/pako:eNqNlMFO6zAQRX_F8hp-IOIVQcsTFSoSgmU2E3uaWjjjYo9LEeLfceKQtFBaskriM3du5tp5l8pplIUM-BKRFM4M1B6akkS62LBFMXtaiDlVbptfQmRHsanQ5-fFw_lkMjCFmKE1G_QCSOAGiS8qP3k1vBIgIpnURSwwBKhxrrPAUNvqAEMFAQtxszWBU00F3psk10nURot_Y_1l78hyxsNAfwMz1zXrG5zvm77u65Yu0i49WEv44qEQV9O7vIpf2AlBcrwruvutV-v1OC1eYR5X5tLa3jRugXSKYqDSdLUIsEGhVkA1hp9mOvlpXu1QPUrve-3FQ1QKUePRWOYU0Le5tMvHshmjmWFQ3lSGasG45ZNZ_Adjs6SOa2sUMLbih1LZdeadtcmReu6mxB4ogGLj6GTDp5HtVFB3Oke2wf2f98GBAA7bn7qmMXzY-2_aueTX3MY9K89kg74Bo9NRf2-5UqZODZaySLcalxAtl7Kkj4TGtU4zv9GGnZfFEmzAM9ke-8c3UrJgH_EL6n8XPfXxCdjtY4c)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqNlMFO6zAQRX_F8hp-IOIVQcsTFSoSgmU2E3uaWjjjYo9LEeLfceKQtFBaskriM3du5tp5l8pplIUM-BKRFM4M1B6akkS62LBFMXtaiDlVbptfQmRHsanQ5-fFw_lkMjCFmKE1G_QCSOAGiS8qP3k1vBIgIpnURSwwBKhxrrPAUNvqAEMFAQtxszWBU00F3psk10nURot_Y_1l78hyxsNAfwMz1zXrG5zvm77u65Yu0i49WEv44qEQV9O7vIpf2AlBcrwruvutV-v1OC1eYR5X5tLa3jRugXSKYqDSdLUIsEGhVkA1hp9mOvlpXu1QPUrve-3FQ1QKUePRWOYU0Le5tMvHshmjmWFQ3lSGasG45ZNZ_Adjs6SOa2sUMLbih1LZdeadtcmReu6mxB4ogGLj6GTDp5HtVFB3Oke2wf2f98GBAA7bn7qmMXzY-2_aueTX3MY9K89kg74Bo9NRf2-5UqZODZaySLcalxAtl7Kkj4TGtU4zv9GGnZfFEmzAM9ke-8c3UrJgH_EL6n8XPfXxCdjtY4c)

</details>

> As you may have noticed, the inbox has nothing to do with the DTM Server.🤭

## Installation

1. Ensure you are NOT using the [CAP bus](https://github.com/EasyAbp/Abp.EventBus.CAP) and have installed another [bus provider](https://docs.abp.io/en/abp/latest/Distributed-Event-Bus#providers).

1. Install the following NuGet packages. ([see how](https://github.com/EasyAbp/EasyAbpGuide/blob/master/docs/How-To.md#add-nuget-packages))

    * EasyAbp.Abp.EventBus.Boxes.Dtm.Grpc
    * EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFramework
    * EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB

1. Add `DependsOn(typeof(AbpEventBusBoxesDtmXxxModule))` attribute to configure the module dependencies. ([see how](https://github.com/EasyAbp/EasyAbpGuide/blob/master/docs/How-To.md#add-module-dependencies))

1. Configure the boxes and gRPC.
```CSharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    // See https://docs.abp.io/en/abp/latest/Distributed-Event-Bus#additional-configuration
    Configure<AbpDistributedEventBusOptions>(options =>
    {
        options.Outboxes.Configure(config =>
        {
            config.UseDbContextWithDtmOutbox<MyProjectDbContext>();
        });

        options.Inboxes.Configure(config =>
        {
            config.UseDbContextWithDtmInbox<MyProjectDbContext>();
        });
    });

    // Use `AddDtmOutbox` and `AddDtmInbox` separately if you only need one of them.
    context.Services.AddDtmBoxes();

    context.Services.AddGrpc();
    context.Services.AddAbpDtmGrpc(options =>
    {
        options.ActionApiToken = "1q2w3e";  // DTM Server invokes app's action APIs with this token for authorization.
        options.AppGrpcUrl = "http://127.0.0.1:54358";  // Base URL for DTM Server to invoke the current app. Only HTTP now!
        options.DtmGrpcUrl = "http://127.0.0.1:36790";  // Base URL for the current app to invoke DTM Server.
    });
}

public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    app.UseConfiguredEndpoints(endpoints =>
    {
        endpoints.MapAbpDtmGrpcService();
    });
}
```

## Usage

1. Install and run the DTM Server. See https://en.dtm.pub/guide/install.html.

1. Ensure the intranet endpoint (you configured above) is available since the DTM Server uses it to invoke your app.

1. The DTM event boxes should work now.

## Roadmap

- [ ] Barrier tables/collections auto clean up.

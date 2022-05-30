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

|                                              	| DTM 2-phase Message Boxes 	| ABP 5.0+ Default Boxes 	|
|----------------------------------------------	|---------------------------	|------------------------	|
| Timeliness                                   	| :heavy_check_mark:        	| :x:                    	|
| Less data transfer                           	| :x:                       	| :heavy_check_mark:     	|
| Eventual consistency (transactional UOW)     	| :heavy_check_mark:        	| :heavy_check_mark:     	|
| Eventual consistency (non-transactional UOW) 	| :x:                       	| :x:                    	|
| Native idempotency                           	| :heavy_check_mark:        	| :heavy_check_mark:     	|
| Multi-tenant-database support                	| :heavy_check_mark:        	| :x:                    	|
| No additional external infrastructure        	| :x:                       	| :heavy_check_mark:     	|
| Dashboard and Alarm                          	| :heavy_check_mark:        	| :x:                    	|

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

[![](https://mermaid.ink/img/pako:eNqFk89u2zAMxl-F0Dl5AWMIkKU7BEPQFllvvtAWnQjVH0-UuhZF372ypCxpkrY-2eSPnz8S5KvonSTRCKa_kWxPNwp3Hk1rIT1BBU1w82cDtzF07rlEMQZno-nIl-8HJj9fLJbj2MBKOyZAC4o5UsmnREofVRpYSgnrCXgYJQaSqUD-6Pwix7YhhVZ7tDvKQaAnsoEhOMDsxfDugzAG7JCpgbVNTkKlOvReHSxeWvhZ0rWGZOGORMW35J_INzkxehrR05EsyfmZ8gl6qXqY0yV03szKGaMChD1B8GgZ-6CcrYKV-i9X4GmSHPuezjRPzH0Cftk3xy7VfNf27e8ZrOGf0hoGZRXvs_Ximr-cQ9G_aj5h03Y1ZVugn7brivXqqGrexU5P_69744bsJG8OMePuw1pu7o8FE1aLlDEkVVpE_VLozf38TP8Tv6fTW-buQTpLV8yekhv0j4BcFJmHqMVMGPIGlUzX-TqVtyIZNNSKJr1KGjDq0IrWviU05kP6JVVwXjQDaqaZmA51-2J70QQf6QDVC6_U2zu02VHa)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqFk89u2zAMxl-F0Dl5AWMIkKU7BEPQFllvvtAWnQjVH0-UuhZF372ypCxpkrY-2eSPnz8S5KvonSTRCKa_kWxPNwp3Hk1rIT1BBU1w82cDtzF07rlEMQZno-nIl-8HJj9fLJbj2MBKOyZAC4o5UsmnREofVRpYSgnrCXgYJQaSqUD-6Pwix7YhhVZ7tDvKQaAnsoEhOMDsxfDugzAG7JCpgbVNTkKlOvReHSxeWvhZ0rWGZOGORMW35J_INzkxehrR05EsyfmZ8gl6qXqY0yV03szKGaMChD1B8GgZ-6CcrYKV-i9X4GmSHPuezjRPzH0Cftk3xy7VfNf27e8ZrOGf0hoGZRXvs_Ximr-cQ9G_aj5h03Y1ZVugn7brivXqqGrexU5P_69744bsJG8OMePuw1pu7o8FE1aLlDEkVVpE_VLozf38TP8Tv6fTW-buQTpLV8yekhv0j4BcFJmHqMVMGPIGlUzX-TqVtyIZNNSKJr1KGjDq0IrWviU05kP6JVVwXjQDaqaZmA51-2J70QQf6QDVC6_U2zu02VHa)

<details>
<summary>See the more detailed sequence diagram</summary>

[![](https://mermaid.ink/img/pako:eNqFVMtu2zAQ_JUFTy1g-wOEwoFju62ABjk4QS6-rMW1TEQkVXKV1Ajy713qETt-IDpJ3OFwdmapN1V4TSpTkf425ApaGCwD2rUDedhwRbB4uIP7hjf-H4zhaYcMZgu8k8ItcEAXsWDjHRTeWsNMGrZoKtI3HQk27F1jNxS678dIYTydzuo6g3nlIwE6MDE21NWlIOXDoRnMtIY8AR5rjYkfnf6xCdN2bcWyNN-hK6ldBHohxxHYA7bSbSw74gPleDh_2WFRa9KfTkfGDUbKIHcil3uqDYZghj7Odd525X7PwHh0bAdfUXihkLWFOlCNgQ7Irjg-YT6C6qvNnINOm5m3CbXZHQXXE_aoD7r5R5w_Z_mf5WKU3A2-kmTFiOL5cITsSKFmcF9TwHYWugk4a-uzAU8oYrY-iLuVd6WMm6XJZHJhVyvoN-1HIM3B3jewo0A3kMMrOk5Zmws5wbdXw7u23YiW4Fe-AIzt91odu68gpPGPDFHm4fsXkzCwJ-UHlVdcHGYBYlMUdBzMiRmXgVe9y0UqTeAhXUdLkuVpqnKl-hRSbiI1sqkqadkXFKNx5QTuMDwnQ1KoKVE1UpaCRaPlh_CWBKyVsFoxKJNXTVtsKl6rtXsXaNNexqU27IPKtlhFGql02Vd7V6iMQ0MDqP-p9Kj3_09bed4)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqFVMtu2zAQ_JUFTy1g-wOEwoFju62ABjk4QS6-rMW1TEQkVXKV1Ajy713qETt-IDpJ3OFwdmapN1V4TSpTkf425ApaGCwD2rUDedhwRbB4uIP7hjf-H4zhaYcMZgu8k8ItcEAXsWDjHRTeWsNMGrZoKtI3HQk27F1jNxS678dIYTydzuo6g3nlIwE6MDE21NWlIOXDoRnMtIY8AR5rjYkfnf6xCdN2bcWyNN-hK6ldBHohxxHYA7bSbSw74gPleDh_2WFRa9KfTkfGDUbKIHcil3uqDYZghj7Odd525X7PwHh0bAdfUXihkLWFOlCNgQ7Irjg-YT6C6qvNnINOm5m3CbXZHQXXE_aoD7r5R5w_Z_mf5WKU3A2-kmTFiOL5cITsSKFmcF9TwHYWugk4a-uzAU8oYrY-iLuVd6WMm6XJZHJhVyvoN-1HIM3B3jewo0A3kMMrOk5Zmws5wbdXw7u23YiW4Fe-AIzt91odu68gpPGPDFHm4fsXkzCwJ-UHlVdcHGYBYlMUdBzMiRmXgVe9y0UqTeAhXUdLkuVpqnKl-hRSbiI1sqkqadkXFKNx5QTuMDwnQ1KoKVE1UpaCRaPlh_CWBKyVsFoxKJNXTVtsKl6rtXsXaNNexqU27IPKtlhFGql02Vd7V6iMQ0MDqP-p9Kj3_09bed4)

[![](https://mermaid.ink/img/pako:eNqtVdtu2zAM_RXCTy2Q5H3G0CJtsi3Agq1Ii77khbaYRKgsebq0C4r--yhfZtdJiq5YnmLxcshzSOk5yY2gJE0c_Qqkc5pJ3Fos1hr456VXBLPbJfwIPjO_YQz3O_QgN-B3bLgCb1E7zL00GnJTFNJ7qbcgHThlni7rNBi80aHIyNbfd47s-OJiWpYpXCvjCFBziAtU29nA5g42hakQsIgOd6VAT4IDxOfMXlRnK89H1zvUW6oOgR5JewfeAFbFF25bJ-5Sjlv8ee2LQpB4hY4eM3SUwkJzub5JlaG1su3jsM6r2tzEtBl7sLX7iuwj2bQylJZKtNR51sbxIHPPVZxs5tBp2Mx1pVGlXk-6A_DXZd4jh2yMZQ6UYXW9LGgymRyJqqr4RvsRcAmwNwF2ZOkSFvCE2kdF5BE24exJ-l1VlMOC4OtiBuiq73XS5ygBG8fUeXCs2nkzXcr3ioijt0HnW43eVLTFj711KbrANuQvwa2u4EKeU0tyizGg7bTzQd7b3hqdSV21vvJUwqfzdq04z5fp4vt8BiJQZFKEUsmcRx-2UozAGqXYJ8P8oQd0StIFM0gTuI3bXBCDD0ei5lGqeqWYH-elUqyEyck53vEJLNE-RJ0icgdLiveZmzomxAf7Pkr2fxb037l9S_GauY_oENkeStH0fzJh1cnPkCnpdu3lZ5orOl5_LBhuaUjf8qYLiq5NoCwKEpJbV_suYnkzHuC8cwemdQfCaHoHG-1EVZmd2wTVzJRmlGSUFGQLlIKfq-doWCdcd8HXQsp_BW0wKL9O1vqFXUP1UMyF9MYm6QZ5KkdJfIhWe50nqbeBWqfmyWu8Xv4AnJ9LYA)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqtVdtu2zAM_RXCTy2Q5H3G0CJtsi3Agq1Ii77khbaYRKgsebq0C4r--yhfZtdJiq5YnmLxcshzSOk5yY2gJE0c_Qqkc5pJ3Fos1hr456VXBLPbJfwIPjO_YQz3O_QgN-B3bLgCb1E7zL00GnJTFNJ7qbcgHThlni7rNBi80aHIyNbfd47s-OJiWpYpXCvjCFBziAtU29nA5g42hakQsIgOd6VAT4IDxOfMXlRnK89H1zvUW6oOgR5JewfeAFbFF25bJ-5Sjlv8ee2LQpB4hY4eM3SUwkJzub5JlaG1su3jsM6r2tzEtBl7sLX7iuwj2bQylJZKtNR51sbxIHPPVZxs5tBp2Mx1pVGlXk-6A_DXZd4jh2yMZQ6UYXW9LGgymRyJqqr4RvsRcAmwNwF2ZOkSFvCE2kdF5BE24exJ-l1VlMOC4OtiBuiq73XS5ygBG8fUeXCs2nkzXcr3ioijt0HnW43eVLTFj711KbrANuQvwa2u4EKeU0tyizGg7bTzQd7b3hqdSV21vvJUwqfzdq04z5fp4vt8BiJQZFKEUsmcRx-2UozAGqXYJ8P8oQd0StIFM0gTuI3bXBCDD0ei5lGqeqWYH-elUqyEyck53vEJLNE-RJ0icgdLiveZmzomxAf7Pkr2fxb037l9S_GauY_oENkeStH0fzJh1cnPkCnpdu3lZ5orOl5_LBhuaUjf8qYLiq5NoCwKEpJbV_suYnkzHuC8cwemdQfCaHoHG-1EVZmd2wTVzJRmlGSUFGQLlIKfq-doWCdcd8HXQsp_BW0wKL9O1vqFXUP1UMyF9MYm6QZ5KkdJfIhWe50nqbeBWqfmyWu8Xv4AnJ9LYA)
   
</details>

If you are still confused about how it ensures eventual consistency, see DTM's [2-phase messages doc](https://en.dtm.pub/practice/msg.html) for more information.

## How Does the DTM Inbox Work?

Todo.

## Installation

1. Install the following NuGet packages. ([see how](https://github.com/EasyAbp/EasyAbpGuide/blob/master/docs/How-To.md#add-nuget-packages))

    * EasyAbp.Abp.EventBus.Boxes.Dtm.Grpc
    * EasyAbp.Abp.EventBus.Boxes.Dtm.EntityFramework
    * EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB

1. Add `DependsOn(typeof(AbpEventBusBoxesDtmXxxModule))` attribute to configure the module dependencies. ([see how](https://github.com/EasyAbp/EasyAbpGuide/blob/master/docs/How-To.md#add-module-dependencies))

1. Configure the module and gRPC.
```CSharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
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

## Roadmap

Todo.

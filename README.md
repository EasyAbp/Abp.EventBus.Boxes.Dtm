# Abp.EventBus.Boxes.Dtm
The [DTM](https://github.com/dtm-labs/dtm) implementation module of ABP distributed event boxes.

## Introduction

This implementation uses DTM's [2-phase messages](https://en.dtm.pub/practice/msg.html) to support ABP event boxes in the [multi-tenant & multi-database scene](https://github.com/abpframework/abp/issues/10036).

You should see the [DTM docs](https://en.dtm.pub/guide/start.html), which will help to understand this module.

## Differences From the ABP's Default Event Boxes

|                                              	| DTM 2-phase Message Boxes 	| ABP 5.0+ Default Boxes 	|
|----------------------------------------------	|---------------------------	|------------------------	|
| Timeliness                                   	| :heavy_check_mark:        	| :x:                    	|
| Less data transfer                           	| :x:                       	| :heavy_check_mark:     	|
| Eventual consistency (transactional UOW)     	| :heavy_check_mark:        	| :heavy_check_mark:     	|
| Eventual consistency (non-transactional UOW) 	| :x:                       	| :heavy_check_mark:     	|
| Native idempotency                           	| :heavy_check_mark:        	| :heavy_check_mark:     	|
| Multi-tenant-database support                	| :heavy_check_mark:        	| :x:                    	|
| No additional external infrastructure        	| :x:                       	| :heavy_check_mark:     	|
| Dashboard and Alarm                          	| :heavy_check_mark:        	| :x:                    	|

## How Does the DTM Outbox Works?

You are publishing events using the ABP event outbox:
```csharp
await _distributedEventBus.PublishAsync(eto1, useOutbox: true);
await _distributedEventBus.PublishAsync(eto2, useOutbox: true);  // The useOutbox is true by default.
```
The DTM outbox collects them temporarily. Let's see what it will do when you complete the current unit of work:
```csharp
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

[![](https://mermaid.ink/img/pako:eNqFVMtu2zAQ_JUFz_YPCIUBI-nBKAw3cI1cfFmJK5soHyqXTBsE-feSFF2rju3oRO3OjmYWQ72JzkkSjWD6Fcl29Kjw4NHsLaQnqKAJHn-sYRND6_6MVYzB2Wha8uP7jsnPF4vlMDTwoB0ToAXFHGnsp0Zqn1kaWEoJqwzYDRIDyTQgv7R-UWrbkEoPR7QHKkWgF7KBITjAosXwYUq82zyn7zozaAoE4UjQRe_TCKROVbh5zgowYItMDaxskhwqXYveq39eRuREaz4Pngb01c65WZFb8i_kbyDH5vw2qfzAevL0EXTpI7k2KhTPwaNl7IJythJW1HyyogTO2-bYdXTBORF3A3jXN8c2zXxme_NtBiv4rbSGXlnFxyJ9VM139zDyXxV_it4dWA5KZuMst4QMuhzUKw6r8Mr5PbY6y6wRdH0RXEJIzHj4L-Hrp_NAhtUhZQxJlTKtX0f0-ml-wX9D73TJy7IkkM7SFbFT5Br9T0AeGZn7qMVMGPIGlUwX_S2P70USaGgvmnSU1GPUYS_29j1BY7mTX6UKzoumR800E_nOb19tJ5rgI51A9WdRUe9_AUnvaEg)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqFVMtu2zAQ_JUFz_YPCIUBI-nBKAw3cI1cfFmJK5soHyqXTBsE-feSFF2rju3oRO3OjmYWQ72JzkkSjWD6Fcl29Kjw4NHsLaQnqKAJHn-sYRND6_6MVYzB2Wha8uP7jsnPF4vlMDTwoB0ToAXFHGnsp0Zqn1kaWEoJqwzYDRIDyTQgv7R-UWrbkEoPR7QHKkWgF7KBITjAosXwYUq82zyn7zozaAoE4UjQRe_TCKROVbh5zgowYItMDaxskhwqXYveq39eRuREaz4Pngb01c65WZFb8i_kbyDH5vw2qfzAevL0EXTpI7k2KhTPwaNl7IJythJW1HyyogTO2-bYdXTBORF3A3jXN8c2zXxme_NtBiv4rbSGXlnFxyJ9VM139zDyXxV_it4dWA5KZuMst4QMuhzUKw6r8Mr5PbY6y6wRdH0RXEJIzHj4L-Hrp_NAhtUhZQxJlTKtX0f0-ml-wX9D73TJy7IkkM7SFbFT5Br9T0AeGZn7qMVMGPIGlUwX_S2P70USaGgvmnSU1GPUYS_29j1BY7mTX6UKzoumR800E_nOb19tJ5rgI51A9WdRUe9_AUnvaEg)

<details>
<summary>See the more detailed sequence diagram</summary>

[![](https://mermaid.ink/img/pako:eNp1VNuO2jAQ_ZWRn1oJ-ICoYkVht43UigcW8cLLkAxgNbZTX3aLVvvvHTtmk-WSp8QzPj6Xcd5EZWoShXD0N5CuaCHxYFFtNfDjpW8IFs-_YRn8zvyDMWyO6EHuwR-58B28Re2w8tJoqIxS0nuqYY-yofqhA8HgjQ5qR7b7Xjuy4-l01rYFzBvjCFCDdC5QV-cCl_tDC5jVNZSxYd3WGPFR1992dprWVp6X5kfUB0qLQC-kvQNvABN15Q5D4PVyw-ca1TbkKcmogrW8BbiSGS43kQF63KGjAkrNlH2G26G18kNL1zngGt9bSy3aLKcv5s4V2Reydzq74vg-aH2FetZ03XSpY54CSpoHuWXA3DUeWJTTfJqVvx4Xo2iuNQ0Hyx5Uf_ojPsK82NG7HjFd1LxsyWIalm5EroR_tmiDTHdvLFvfGH3geVQ0mUxu7EoEftJpBCwfTibAkSw9QAmvyNHyMMgbIcKXV-mPyRCHiuBHuQB06XsrhvkIsPF-OA-OJ-Xrp0G9HpMzemTes7z2OZHudrFpLlQVnS3Jrg3NuN1417uSqdIEnuN9VcRpX-Ye85QuBwFM1XnZNCzZVOSc1AcxEoqsQlnz_-EtHrcVjKHYjoJfa9pjaPxWbPU7t4Z0Nx9r6Y0VxR4bRyMR7_7qpCtReBvo3JT_Mbnr_T_FF3xS)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNp1VNuO2jAQ_ZWRn1oJ-ICoYkVht43UigcW8cLLkAxgNbZTX3aLVvvvHTtmk-WSp8QzPj6Xcd5EZWoShXD0N5CuaCHxYFFtNfDjpW8IFs-_YRn8zvyDMWyO6EHuwR-58B28Re2w8tJoqIxS0nuqYY-yofqhA8HgjQ5qR7b7Xjuy4-l01rYFzBvjCFCDdC5QV-cCl_tDC5jVNZSxYd3WGPFR1992dprWVp6X5kfUB0qLQC-kvQNvABN15Q5D4PVyw-ca1TbkKcmogrW8BbiSGS43kQF63KGjAkrNlH2G26G18kNL1zngGt9bSy3aLKcv5s4V2Reydzq74vg-aH2FetZ03XSpY54CSpoHuWXA3DUeWJTTfJqVvx4Xo2iuNQ0Hyx5Uf_ojPsK82NG7HjFd1LxsyWIalm5EroR_tmiDTHdvLFvfGH3geVQ0mUxu7EoEftJpBCwfTibAkSw9QAmvyNHyMMgbIcKXV-mPyRCHiuBHuQB06XsrhvkIsPF-OA-OJ-Xrp0G9HpMzemTes7z2OZHudrFpLlQVnS3Jrg3NuN1417uSqdIEnuN9VcRpX-Ye85QuBwFM1XnZNCzZVOSc1AcxEoqsQlnz_-EtHrcVjKHYjoJfa9pjaPxWbPU7t4Z0Nx9r6Y0VxR4bRyMR7_7qpCtReBvo3JT_Mbnr_T_FF3xS)

[![](https://mermaid.ink/img/pako:eNqtVctu2zAQ_JWFTglg-16hcODGaWugRho4gS--UOLaJkKRKrlMagT59y4pqVb9KNKgOkna58zski9ZaSVmeebxR0BT4lSJjRPVygA_pEgjTO_ncBuosD9hCMutIFBroC0bPgE5YbwoSVkDpa0qRaTMBpQHr-3zVZNGBLImVAW65vvBoxuOx5O6zuFaW48gDIf4gI2dDWzel81hIiXMosNDLQWh5AD5sXDj9G9B_Ot6K8wG00_AJzTkgSyI1HzlN_3ED7dLrmurWiNhAlIG5zgE2NJ2eLuMHQgShfCYw8xwy9SmK4Rz6jeWxrPXa3yvHdbCtXD2xtZzge4J3RnPxjg8n1QeZe0wHTsd4rhOEiXMPeWOiv_Z5lJwyNo6hq8ti0uqwtFodCIqKfoVdwPgFmBnA2zR4RXM4FkwvSyIOkEkXDwr2qamvKgQvsymIHz6XmV9jjJwcUo9gWe1Ltvh0tRrIk7eWnjq5OmN07GYXf2IbZ9iH9iFDDtoTSSPnw9liR3JXY0D2s47H-W9723RhTIJ-oKwhg-X3VZxns-T2bebKciAkUkZaq1KnnzYKDkAZ7Vmn0KUj71C5ySdMYM4gvu4zBVy8cORiHuUqFQxK_PjSWnNStgSvecVb4qg5uVlCKdofyfKk9T-Z_n-ncm_6duQ9B7WI8sHxHf4UY7OZExQvodCK7_tjjrbHsjxsGN9xAYP-Zvf7YOiaxuoqgqlYux6t4-Y3w0P6rxx5CcNBGkNvoGOuXCPcdFTZu_XQbdDZbhKNsgqdJVQki-nl2hYZdx3xadAzq8S1yJoWmUr88quIV0LN1KRdVm-FjyWgyxeO4udKbOcXMDOqb3gWq_XX2UFRCc)](https://mermaid-js.github.io/mermaid-live-editor/edit#pako:eNqtVctu2zAQ_JWFTglg-16hcODGaWugRho4gS--UOLaJkKRKrlMagT59y4pqVb9KNKgOkna58zski9ZaSVmeebxR0BT4lSJjRPVygA_pEgjTO_ncBuosD9hCMutIFBroC0bPgE5YbwoSVkDpa0qRaTMBpQHr-3zVZNGBLImVAW65vvBoxuOx5O6zuFaW48gDIf4gI2dDWzel81hIiXMosNDLQWh5AD5sXDj9G9B_Ot6K8wG00_AJzTkgSyI1HzlN_3ED7dLrmurWiNhAlIG5zgE2NJ2eLuMHQgShfCYw8xwy9SmK4Rz6jeWxrPXa3yvHdbCtXD2xtZzge4J3RnPxjg8n1QeZe0wHTsd4rhOEiXMPeWOiv_Z5lJwyNo6hq8ti0uqwtFodCIqKfoVdwPgFmBnA2zR4RXM4FkwvSyIOkEkXDwr2qamvKgQvsymIHz6XmV9jjJwcUo9gWe1Ltvh0tRrIk7eWnjq5OmN07GYXf2IbZ9iH9iFDDtoTSSPnw9liR3JXY0D2s47H-W9723RhTIJ-oKwhg-X3VZxns-T2bebKciAkUkZaq1KnnzYKDkAZ7Vmn0KUj71C5ySdMYM4gvu4zBVy8cORiHuUqFQxK_PjSWnNStgSvecVb4qg5uVlCKdofyfKk9T-Z_n-ncm_6duQ9B7WI8sHxHf4UY7OZExQvodCK7_tjjrbHsjxsGN9xAYP-Zvf7YOiaxuoqgqlYux6t4-Y3w0P6rxx5CcNBGkNvoGOuXCPcdFTZu_XQbdDZbhKNsgqdJVQki-nl2hYZdx3xadAzq8S1yJoWmUr88quIV0LN1KRdVm-FjyWgyxeO4udKbOcXMDOqb3gWq_XX2UFRCc)
   
</details>

If you are still confused about how it ensures the eventual consistency, see DTM's [2-phase messages doc](https://en.dtm.pub/practice/msg.html) for more information.

## How Does the DTM Inbox Works?

Todo.

## Installation

Todo.

## Usage

Todo.

## Roadmap

Todo.

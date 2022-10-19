using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blocto.Sdk.Core.Extension
{
    public static class UniversalLinkExtension
    {
        // private static List<string> _keywords = new List<string>()
        //                                         {
        //                                             "request_id",
        //                                             "address",
        //                                             "signature",
        //                                             "tx_hash"
        //                                         };
        // public static UniversalLinkResponse ParseRequestAccountDeeplink(this string universallink)
        // {
        //     var content = universallink.Split("?");
        //     var tmp = content[1].Split("&");
        //     var response = new UniversalLinkResponse
        //                    {
        //                        RequestId = Guid.Parse(tmp[0].Split("=")[1]),
        //                        Address = tmp[1].Split("=")[1],
        //                        IsInitStatus = false
        //                    };
        //     
        //     return response;
        // }
        //
        // public static UniversalLinkResponse ParseSignMessageDeeplink(this string universallink)
        // {
        //     var content = universallink.Split("?");
        //     var tmp = content[1].Split("&");
        //     var response = new UniversalLinkResponse
        //                    {
        //                        RequestId = Guid.Parse(tmp[0].Split("=")[1]),
        //                        Signature =tmp[1].Split("=")[1],
        //                        IsInitStatus = false
        //                    };
        //     
        //     return response;
        // }
        //
        // public static UniversalLinkResponse ParseSendTransactionDeeplink(this string universallink)
        // {
        //     var content = universallink.Split("?");
        //     var tmp = content[1].Split("&");
        //     var response = new UniversalLinkResponse
        //                    {
        //                        RequestId = Guid.Parse(tmp[0].Split("=")[1]),
        //                        LastTx = tmp[1].Split("=")[1],
        //                        IsInitStatus = false
        //                    };
        //     
        //     return response;
        // }
        //
        // public static UniversalLinkResponse ParseUniversalLink(this string universallink)
        // {
        //     var response = new UniversalLinkResponse();
        //     var contents = universallink.Split("?")[1].Split("&");
        //     foreach (var keyword in UniversalLinkExtension._keywords)
        //     {
        //         if(contents.Any(p => p.Contains(keyword)))
        //         {
        //             var tmp = contents.First(p => p.Contains(keyword)).Split("=");
        //             switch (keyword)
        //             {
        //                 case "request_id":
        //                     response.RequestId = Guid.Parse(tmp[1]);
        //                     break;
        //                 case "address":
        //                     response.Address = tmp[1];
        //                     response.Action = ActionEnum.RequestAccount;
        //                     break;
        //                 case "signature":
        //                     response.Signature = tmp[1];
        //                     response.Action = ActionEnum.SignMessage;
        //                     break;
        //                 case "tx_hash":
        //                     response.LastTx = tmp[1];
        //                     response.Action = ActionEnum.SendTransaction;
        //                     break;
        //             }
        //         }
        //     }
        //     
        //     return response;
        // }
    }
}
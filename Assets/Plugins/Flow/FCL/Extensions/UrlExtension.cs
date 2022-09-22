using System;
using System.Linq;
using System.Text;
using Flow.FCL.Models;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;

namespace Flow.FCL.Extensions
{
    public static class UrlExtension
    {
        public static (string IframeUrl, Uri PollingUrl) AuthnEndpoint(this AuthnAdapterResponse response)
        {
            var iframeUrlBuilder = new StringBuilder();
            iframeUrlBuilder.Append(response.Local.Endpoint + "?")
                           .Append(Uri.EscapeDataString("channel") + "=")
                           .Append(Uri.EscapeDataString(response.Local.Params.Channel) + "&")
                           .Append(Uri.EscapeDataString("appId") + "=")
                           .Append(Uri.EscapeDataString(response.Local.Params.AppId) + "&")
                           .Append(Uri.EscapeDataString("authenticationId") + "=")
                           .Append(Uri.EscapeDataString(response.Local.Params.AuthenticationId) + "&")
                           .Append(Uri.EscapeDataString("thumbnail") + "=")
                           .Append(Uri.EscapeDataString(response.Local.Params.Thumbnail) + "&")
                           .Append(Uri.EscapeDataString("title") + "=")
                           .Append(Uri.EscapeDataString(response.Local.Params.Title) + "&")
                           .Append(Uri.EscapeDataString("fclVersion") + "=")
                           .Append(Uri.EscapeDataString(response.Local.Params.FclVersion));
                
            if(response.Local.Params.AccountProofIdentifier != null && response.Local.Params.AccountProofIdentifier != null)
            {
                iframeUrlBuilder.Append("&")
                                .Append(Uri.EscapeDataString("accountProofIdentifier") + "=")
                                .Append(Uri.EscapeDataString(response.Local.Params.AccountProofIdentifier) + "&")
                                .Append(Uri.EscapeDataString("accountProofNonce") + "=")
                                .Append(Uri.EscapeDataString(response.Local.Params.AccountProofNonce));
            }
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(response.Updates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("authenticationId") + "=")
                             .Append(Uri.EscapeDataString(response.Updates.Params.AuthenticationId));
            var iframeUrl = iframeUrlBuilder.ToString();
            var pollingUrl = pollingUrlBuilder.ToString();
            var pollingUri = new Uri(pollingUrl.ToString());    
            
            return (iframeUrl, pollingUri);
        }
        
        public static Uri PayerEndpoint(this PreAuthzAdapterResponse adapterResponse)
        {
            var payerPostUrlBuilder = new StringBuilder();
            var item = adapterResponse.AuthorizerData.Payers.First();
            var sessionId = item.Params.GetValue("sessionId")?.ToString();
            var payerId = item.Params.GetValue("payerId")?.ToString();
            payerPostUrlBuilder.Append(item.Endpoint + "?")
                               .Append(Uri.EscapeDataString("sessionId") + "=")
                               .Append(Uri.EscapeDataString(sessionId!) + "&")
                               .Append(Uri.EscapeDataString("payerId") + "=")
                               .Append(Uri.EscapeDataString(payerId!));
            var payerUri = new Uri(payerPostUrlBuilder.ToString());
            return payerUri;
        }
        
        public static string PreAuthzEndpoint(this FclService service)
        {
            var sessionId = service.PollingParams.GetValue("sessionId")?.ToString();
            var preAuthzUrlBuilder = new StringBuilder();
            preAuthzUrlBuilder.Append(service.Endpoint.AbsoluteUri + "?")
                              .Append(Uri.EscapeDataString("sessionId") + "=")
                              .Append(Uri.EscapeDataString(sessionId!));
            
            return preAuthzUrlBuilder.ToString();
        }
        
        public static (string IframeUrl, Uri PollingUrl) AuthzEndpoint(this AuthzAdapterResponse adapterResponse)
        {
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(adapterResponse.AuthorizationUpdates.Endpoint.AbsoluteUri + "?")
                              .Append(Uri.EscapeDataString("sessionId") + "=")
                              .Append(Uri.EscapeDataString(adapterResponse.AuthorizationUpdates.Params.SessionId()) + "&")
                              .Append(Uri.EscapeDataString("authorizationId") + "=")
                              .Append(Uri.EscapeDataString(adapterResponse.AuthorizationUpdates.Params.AuthorizationId()));
                                    
            var pollingUri = new Uri(pollingUrlBuilder.ToString());
            var authzIframeUrl = adapterResponse.Local.First().Endpoint.AbsoluteUri;
            
            return (authzIframeUrl, pollingUri);
        }
        
        public static (string IframeUrl, Uri PollingUrl) AuthzEndpoint(this NonCustodialAuthzResponse adapterResponse)
        {
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(adapterResponse.AuthorizationUpdates.Endpoint.AbsoluteUri + "?")
                             .Append(Uri.EscapeDataString("sessionId") + "=")
                             .Append(Uri.EscapeDataString(adapterResponse.AuthorizationUpdates.Params.SessionId()) + "&")
                             .Append(Uri.EscapeDataString("authorizationId") + "=")
                             .Append(Uri.EscapeDataString(adapterResponse.AuthorizationUpdates.Params.AuthorizationId()));
                                    
            var pollingUri = new Uri(pollingUrlBuilder.ToString());
            var authzIframeUrl = adapterResponse.Local.Endpoint.AbsoluteUri;
            
            return (authzIframeUrl, pollingUri);
        }
        
        public static (string IframeUrl, Uri PollingUrl) SignMessageEndpoint(this AuthnAdapterResponse response)
        {
            var iframeUrlBuilder = new StringBuilder();
            iframeUrlBuilder.Append(response.Local.Endpoint + "?")
                            .Append(Uri.EscapeDataString("channel") + "=")
                            .Append(Uri.EscapeDataString(response.Local.Params.Channel) + "&")
                            .Append(Uri.EscapeDataString("signatureId") + "=")
                            .Append(Uri.EscapeDataString(response.Local.Params.SignatureId));
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(response.Updates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("signatureId") + "=")
                             .Append(Uri.EscapeDataString(response.Updates.Params.SignatureId) + "&")
                             .Append(Uri.EscapeDataString("sessionId") + "=") 
                             .Append(Uri.EscapeDataString(response.Updates.Params.SessionId));
            
            var iframeUrl = iframeUrlBuilder.ToString();
            var pollingUrl =pollingUrlBuilder.ToString();
            var pollingUri = new Uri(pollingUrlBuilder.ToString());
            
            return (iframeUrl, pollingUri);
        }
        
        public static string AuthzAdapterEndpoint(this AuthInformation data)
        {
            var sessionId = data.Params.GetValue("sessionId")?.ToString();
            var authzUrlBuilder = new StringBuilder();
            authzUrlBuilder.Append(data.Endpoint + "?")
                           .Append(Uri.EscapeDataString("sessionId") + "=")
                           .Append(Uri.EscapeDataString(sessionId!));
                
            var postUrl = authzUrlBuilder.ToString();
            return postUrl;
        }
        
        public static string SignMessageAdapterEndpoint(this FclService service)
        {
            var signUrlBuilder = new StringBuilder();
            var sessionId = service.PollingParams.SessionId();
            signUrlBuilder.Append(service.Endpoint + "?")
                          .Append(Uri.EscapeDataString("sessionId") + "=")
                          .Append(Uri.EscapeDataString(sessionId));
            var signUrl = signUrlBuilder.ToString();
            return signUrl;
        }
        
        public static string PreAuthAdapterEndpoint(this FclService service)
        {
            var preAuthzUrlBuilder = new StringBuilder();
            preAuthzUrlBuilder.Append(service.Endpoint.AbsoluteUri + "?")
                              .Append(Uri.EscapeDataString("sessionId") + "=")
                              .Append(Uri.EscapeDataString(service.PollingParams.SessionId()));
            return preAuthzUrlBuilder.ToString();
        }
    }
}
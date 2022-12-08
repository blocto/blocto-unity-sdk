## What is FCL?

The Flow Client Library (FCL) is used to interact with user wallets and the Flow blockchain. When using FCL for authentication, dApps are able to support all FCL-compatible wallets on Flow and their users without any custom integrations or changes needed to the dApp code.

For more description, please refer to [fcl.js](https://github.com/onflow/fcl-js)

This repo is inspired by [fcl-js](https://github.com/onflow/fcl-js) and [flow.net](https://github.com/tyronbrand/flow.net)

---
## Getting Started

You can watch the source code at [Github](https://github.com/portto/blocto-unity-sdk/tree/main/Assets/Plugins/Flow/FCL)

### Requirements
- .Net Core version >= 2.1

### Release Page

FCL-SDK is available through [Github](https://github.com/portto/blocto-unity-sdk/releases/tag/fcl-unity-0.1.0). You can include specific subspec to install, simply add the following line to your Podfile:

## Import unitypackage
You can import .unitypackage to your unity project, simply perform the following steps 

You can import Standard Asset Packages, which are asset collections pre-made and supplied with Unity, and Custom Packages, which are made by people using Unity. The more description at [unity document](https://docs.unity3d.com/Manual/AssetPackagesImport.html).

Choose Assets > Import Package > to import both types of package.

![unity import package](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FNvJDR0MSuBI5TYdDpVIP%2FImport-unity-package.png?alt=media&token=f0160516-335f-4a56-bf63-a186bd36e478)

---
## FCL for dApps
### Configuration

Initialize `WalletProvider` instance e.g. `BloctoWalletProvider`, `DapperWalletProvider`. And simply specify `flow.network` then you are good to go.

```csharp
using Flow.FCL;
using Flow.FCL.Config
using Blocto.SDK.Flow;

var config = new Config();
config.Put("discovery.wallet", "https://flow-wallet-dev.blocto.app/api/flow/authn")
      .Put("accessNode.api", "https://rest-testnet.onflow.org/v1")
      .Put("flow.network", "dev");
        
var walletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
    initialFun: GetWallet => {
                    var walletProvider = GetWallet.Invoke(
                        gameObject,
                        new FlowUnityWebRequest(gameObject, config.Get("accessNode.api")),
                        new ResolveUtility());
                    
                    return walletProvider;
                },
    env: {"dev" or "mainnet"},
    bloctoAppIdentifier:Guid.Parse("d0c4c565-db60-4848-99c8-2bdfc6bd3576"));
        
var fcl = FlowClientLibrary.CreateClientLibrary(
    initialFun: GetFCL => {
                    var fcl = GetFCL.Invoke(gameObject, _walletProvider, new ResolveUtility());
                    return fcl;
                }, 
    config: config);
```

> **Note**: bloctoSDKAppId can be found in [Blocto Developer Dashboard](https://developers.blocto.app/), for detail instruction please refer to [Blocto Docs](https://docs.blocto.app/blocto-sdk/register-app-id)

### User Signatures

Cryptographic signatures are a key part of the blockchain. They are used to prove ownership of an address without exposing its private key. While primarily used for signing transactions, cryptographic signatures can also be used to sign arbitrary messages.

FCL has a feature that let you send arbitrary data to a configured wallet/service where the user may approve signing it with their private keys.

We can retrieve user signatures only after user had logged in, otherwise error will be thrown.

```csharp
using Flow.FCL;
using Flow.Net.Sdk.Core.Models;

var userSignature = default(FlowSignature);
var originalMessage = "SignMessage Test";
fcl.SignUserMessage(
    message: originalMessage,
    callback: result => {
                  if(result.IsSuccessed == false)
                  {
                      Debug.Log($"Get signmessage failed, Reason: {result.Message}");
                      return;
                  }
                
                  userSignature = result.Data;
                  Debug.Log($"Message: {originalMessage} \r\n");
                  foreach (var userSignature in result.Data)
                  {
                      Debug.Log($"Signature: {Encoding.UTF8.GetString(userSignature.Signature)} \r\nKeyId: {userSignature.KeyId}\");
                  }
              }); 
```

The message could be signed by several private key of the same wallet address. Those signatures will be valid all together as long as their corresponding key weight sum up at least 1000.
For more info about multiple signatures, please refer to [Flow docs](https://developers.flow.com/learn/concepts/accounts-and-keys#single-party-multiple-signatures)


### Blockchain Interactions
- *Query the chain*: Send arbitrary Cadence scripts to the chain and receive back decoded values
```csharp
using Flow.FCL;
using Flow.Net.Sdk.Core.Models;

var script = @" 
            pub struct User {
                pub var balance: UFix64
                pub var address: Address
                pub var name: String
                
                init(name: String, address: Address, balance: UFix64) {
                    self.name = name
                    self.address = address
                    self.balance = balance
                }
            }

            pub fun main(name: String): User {
                return User(
                    name: name,
                    address: 0x1,
                    balance: 10.0
                )
            }";

var flowScript = new FlowScript
                    {
                        Script = script,
                        Arguments = new List<ICadence>
                                    {
                                        new CadenceString("blocto")
                                    }
                    };

var result = await fcl.QueryAsync(flowScript);
if(result.IsSuccessed)
{
    //// Composite object parser
    var name = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceString>("name").Value;
    var balance = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceNumber>("balance").Value;
    var address = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceAddress>("address").Value;
    Debug.Log($"Name: {name}, Balance: {balance}, Address: {address})";
}
else
{
    Debug.Log($"Error message: {result.Message}");
}
```
- *Mutate the chain*: Send arbitrary transactions with specify authorizer to perform state changes on chain.

```csharp
using Flow.FCL;
using Flow.Net.Sdk.Core.Models;

var script = @"
            import ValueDapp from 0x5a8143da8058740c

            transaction(value: UFix64) {
                prepare(authorizer: AuthAccount) {
                    ValueDapp.setValue(value)
                }
            }";

var tx = new FlowTransaction
            {
                Script = script,
                GasLimit = 1000,
                Arguments = new List<ICadence>
                            {
                                new CadenceNumber(CadenceNumberType.UFix64, "123.456"),
                            },
            };

fcl.Mutate(tx, txId => {
                    Debug.Log($"https://testnet.flowscan.org/transaction/{txId}");
                });
```

[Learn more about on-chain interactions >](https://docs.onflow.org/fcl/reference/api/#on-chain-interactions)

---
## Prove ownership
To prove ownership of a wallet address, there are two approaches.
- Account proof: in the beginning of authentication, there are `accountProofData` you can provide for user to sign and return generated signatures along with account address. 

`fcl.authanticate` is also called behide `fcl.login()` with accountProofData set to nil.

```csharp
using Flow.FCL;

var accountProofData = new AccountProofData
                               {
                                   AppId = "{your app name}",
                                   Nonce = KeyGenerator.GetUniqueKey(32).StringToHex()
                               };
        
fcl.Authenticate(accountProofData, ((currentUser,  accountProofData) => {
                                            Debug.Log(currentUser.Addr.Address.AddHexPrefix());
                                        }));
```

- [User signature](#User-Signatures): provide specific message for user to sign and generate one or more signatures.

### Verifying User Signatures

What makes message signatures more interesting is that we can use Flow blockchain to verify the signatures. Cadence has a built-in function called verify that will verify a signature against a Flow account given the account address.

FCL includes a utility function, verifyUserSignatures, for verifying one or more signatures against an account's public key on the Flow blockchain.

You can use both in tandem to prove a user is in control of a private key or keys. This enables cryptographically-secure login flow using a message-signing-based authentication mechanism with a user’s public address as their identifier.

To verify above ownership, there are two utility functions define accordingly in [AppUtilities](https://github.com/portto/fcl-swift/blob/main/Sources/FCL-SDK/AppUtilities/AppUtilities.swift).

---
## Utilities
- Get account details from any Flow address
```csharp
var account = awai fcl.FlowClient.GetAccountAtLatestBlockAsync(address: address);
```
- Get the latest block
```csharp
var lastBlock = await fcl.FlowClient.GetLatestBlockAsync(isSealed: true);
```
- Transaction status polling
```csharp
var txr = await fcl.FlowClient.GetTransactionResultAsync(transactionId: txHash);

```

[Learn more about utilities >](https://github.com/tyronbrand/flow.net)

---
## FCL for Wallet Providers
Wallet providers on Flow have the flexibility to build their user interactions and UI through a variety of ways:
- Native app intercommunication via Universal links or custom schemes.
- Back channel communication via HTTP polling with webpage button approving.

FCL is agnostic to the communication channel and be configured to create both custodial and non-custodial wallets. This enables users to interact with wallet providers both native app install or not.

Native app should be considered first to provide better user experience if installed, otherwise fallback to back channel communication.

The communication channels involve responding to a set of pre-defined FCL messages to deliver the requested information to the dApp.  Implementing a FCL compatible wallet on Flow is as simple as filling in the responses with the appropriate data when FCL requests them.


### Current Wallet Providers
- [Blocto](https://blocto.portto.io/en/) (fully supported) [Docs](https://docs.blocto.app/blocto-sdk/unity-sdk-coming-soon/flow)

---

## Next Steps

Learn Flow's smart contract language to build any script or transactions: [Cadence](https://docs.onflow.org/cadence/).

Explore all of Flow [docs and tools](https://docs.onflow.org).

---

## Support

Notice a problem or want to request a feature? [Add an issue](https://github.com/portto/blocto-unity-sdk/issues) or [Make a pull request](https://github.com/portto/blocto-unity-sdk/compare).
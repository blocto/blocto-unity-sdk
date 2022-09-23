# FCL-Unity Demo App

The repository contains fcl-unity, blocto-unity-sdk and demo app. This demo application demonstrates how it is to access the blocto wallet through fcl-unity and blocto-unity-sdk.

## Requirements

* Unity version 2021.3.5f1
* .Net Core version >= 2.1
* iOS version >= 13
* Android version >= 11

## Get Started

You can browse this repository and find your desired utility or you can clone this repository to run demo app. 

    git clone https://github.com/portto/blocto-unity-sdk.git

After clone repository, the entire directory is the Unity project directory, open it with Unity and open the FlowScene.unity scene to see demo app's showcase feature

![flowscene.unity](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2Ft6XmJaMeDnmxUebHQadd%2Fflowscene.png?alt=media&token=e1bb11bd-fa87-4d1b-bd26-14f279cf0117)

In the project settings, you need to fill in your contracted team ID in Identification to be able to run the demo app in the iOS simulator.

![Signing Team ID](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FeMGV8s4Ne7fP0sAQgM5a%2FSigningTeamID.png?alt=media&token=102fdfcf-83d3-4f85-b21f-57a31244fa07)
## Tutorials Demo App

The demo app include five access to flow blockchain interactions

* Interact with Flow networks (Mainnet-beta, Testnet)
* Sign and verify messages
* Send transactions
* Query smart contract state
* Mutate smart contract state

More detailed description at [fcl-unity](https://github.com/portto/blocto-unity-sdk/tree/main/Assets/Plugins/Flow) and [blocto-unity-sdk](https://github.com/portto/blocto-unity-sdk/tree/main/Assets/Plugins/Blocto.Sdk)
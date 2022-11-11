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

Please check `MainController` has been add to inspector of main camera
![flowscene](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FdpQVQ7kBrVcFCdFIwmRL%2FMain%20Camera%20Inspector.png?alt=media&token=ba69b993-4e1d-4186-bded-170b6d476d18)

## Tutorials Demo App

The demo app include five access to flow blockchain interactions

* Interact with Flow networks (Mainnet-beta, Testnet)
* Sign and verify messages
* Send transactions
* Query smart contract state
* Mutate smart contract state

## How to use demo app

1. You can first connect to the blocto wallet by clicking **ConnectWallet**, which will display the wallet address in the textbox (![wallet address](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2F3TLvLUMdbAwU1PPPTHXm%2Faddress.jpg?alt=media&token=c1848dbc-2131-4182-a73e-93adb7ed8d16 "wallet address"))  when the successfully connected.

    ![ConnectWallet][ConnectWallet]

[ConnectWallet]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FgbzZpE2Y39rUDqQXnXwV%2Fconnectwallet.jpg?alt=media&token=1332f25b-2f4b-4eec-998f-a9307cbf0786 "Connect Wallet"

2. The **Sign Message** button means that use your’s private key to sign inpufield string.

    ![Sign Message][SignMessage]

    When the sign is completed, the signature and key index will be displayed in the same inputfield.

    ![SignMessage Result][SignMessageResult]

[SignMessageResult]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2Fv0QB7lUUr4QTPW3es8GP%2Faftersignmessage.jpg?alt=media&token=7387cf3b-2289-4dd8-9802-d1b09f1dbf18 "SignMessage Result"

[SignMessage]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FTjkbvGVbn2OYM0kVuBtN%2Fsignmessage.jpg?alt=media&token=f0bb10d1-fef9-4ec2-9272-f0d6b0575bc3 "Sign Message"

3. When the sign message is completed, you can click Verify Message to verify the signature on the blockchain. The verification results will be displayed in inputField.

    ![Verify Message][VerifyMessage]

[VerifyMessage]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2F0HUjoBuEBEQjQTcbHrGG%2Fverifymessage.jpg?alt=media&token=f49c8e87-e9f5-43c0-bf92-f825fdbae5d4 "Verify Message"

4. The **Mutate** button means to write data to blockchain, similar to an Ethereum transaction. The textbox (![wallet address](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FfiMGDaPsMfMwxo7GDa6s%2Frecipient.jpg?alt=media&token=d5cb7463-a052-4eed-89d9-7227afbcc7fa "wallet address")) is the recipient address, which you can change to any address, and the textbox (![wallet address](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2Fjn5bOaQB3gbXggYCi6lZ%2Fvalueoftokentransferred.jpg?alt=media&token=8eb14113-3db6-4a0f-8397-8a1679265c93 "value of taken transferred")) is the value of the token transferred. When mutate is completed it will show the transaction hash in the inputfield.

    ![Mutate][Mutate]

[Mutate]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FPoZpXvTgU8UQLLy7Vuv9%2Fmutate.jpg?alt=media&token=ec882c6d-be67-4111-887a-685e9ad7db76 "Mutate"

5. The **Transaction** button implies interaction with a smart contract on the blockchain, that is the same as Mutate, only with a different script for the transaction structure. The button allows setting the property of smart contract to the specified value. You can enter the value in textbox.

    ![Transaction][Transaction]

[Transaction]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2F7ghHgnwZZruJsd46dTz7%2Ftransaction.jpg?alt=media&token=f1092921-deba-4618-863e-35249692c412 "Transaction"

6. The Query button means to send a query to a smart contract to get properties of smart contract and then display the results in the inputfield. 

    ![Query][Query]

[Query]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FHTQMk7Hzt3atN7LwJVL3%2Fquery.jpg?alt=media&token=01c704ef-6e13-4546-9ab2-1ed8b6ee7bae "Query"

7. The GetLatestTx button means to get the latest transaction status of the wallet, including execution, status information, and display it in the inputfield.

    ![GetLatestTx][GetLatestTx]

[GetLatestTx]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2F4M6X2gwONZwBwPb0oF87%2Fgetlatesttx.jpg?alt=media&token=111588b5-ac14-4540-a90e-58a0ffaa8c1b "GetLatestTx"

## Project setting
On android platform, please follow the below steps to set up the project property.
1. Create a new Keystore, you can refer to the following links to create Keystore.
    * https://docs.unity3d.com/Manual/android-keystore-create.html
    * https://docs.unity.cn/2021.1/Documentation/Manual/android-keystore-manager.html

2. Make sure the description of App - Android on Blocto dashboard matches with your unity project’s Identification on unity editor (check screenshot below)

    ![Android_Identification][AndroidSetting]

[AndroidSetting]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FxqptjL0J97MJ1Pk9Q4AJ%2FAndroid_identification.jpg?alt=media&token=b769a134-d626-412d-b586-fd81a34cd066 "Android Identification"

3. Go to “Publishing Settings”, choose Project Settings > Player > Publishing Settings then enable 
    * Custom Main Manifest
    * Custom Main Gradle Template
    * Custom Gradle Properties Template

![Publish Setting][PublishSetting]

[PublishSetting]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FYcJROnDIZTrZLv8glfVW%2Fpublishsettings.jpg?alt=media&token=996a9b9a-b66d-414b-878e-f1fddee0a5b8 "Publish Setting"

On iOS platform, please set up target minimum iOS version as **13.0**.

![iOS version][iOSversion]

[iOSversion]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FDyOAJdpDdPEnHw7C9G8K%2FiOS_target_version.jpg?alt=media&token=5d0e82bb-d547-4266-8eb6-0ce57a77fce1 "iOS version" 

More detailed description at [fcl-unity](https://github.com/portto/blocto-unity-sdk/tree/main/Assets/Plugins/Flow) and [blocto-unity-sdk](https://github.com/portto/blocto-unity-sdk/tree/main/Assets/Plugins/Blocto.Sdk)


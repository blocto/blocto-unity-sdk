# Unity Demo App

The repository contains fcl-unity, blocto-unity-sdk and demo app. This demo application demonstrates how it is to access the blocto wallet through blocto-unity-sdk. If you are trying to access the flow chain, you will need to install additional fcl-unity 

## Requirements

* Unity version 2021.3.12f1
* .Net Core version >= 2.1
* iOS version >= 13
* Android version >= 7.1

## Get Started

You can browse this repository and find your desired utility or you can clone this repository to run demo app. 

    git clone https://github.com/portto/blocto-unity-sdk.git

After clone repository, the entire directory is the Unity project directory, open it with Unity and open the scene to see demo app's showcase feature

![Scenes](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FXmjSNJjJZNZM5sCVqATx%2Fscenes.jpg?alt=media&token=34b4ae62-019a-4e80-901d-bef015e802ea)

In the project settings, you need to fill in your contracted team ID in Identification to be able to run the demo app in the iOS simulator.

![Signing Team ID](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FeMGV8s4Ne7fP0sAQgM5a%2FSigningTeamID.png?alt=media&token=102fdfcf-83d3-4f85-b21f-57a31244fa07)

Please check `Script` has been add to inspector of main camera
![flowscene](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2Fm1uZ5vqH6W5OIDMmbQaG%2Fcheckscript.jpg?alt=media&token=5cb61215-ed42-4af4-bc31-f924c461de4c)

## Tutorials Demo App

The demo app include five access to flow blockchain interactions

* Interact with Flow networks (Mainnet-beta, Testnet)
* Sign and verify messages
* Send transactions
* Query smart contract state
* Mutate smart contract state

## How to use demo app

1. You can first connect to the blocto wallet by clicking **ConnectWallet**, which will display the wallet address in the textbox (![wallet address](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FHNd85PktMWfoIebKHLU9%2Fwalletaddress.jpg?alt=media&token=c70d3b1a-7733-4377-99c7-0fd194da8177))  when the successfully connected.

    ![ConnectWallet][ConnectWallet]

[ConnectWallet]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FCo5mVSzigPUR4UDN2Cha%2Fconnectedwallet.jpg?alt=media&token=a840273e-fbec-47f3-a518-ba79b2d14317 "Connect Wallet"

2. The **Sign Message** button means that use your’s private key to sign inpufield string. (**only support flowchain**)

    ![Sign Message][SignMessage]

    When the sign is completed, the signature and key index will be displayed in the same inputfield.

    ![SignMessage Result][SignMessageResult]

[SignMessageResult]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FHxrJtarOvLZMNN5KW0Ha%2Fsigned_message.jpg?alt=media&token=3231eedd-ee22-43ba-9866-a8ef7c3f26bf "SignMessage Result"

[SignMessage]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FIbHYqOHxcK27Qsvdwdu3%2Fsignmessage.jpg?alt=media&token=54702997-2e81-45b5-939a-be3e2cd0679e "Sign Message"

3. When the sign message is completed, you can click Verify Message to verify the signature on the blockchain. The verification results will be displayed in inputField.

    ![Verify Message][VerifyMessage]

[VerifyMessage]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FuRHtSwGp15mA3DLOZNZ2%2Fverify_message.jpg?alt=media&token=da6633aa-157e-4ffa-812f-d3ef4e4fb7bb "Verify Message"

4. The **Transfer** button means to write data to blockchain. The textbox (![wallet address](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FfiMGDaPsMfMwxo7GDa6s%2Frecipient.jpg?alt=media&token=d5cb7463-a052-4eed-89d9-7227afbcc7fa "wallet address")) is the recipient address, which you can change to any address, and the textbox (![wallet address](https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2Fjn5bOaQB3gbXggYCi6lZ%2Fvalueoftokentransferred.jpg?alt=media&token=8eb14113-3db6-4a0f-8397-8a1679265c93 "value of taken transferred")) is the value of the token transferred. When mutate is completed it will show the transaction hash in the inputfield.

    ![Transfer][Transfer]

[Transfer]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2Ffr9GxY166K4K4dW6HYg8%2Ftransfered.jpg?alt=media&token=ff92a04a-41e2-40c8-bfc7-132a24f9eaa2 "Mutate"

5. The **SetValue** button implies interaction with a smart contract on the blockchain, that is the same as Transfer, only with a different script for the transaction structure. The button allows setting the property of smart contract to the specified value. You can enter the value in textbox. When mutate is completed it will show the transaction hash in the inputfield.
    ![Transaction][Transaction]

[Transaction]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FcuivMLVvgzpJqDtpuJjw%2Fsetedvalue.jpg?alt=media&token=7985ca79-21bc-405c-b342-1203326a12fa "Transaction"

6. The **GetValue** button means to send a query to a smart contract to get properties of smart contract and then display the results in the inputfield. 

    ![Query][Query]

[Query]: https://files.gitbook.com/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F-MFJEAgz-LrhDYkRm4sv%2Fuploads%2FxDN3F7Yzpbw84Zslnhbc%2Fqueryed.jpg?alt=media&token=cf41ebb2-8714-4f1d-a043-f11eb9d67272 "Query"

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


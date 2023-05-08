using System.Collections.Generic;
using Blocto.Sdk.Flow.Utility;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using NUnit.Framework;

namespace Editor.EditTests
{
    [TestFixture]
    public class FlowResolveUtilityTest
    {
        static string _script = "import FungibleToken from 0x9a0766d93b6608b7\nimport FlowToken from 0x7e60df042a9c0868\n\ntransaction(amount: UFix64, to: Address) {\n\n    // The Vault resource that holds the tokens that are being transferred\n    let sentVault: @FungibleToken.Vault\n\n    prepare(signer: AuthAccount) {\n\n        // Get a reference to the signer's stored vault\n        let vaultRef = signer.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)\n            ?? panic(\"Could not borrow reference to the owner's Vault!\")\n\n        // Withdraw tokens from the signer's stored vault\n        self.sentVault <- vaultRef.withdraw(amount: amount)\n    }\n\n    execute {\n\n        // Get the recipient's public account object\n        let recipient = getAccount(to)\n\n        // Get a reference to the recipient's Receiver\n        let receiverRef = recipient.getCapability(/public/flowTokenReceiver)\n            .borrow<&{FungibleToken.Receiver}>()\n            ?? panic(\"Could not borrow receiver reference to the recipient's Vault\")\n\n        // Deposit the withdrawn tokens in the recipient's receiver\n        receiverRef.deposit(from: <-self.sentVault)\n    }\n}";

        private ResolveUtility _target;

        private FlowTransaction _tx;
        
        [SetUp]
        public void SetUp()
        {
            _target = new ResolveUtility();
            _tx = new FlowTransaction
                  {
                      Script = _script,
                      GasLimit = 1000,
                      Arguments = new List<ICadence>
                                  {
                                      new CadenceNumber(CadenceNumberType.UFix64, $"0.1"),
                                      new CadenceAddress("0xf90c31e986eb22f90xf90c31e986eb22f9")
                                  },
                  };
        }

        [Test]
        public void TestResolve()
        {
            var result = _target.ResolvePreSignable(ref _tx);
            var expected = result.GetValue("interaction");
            var expectInteraction = "";
            
            Assert.AreEqual(result.GetValue("interaction"), expectInteraction);
        }
    }
}
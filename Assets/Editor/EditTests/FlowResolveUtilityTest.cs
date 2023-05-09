using System.Collections.Generic;
using System.Linq;
using Blocto.Sdk.Flow.Utility;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json;
using NSubstitute;
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
        }

        [Test]
        public void TestResolvePreSignable()
        {
            var mockNumber = Substitute.For<CadenceNumber>(CadenceNumberType.UFix64, $"0.1");
            mockNumber.TempId.Returns("1234567890");
            
            var mockAddress = NSubstitute.Substitute.For<CadenceAddress>("0xf90c31e986eb22f90xf90c31e986eb22f9");
            mockAddress.TempId.Returns("0987654321");
            mockAddress.Type.Returns("Address");
            
            _tx = new FlowTransaction
                  {
                      Script = "test",
                      GasLimit = 1000,
                      Arguments = new List<ICadence>
                                  {
                                      mockNumber, mockAddress
                                  },
                  };
            
            var result = _target.ResolvePreSignable(ref _tx);
            var actual = JsonConvert.SerializeObject(result);
            var expected = "{\"args\":[{\"type\":\"UFix64\",\"value\":\"0.1\"},{\"type\":\"Address\",\"value\":\"0xf90c31e986eb22f90xf90c31e986eb22f9\"}],\"interaction\":{\"tag\":\"TRANSACTION\",\"assigns\":{},\"reason\":null,\"status\":\"OK\",\"arguments\":{\"1234567890\":{\"kind\":\"ARGUMENT\",\"tempId\":\"1234567890\",\"value\":\"0.1\",\"asArgument\":{\"type\":\"UFix64\",\"value\":\"0.1\"},\"xform\":{\"label\":\"UFix64\"}},\"0987654321\":{\"kind\":\"ARGUMENT\",\"tempId\":\"0987654321\",\"value\":\"0xf90c31e986eb22f90xf90c31e986eb22f9\",\"asArgument\":{\"type\":\"Address\",\"value\":\"0xf90c31e986eb22f90xf90c31e986eb22f9\"},\"xform\":{\"label\":\"Address\"}}},\"message\":{\"cadence\":\"test\",\"refBlock\":null,\"computeLimit\":1000,\"proposer\":null,\"payer\":null,\"authorizations\":[],\"arguments\":[\"1234567890\",\"0987654321\"],\"params\":[]},\"events\":null,\"account\":{\"addr\":null},\"collection\":null,\"transaction\":null,\"block\":null,\"params\":{}},\"voucher\":{\"cadence\":\"test\",\"refBlock\":null,\"computeLimit\":1000,\"arguments\":[{\"type\":\"UFix64\",\"value\":\"0.1\"},{\"type\":\"Address\",\"value\":\"0xf90c31e986eb22f90xf90c31e986eb22f9\"}],\"proposalKey\":{\"address\":null,\"keyId\":null,\"sequenceNum\":null}},\"f_type\":\"PreSignable\",\"f_vsn\":\"1.0.1\",\"addr\":null,\"keyId\":0,\"cadence\":\"test\",\"roles\":{\"proposer\":true,\"authorizer\":true,\"payer\":true}}";
            Assert.AreEqual(expected, actual);
        }
    }
}
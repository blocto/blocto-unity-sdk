using System;
using Flow.FCL.Extensions;
using Flow.FCL.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Editor.EditTests
{
    [TestFixture]
    public class UrlExtensionTest
    {
        [Test]
        public void SignMessageAdapterEndpoint_Test()
        {
            var _target = new FclService
            {
                FType = "service",
                Type = ServiceTypeEnum.USERSIGNATURE,
                Endpoint = new Uri("https://wallet-v2-dev.blocto.app/api/flow/user-signature"),
                Id = "DGEJohAHIYdN5uIzj8_-B-FFq-ES9bWLHNM2guEHweF",
                PollingParams = new JObject
                {
                    new JProperty("sessionId", "DGEJohAHIYdN5uIzj8_-B-FFq-ES9bWLHNM2guEHweF")
                }
            };

            var actual = _target.SignMessageAdapterEndpoint();
            var expected = "https://wallet-v2-dev.blocto.app/api/flow/user-signature?sessionId=DGEJohAHIYdN5uIzj8_-B-FFq-ES9bWLHNM2guEHweF";
            Assert.AreEqual(expected, actual);
        }
    }
}
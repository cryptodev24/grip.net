﻿// Copyright (c) RealCrowd, Inc. All rights reserved. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RealCrowd.PublishControl;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace RealCrowd.Grip.Tests
{
    public class FppFormat : Format
    {
        public object Value { get; set; }

        public override string Name
        {
            get { return "fpp"; }
        }

        public override object ToObject()
        {
            return Value;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        private Configuration config;

        [TestInitialize]
        public void LoadConfig()
        {
            var reader = new StreamReader(Environment.GetEnvironmentVariable("RC_GRIP_CONFIG"));
            var configObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.ReadToEnd());
            config = new PublishControl.Configuration(configObj["gripProxiesString"].ToString());
            config.Entries.Add(new Configuration.Entry() { ControlIss = "test-iss", Key = Encoding.UTF8.GetBytes("test-key") });
        }

        [TestMethod]
        public void BadSignatureFormat()
        {
            var token = "invalid-data";
            try
            {
                Validator.CheckGripSignature(token, config, new DateTime(2013, 11, 22, 14, 30, 0));
            }
            catch (ValidationException)
            {
                // we want it to fail
                return;
            }

            Assert.IsTrue(false);
        }

        [TestMethod]
        public void ExpiredSignature()
        {
            var token = "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJpc3MiOiAidGVzdC1pc3MiLCAiZXhwIjogMTM4NTE2NTQ2M30.kXmc_P_FsmqZH_tUPph4WPK9cW987RFYpMnOGR3OHt8";
            try
            {
                Validator.CheckGripSignature(token, config, new DateTime(2013, 11, 23, 1, 9, 0, DateTimeKind.Utc));
            }
            catch (ValidationException)
            {
                // we want it to fail
                return;
            }

            Assert.IsTrue(false);
        }

        [TestMethod]
        public void ValidSignature()
        {
            var token = "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJpc3MiOiAidGVzdC1pc3MiLCAiZXhwIjogMTM4NTE2NTQ2M30.kXmc_P_FsmqZH_tUPph4WPK9cW987RFYpMnOGR3OHt8";
            var claim = Validator.CheckGripSignature(token, config, new DateTime(2013, 11, 22, 23, 9, 0, DateTimeKind.Utc));
            Assert.AreEqual("test-iss", claim["iss"].ToString());
        }

        [TestMethod]
        public void Publish()
        {
            var pub = new PublishControlSet();
            pub.ApplyConfiguration(config);
            pub.PublishAsync("test", new Item() { Formats = new List<Format>() { new FppFormat() { Value = "hello" } } }).Wait();
        }
    }
}

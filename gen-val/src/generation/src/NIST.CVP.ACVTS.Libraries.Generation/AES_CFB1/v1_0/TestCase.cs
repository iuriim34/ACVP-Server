﻿using System.Collections.Generic;
using Newtonsoft.Json;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Symmetric;
using NIST.CVP.ACVTS.Libraries.Generation.Core;
using NIST.CVP.ACVTS.Libraries.Math;
using NIST.CVP.ACVTS.Libraries.Math.Helpers;

namespace NIST.CVP.ACVTS.Libraries.Generation.AES_CFB1.v1_0
{
    public class TestCase : ITestCase<TestGroup, TestCase>
    {
        public int TestCaseId { get; set; }
        [JsonIgnore]
        public bool? TestPassed => true;
        [JsonIgnore]
        public bool Deferred => false;
        public TestGroup ParentGroup { get; set; }
        public int PayloadLen { get; set; }
        [JsonProperty(PropertyName = "iv")]
        public BitString IV { get; set; }
        [JsonProperty(PropertyName = "pt")]
        public BitString PlainText { get; set; }
        [JsonProperty(PropertyName = "key")]
        public BitString Key { get; set; }
        [JsonProperty(PropertyName = "ct")]
        public BitString CipherText { get; set; }
        [JsonProperty(PropertyName = "resultsArray")]
        public List<AlgoArrayResponse> ResultsArray { get; set; }

        public bool SetResultsArrayString(int index, string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            switch (name.ToLower())
            {
                case "key":
                    ResultsArray[index].Key = new BitString(value);
                    return true;

                case "iv":
                    ResultsArray[index].IV = new BitString(value);
                    return true;

                case "plaintext":
                case "pt":
                    ResultsArray[index].PlainText = new BitString(
                        MsbLsbConversionHelpers.GetBitArrayFromStringOf1sAnd0sReversed(value)
                    );
                    return true;

                case "ciphertext":
                case "ct":
                    ResultsArray[index].CipherText = new BitString(
                        MsbLsbConversionHelpers.GetBitArrayFromStringOf1sAnd0sReversed(value)
                    );
                    return true;
            }
            return false;
        }

        public bool SetString(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            switch (name.ToLower())
            {
                case "key":
                    Key = new BitString(value);
                    return true;

                case "iv":
                    IV = new BitString(value);
                    return true;

                case "plaintext":
                case "pt":
                    PlainText = new BitString(
                        MsbLsbConversionHelpers.GetBitArrayFromStringOf1sAnd0sReversed(value)
                    );
                    return true;

                case "ciphertext":
                case "ct":
                    CipherText = new BitString(
                        MsbLsbConversionHelpers.GetBitArrayFromStringOf1sAnd0sReversed(value)
                    );
                    return true;
            }
            return false;
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NIST.CVP.ACVTS.Libraries.Common;
using NIST.CVP.ACVTS.Libraries.Common.Helpers;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Symmetric.TDES;
using NIST.CVP.ACVTS.Libraries.Generation.Core;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Parsers;
using NIST.CVP.ACVTS.Libraries.Math;

namespace NIST.CVP.ACVTS.Libraries.Generation.TDES_CFBP.v1_0.Parsers
{
    public class LegacyResponseFileParser : ILegacyResponseFileParser<TestVectorSet, TestGroup, TestCase>
    {
        public ParseResponse<TestVectorSet> Parse(string path)
        {

            if (string.IsNullOrEmpty(path))
            {
                return new ParseResponse<TestVectorSet>("There was no path supplied.");
            }

            if (!Directory.Exists(path))
            {
                return new ParseResponse<TestVectorSet>($"Could not find path {path}");
            }

            var algo = EnumHelpers.GetEnumFromEnumDescription<AlgoMode>(Path.GetFileName(path));

            // these are the only algorithms this parse will support

            if (!new[] { AlgoMode.TDES_CFBP1_v1_0, AlgoMode.TDES_CFBP8_v1_0, AlgoMode.TDES_CFBP64_v1_0 }.Contains(algo))
            {
                throw new ArgumentException("Unsupported algorithm.");
            }

            var testVectorSet = new TestVectorSet
            {
                Algorithm = EnumHelpers.GetEnumDescriptionFromEnum(algo),
            };

            // this regex will allow to pull out just the encryption group, or just the decryption group

            var encryptDecryptRegEx = new Regex(@"\[(?<direction>[a-zA-Z]*)\](?<tests>[\sa-zA-Z0-9 =]*)");


            var groups = new List<TestGroup>();


            var files = Directory.GetFiles(path, "*.fax");
            foreach (var file in files)
            {

                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(file).Replace("\r\n", "\n");
                }
                catch (Exception ex)
                {
                    return new ParseResponse<TestVectorSet>(ex.Message);
                }
                foreach (Match match in encryptDecryptRegEx.Matches(fileContent))
                {
                    var testGroup = GetTestGroupFromFileName(file);
                    var tests = CreateTestCases(match.Groups["tests"].Value, algo != AlgoMode.TDES_CFBP1_v1_0, testGroup.TestType == "MCT");

                    if (tests.Any())
                    {
                        testGroup.Function = match.Groups["direction"].Value.ToLower();
                        testGroup.Tests = tests;
                        groups.Add(testGroup);
                    }
                }
            }
            testVectorSet.TestGroups = groups;
            return new ParseResponse<TestVectorSet>(testVectorSet);
        }

        private List<TestCase> CreateTestCases(string input, bool isPtAndCtHex, bool isMCT)
        {
            var testCases = new List<TestCase>();
            var resultsArray = new List<AlgoArrayResponse>();

            var regExTestCase = new Regex(@"(?:[A-Za-z0-9]* = [0-9a-f]*\n)*\n");
            var regExProperty = new Regex(@"(?<prop>[A-Za-z0-9]*) = (?<val>[0-9a-f]*)");


            input = input.Trim();
            //add a pair of new lines to ensure that the last case is matched
            input = input + "\n\n";

            //the .where clause will filter out empty matches
            var matches = regExTestCase.Matches(input).Where(x => x.Value.Trim() != "");

            foreach (Match match in matches) //ignore a potential empty match
            {

                //create bitstring for all the non-null properties
                var properties = regExProperty.Matches(match.Value.Trim());
                var propertiesMap = new Dictionary<string, BitString>()
                {
                    { "KEYs", null},
                    { "KEY1", null},
                    { "KEY2", null},
                    { "KEY3", null},
                    { "IV1", null},
                    { "IV2", null},
                    { "IV3", null},
                    { "PLAINTEXT", null},
                    { "PLAINTEXT1", null},
                    { "PLAINTEXT2", null},
                    { "PLAINTEXT3", null},
                    { "CIPHERTEXT", null},
                    { "CIPHERTEXT1", null},
                    { "CIPHERTEXT2", null},
                    { "CIPHERTEXT3", null}
                };
                foreach (Match property in properties)
                {
                    var prop = property.Groups["prop"].Value;
                    var val = property.Groups["val"].Value;
                    if (prop != "COUNT")
                    {
                        BitString bitString;
                        //for plaintext and ciphertext that are not on the byte boundary
                        //cavs outputs strings as a serieos of zeros and ones
                        //this ensures that both formats can be parsed correctly
                        //checking for "TEXT" isn't ideal probably, but works
                        if (!isPtAndCtHex && prop.Contains("TEXT"))
                        {
                            bitString = new BitString(new BitArray(val.Reverse().Select(x => x == '1' || x == '0' ?
                                x == '1' :
                                throw new InvalidCastException()).ToArray()));
                        }
                        else
                        {
                            bitString = new BitString(val);

                        }
                        propertiesMap[prop] = bitString;
                    }
                }

                if (propertiesMap["KEYs"] == null)
                {
                    propertiesMap["KEYs"] = propertiesMap["KEY1"]
                        .ConcatenateBits(propertiesMap["KEY2"])
                        .ConcatenateBits(propertiesMap["KEY3"]);
                }
                if (isMCT)
                {
                    resultsArray.Add(new AlgoArrayResponse()
                    {
                        Keys = propertiesMap["KEYs"],
                        IV = propertiesMap["IV1"],
                        PlainText = propertiesMap["PLAINTEXT"],
                        CipherText = propertiesMap["CIPHERTEXT"],
                    });
                }
                else
                {
                    testCases.Add(new TestCase()
                    {
                        Keys = propertiesMap["KEYs"],
                        IV = propertiesMap["IV1"],
                        PlainText = propertiesMap["PLAINTEXT"],
                        PlainText1 = propertiesMap["PLAINTEXT1"],
                        PlainText2 = propertiesMap["PLAINTEXT2"],
                        PlainText3 = propertiesMap["PLAINTEXT3"],
                        CipherText = propertiesMap["CIPHERTEXT"],
                        CipherText1 = propertiesMap["CIPHERTEXT1"],
                        CipherText2 = propertiesMap["CIPHERTEXT2"],
                        CipherText3 = propertiesMap["CIPHERTEXT3"],
                    });

                }

            }
            return isMCT ?
                new List<TestCase> { new TestCase { ResultsArray = resultsArray } } :
                testCases;

        }

        private TestGroup GetTestGroupFromFileName(string file)
        {
            var fileNameRegex = new Regex(@"(?:TCFBP\d\d?)(?<type>[a-zA-Z]*)(?<keyingOption>\d)?(?:\.fax)");

            var fileNameToTestType = new Dictionary<string, string>()
            {
                {"invperm", "InversePermutation"},
                {"MMT", "MultiBlockMessage"},
                {"Monte", "MCT"},
                {"permop", "Permutation"},
                {"subtab", "SubstitutionTable"},
                {"varkey", "VariableKey"},
                {"vartext", "VariableText"}
            };

            var match = fileNameRegex.Matches(file)[0];
            var testType = fileNameToTestType[match.Groups["type"].Value];
            var keyingOption = match.Groups["keyingOption"].Value;

            var testGroup = new TestGroup
            {
                TestType = testType,
                KeyingOption = keyingOption != "" ? TdesHelpers.GetKeyingOptionFromNumberOfKeys(int.Parse(keyingOption)) : 1
            };

            return testGroup;
        }


        //protected TestGroup CreateTestGroup(string path)
        //{
        //    var testGroup = new TestGroup();
        //    var fileName = Path.GetFileName(path).ToLower();

        //    //parse direction
        //    if (fileName.Contains("gen"))
        //    {
        //        testGroup.Function = "gen";
        //    }
        //    else if (fileName.Contains("ver"))
        //    {
        //        testGroup.Function = "ver";
        //    }
        //    else
        //    {
        //        throw new Exception("Could not determine direction from filename");
        //    }

        //    //parse key size
        //    if (fileName.Contains("128"))
        //    {
        //        testGroup.KeyLength = 128;
        //        testGroup.CmacType = CmacTypes.AES128;
        //    }
        //    else if (fileName.Contains("192"))
        //    {
        //        testGroup.KeyLength = 192;
        //        testGroup.CmacType = CmacTypes.AES192;

        //    }
        //    else if (fileName.Contains("256"))
        //    {
        //        testGroup.KeyLength = 256;
        //        testGroup.CmacType = CmacTypes.AES256;
        //    }
        //    else
        //    {
        //        throw new Exception("Could not determine keysize from filename.");
        //    }
        //    testGroup.Tests = new List<ITestCase>();
        //    return testGroup;
        //}
    }
}

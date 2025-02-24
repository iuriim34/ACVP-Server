﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.RSA.Enums;
using NIST.CVP.ACVTS.Libraries.Generation.Core;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Parsers;

namespace NIST.CVP.ACVTS.Libraries.Generation.RSA.v1_0.KeyGen.Parsers
{
    public class LegacyResponseFileParser : ILegacyResponseFileParser<TestVectorSet, TestGroup, TestCase>
    {
        public ParseResponse<TestVectorSet> Parse(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new ParseResponse<TestVectorSet>("There was no path supplied.");
            }

            if (!File.Exists(path))
            {
                return new ParseResponse<TestVectorSet>($"Could not find file: {path}");
            }

            var lines = new List<string>();
            try
            {
                lines = File.ReadAllLines(path).ToList();
            }
            catch (Exception ex)
            {
                return new ParseResponse<TestVectorSet>(ex.Message);
            }

            var groups = new List<TestGroup>();
            TestGroup curGroup = null;
            TestCase curTestCase = null;
            var inCases = false;
            var caseId = 1;

            foreach (var line in lines)
            {
                var workingLine = line.Trim();
                if (string.IsNullOrEmpty(workingLine))
                {
                    continue;
                }

                if (workingLine.StartsWith("#"))
                {
                    continue;
                }

                if (workingLine.StartsWith("["))
                {
                    if (curGroup == null || inCases)
                    {
                        inCases = false;
                        curGroup = new TestGroup { PubExp = PublicExponentModes.Random };
                        groups.Add(curGroup);
                    }

                    workingLine = workingLine.Replace("[", "").Replace("]", "");
                    var propParts = workingLine.Split("=".ToCharArray());
                    curGroup.SetString(propParts[0].Trim(), propParts[1].Trim());
                    continue;
                }

                if (workingLine.StartsWith("e = "))
                {
                    // If this is the first e after a [] start launching test cases
                    curTestCase = new TestCase { TestCaseId = caseId };
                    inCases = true;

                    // Each e is a new test case
                    curGroup.Tests.Add(curTestCase);
                    caseId++;
                }

                var parts = workingLine.Split("=".ToCharArray());
                curTestCase.SetString(parts[0].Trim(), parts[1].Trim());
            }

            return new ParseResponse<TestVectorSet>(new TestVectorSet { Algorithm = "RSA", Mode = "keyGen", TestGroups = groups });
        }
    }
}

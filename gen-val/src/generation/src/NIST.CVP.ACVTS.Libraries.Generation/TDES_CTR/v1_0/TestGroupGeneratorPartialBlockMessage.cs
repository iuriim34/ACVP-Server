﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NIST.CVP.ACVTS.Libraries.Generation.Core;

namespace NIST.CVP.ACVTS.Libraries.Generation.TDES_CTR.v1_0
{
    public class TestGroupGeneratorPartialBlockMessage : ITestGroupGeneratorAsync<Parameters, TestGroup, TestCase>
    {
        private const string TEST_TYPE = "AFT";
        private const string INTERNAL_TEST_TYPE = "partialblock";

        public Task<List<TestGroup>> BuildTestGroupsAsync(Parameters parameters)
        {
            var testGroups = new List<TestGroup>();

            if (parameters.PayloadLen.ContainsValueOtherThan(128))
            {
                foreach (var direction in parameters.Direction)
                {
                    foreach (var keyingOption in parameters.KeyingOption)
                    {
                        if (direction.ToLower() == "encrypt" && keyingOption == 2)
                        {
                            // Don't allow encrypt on key option 2
                            continue;
                        }

                        var testGroup = new TestGroup
                        {
                            Direction = direction,
                            KeyingOption = keyingOption,

                            // Only test case generator that cares about this information
                            PayloadLength = parameters.PayloadLen,

                            TestType = TEST_TYPE,
                            InternalTestType = INTERNAL_TEST_TYPE
                        };

                        testGroups.Add(testGroup);
                    }
                }
            }

            return Task.FromResult(testGroups);
        }
    }
}

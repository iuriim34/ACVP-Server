﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NIST.CVP.ACVTS.Libraries.Common.Helpers;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.DSA.Ed;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.DSA.Ed.Enums;
using NIST.CVP.ACVTS.Libraries.Generation.Core;
using NIST.CVP.ACVTS.Libraries.Oracle.Abstractions;
using NIST.CVP.ACVTS.Libraries.Oracle.Abstractions.ParameterTypes;

namespace NIST.CVP.ACVTS.Libraries.Generation.EDDSA.v1_0.SigGen
{
    public class TestGroupGeneratorBitFlip : ITestGroupGeneratorAsync<Parameters, TestGroup, TestCase>
    {
        private const string TEST_TYPE = "BFT";

        private readonly IOracle _oracle;

        public TestGroupGeneratorBitFlip(IOracle oracle)
        {
            _oracle = oracle;
        }

        public async Task<List<TestGroup>> BuildTestGroupsAsync(Parameters parameters)
        {
            // Use a hash set because the registration allows for duplicate pairings to occur
            // Equality of groups is done via name of the curve and name of the hash function.
            // HashSet eliminates any duplicates that may be registered
            var testGroups = new HashSet<TestGroup>();

            foreach (var curveName in parameters.Curve)
            {
                var curve = EnumHelpers.GetEnumFromEnumDescription<Curve>(curveName);

                EdKeyPair key = null;
                var param = new EddsaKeyParameters
                {
                    Curve = curve
                };

                if (parameters.IsSample)
                {
                    var keyResult = await _oracle.GetEddsaKeyAsync(param);
                    key = keyResult.Key;
                }

                var paramMsg = new EddsaMessageParameters
                {
                    IsSample = parameters.IsSample
                };

                var message = await _oracle.GetEddsaMessageBitFlipAsync(paramMsg);

                if (parameters.Pure)
                {
                    var testGroup = new TestGroup
                    {
                        Curve = curve,
                        PreHash = false,
                        KeyPair = key,
                        Message = message,
                        TestType = TEST_TYPE
                    };
                    testGroups.Add(testGroup);
                }

                if (parameters.PreHash)
                {
                    var testGroup = new TestGroup
                    {
                        Curve = curve,
                        PreHash = true,
                        KeyPair = key,
                        Message = message,
                        TestType = TEST_TYPE
                    };
                    testGroups.Add(testGroup);
                }
            }

            return testGroups.ToList();
        }
    }
}

﻿using System;
using System.Threading.Tasks;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.RSA.Keys;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Enums;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KAS.Helpers;
using NIST.CVP.ACVTS.Libraries.Generation.Core;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Async;
using NIST.CVP.ACVTS.Libraries.Oracle.Abstractions;
using NIST.CVP.ACVTS.Libraries.Oracle.Abstractions.ParameterTypes.Kas.Sp800_56Br2;
using NLog;

namespace NIST.CVP.ACVTS.Libraries.Generation.KAS_SSC.Sp800_56Br2.Ifc
{
    public class TestCaseGeneratorAft : ITestCaseGeneratorAsync<TestGroup, TestCase>
    {
        private readonly IOracle _oracle;

        public TestCaseGeneratorAft(IOracle oracle)
        {
            _oracle = oracle;
        }

        public int NumberOfTestCasesToGenerate => 10;

        public async Task<TestCaseGenerateResponse<TestGroup, TestCase>> GenerateAsync(TestGroup @group, bool isSample, int caseNo = -1)
        {
            try
            {
                var serverRequirements = KeyGenerationRequirementsHelper.GetKeyGenerationOptionsForSchemeAndRole(
                    group.Scheme, group.KasMode,
                    KeyGenerationRequirementsHelper.GetOtherPartyKeyAgreementRole(group.KasRole),
                    KeyConfirmationRole.None,
                    KeyConfirmationDirection.None);

                var iutRequirements = KeyGenerationRequirementsHelper.GetKeyGenerationOptionsForSchemeAndRole(
                    group.Scheme, group.KasMode,
                    group.KasRole,
                    KeyConfirmationRole.None,
                    KeyConfirmationDirection.None);

                KeyPair serverKey = serverRequirements.GeneratesEphemeralKeyPair ? group.ShuffleKeys.Pop() : null;
                KeyPair iutKey = iutRequirements.GeneratesEphemeralKeyPair ? group.ShuffleKeys.Pop() : null;

                var result = await _oracle.GetKasIfcSscAftTestAsync(new KasSscAftParametersIfc()
                {
                    IsSample = isSample,
                    Modulo = group.Modulo,
                    PublicExponent = group.PublicExponent,
                    Scheme = group.Scheme,
                    KasMode = group.KasMode,
                    KeyGenerationMethod = group.KeyGenerationMethod,
                    IutKeyAgreementRole = group.KasRole,
                    ServerGenerationRequirements = group.ServerRequirements,
                    IutGenerationRequirements = group.IutRequirements,

                    ServerKeyPair = serverKey,
                    IutKeyPair = iutKey,
                });

                return new TestCaseGenerateResponse<TestGroup, TestCase>(new TestCase()
                {
                    Deferred = true,
                    TestPassed = true,
                    ServerC = result.ServerC,
                    ServerZ = result.ServerZ,
                    ServerKey = result.ServerKeyPair ?? new KeyPair() { PubKey = new PublicKey() },
                    IutKey = result.IutKeyPair ?? new KeyPair() { PubKey = new PublicKey() },
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new TestCaseGenerateResponse<TestGroup, TestCase>(ex.Message);
            }
        }

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    }
}

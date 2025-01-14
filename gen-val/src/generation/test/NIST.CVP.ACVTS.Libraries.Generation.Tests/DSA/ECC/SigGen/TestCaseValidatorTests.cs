﻿using System.Threading.Tasks;
using Moq;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.DSA.ECC;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.DSA.ECC.Enums;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Async;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Enums;
using NIST.CVP.ACVTS.Libraries.Generation.ECDSA.v1_0.SigGen;
using NIST.CVP.ACVTS.Libraries.Math;
using NIST.CVP.ACVTS.Tests.Core.TestCategoryAttributes;
using NUnit.Framework;

namespace NIST.CVP.ACVTS.Libraries.Generation.Tests.DSA.ECC.SigGen
{
    [TestFixture, UnitTest]
    public class TestCaseValidatorTests
    {
        [Test]
        public async Task ShouldRunVerifyMethodAndSucceedWithGoodSignature()
        {
            var subject = new TestCaseValidator(GetTestCase(), GetTestGroup(), GetDeferredResolver(true).Object);
            var result = await subject.ValidateAsync(GetResultTestCase());

            Assert.AreEqual(Disposition.Passed, result.Result);
        }

        [Test]
        public async Task ShouldRunVerifyMethodAndFailWithBadSignature()
        {
            var subject = new TestCaseValidator(GetTestCase(), GetTestGroup(), GetDeferredResolver(false).Object);
            var result = await subject.ValidateAsync(GetResultTestCase());

            Assert.AreEqual(Disposition.Failed, result.Result);
        }

        private TestCase GetTestCase()
        {
            return new TestCase
            {
                TestCaseId = 1,
                Message = new BitString("BEEFFACE")
            };
        }

        private TestCase GetResultTestCase()
        {
            return new TestCase
            {
                TestCaseId = 1,
                Signature = new EccSignature(3, 4)
            };
        }

        private TestGroup GetTestGroup()
        {
            return new TestGroup
            {
                Curve = Curve.P192,
                HashAlg = new HashFunction(ModeValues.SHA2, DigestSizes.d256),
                KeyPair = new EccKeyPair(new EccPoint(1, 2))
            };
        }

        private Mock<IDeferredTestCaseResolverAsync<TestGroup, TestCase, EccVerificationResult>> GetDeferredResolver(bool shouldPass)
        {
            var goodResult = Task.FromResult(new EccVerificationResult());
            var badResult = Task.FromResult(new EccVerificationResult("fail"));

            var mock = new Mock<IDeferredTestCaseResolverAsync<TestGroup, TestCase, EccVerificationResult>>();
            mock
                .Setup(s => s.CompleteDeferredCryptoAsync(It.IsAny<TestGroup>(), It.IsAny<TestCase>(), It.IsAny<TestCase>()))
                .Returns(shouldPass ? goodResult : badResult);

            return mock;
        }
    }
}

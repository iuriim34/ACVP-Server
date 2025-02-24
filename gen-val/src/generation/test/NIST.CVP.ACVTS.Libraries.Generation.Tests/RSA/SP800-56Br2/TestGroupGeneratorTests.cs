﻿using System.Linq;
using System.Threading.Tasks;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.Asymmetric.RSA.Enums;
using NIST.CVP.ACVTS.Libraries.Generation.RSA.Sp800_56Br2.DpComponent;
using NIST.CVP.ACVTS.Tests.Core.TestCategoryAttributes;
using NUnit.Framework;

namespace NIST.CVP.ACVTS.Libraries.Generation.Tests.RSA.Sp800_56Br2
{
    [TestFixture, UnitTest]
    public class TestGroupGeneratorTests
    {
        [Test]
        [TestCase(new [] { 2048 })]
        [TestCase(new [] { 2048, 3072 })]
        [TestCase(new [] { 2048, 3072, 4096 })]
        public async Task ShouldCreateCorrectNumberOfTestGroupsByModulus(int[] modulus)
        {
            var parameters = new Parameters
            {
                IsSample = true,
                Modulus = modulus,
                KeyFormat = new [] { PrivateKeyModes.Standard },
            };

            var tgGen = new TestGroupGenerator();
            var groups = await tgGen.BuildTestGroupsAsync(parameters);
            
            Assert.AreEqual(modulus.Length, groups.Count, "modulus");
        }
        
        [Test]
        [TestCase(new [] { PrivateKeyModes.Standard })]
        [TestCase(new [] { PrivateKeyModes.Crt })]
        [TestCase(new [] { PrivateKeyModes.Standard, PrivateKeyModes.Crt })]
        public async Task ShouldCreateCorrectNumberOfTestGroupsByKeyFormat(PrivateKeyModes[] keyFormats)
        {
            var parameters = new Parameters
            {
                IsSample = true,
                Modulus = new [] { 2048 },
                KeyFormat = keyFormats,
            };

            var tgGen = new TestGroupGenerator();
            var groups = await tgGen.BuildTestGroupsAsync(parameters);
            
            Assert.AreEqual(keyFormats.Length * parameters.Modulus.Length, groups.Count, "keyFormats");
        }
    }
}

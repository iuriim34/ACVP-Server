﻿using System;
using System.Collections.Generic;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Async;

namespace NIST.CVP.ACVTS.Libraries.Generation.KeyWrap.v1_0
{
    public class TestCaseValidatorFactory<TTestVectorSet, TTestGroup, TTestCase> : ITestCaseValidatorFactoryAsync<TTestVectorSet, TTestGroup, TTestCase>
        where TTestVectorSet : TestVectorSetBase<TTestGroup, TTestCase>
        where TTestGroup : TestGroupBase<TTestGroup, TTestCase>
        where TTestCase : TestCaseBase<TTestGroup, TTestCase>, new()
    {
        public List<ITestCaseValidatorAsync<TTestGroup, TTestCase>> GetValidators(TTestVectorSet testVectorSet)
        {
            var list = new List<ITestCaseValidatorAsync<TTestGroup, TTestCase>>();

            foreach (var group in testVectorSet.TestGroups)
            {
                foreach (var test in group.Tests)
                {
                    var workingTest = (TTestCase)test;
                    if (group.Direction.Equals("encrypt", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new TestCaseValidatorEncrypt<TTestGroup, TTestCase>(workingTest));
                    }
                    if (group.Direction.Equals("decrypt", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new TestCaseValidatorDecrypt<TTestGroup, TTestCase>(workingTest));
                    }
                }
            }

            return list;
        }
    }
}

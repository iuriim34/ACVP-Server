﻿using System.Collections.Generic;
using NIST.CVP.ACVTS.Libraries.Common.ExtensionMethods;
using NIST.CVP.ACVTS.Libraries.Generation.Core;

namespace NIST.CVP.ACVTS.Libraries.Generation.DSA.v1_0.KeyGen
{
    public class ParameterValidator : ParameterValidatorBase, IParameterValidator<Parameters>
    {
        public static int[] VALID_L = { 2048, 3072 };
        public static int[] VALID_N = { 224, 256 };

        public ParameterValidateResponse Validate(Parameters parameters)
        {
            var errors = new List<string>();
            var result = "";

            if (errors.AddIfNotNullOrEmpty(ValidateArrayAtLeastOneItem(parameters.Capabilities, "Capabilities")))
            {
                return new ParameterValidateResponse(errors);
            }

            foreach (var capability in parameters.Capabilities)
            {
                result = ValidateValue(capability.L, VALID_L, "L");
                errors.AddIfNotNullOrEmpty(result);

                result = ValidateValue(capability.N, VALID_N, "N");
                errors.AddIfNotNullOrEmpty(result);

                if (!VerifyLenPair(capability.L, capability.N))
                {
                    errors.Add("Invalid L, N pair");
                }
            }

            return new ParameterValidateResponse(errors);
        }

        private bool VerifyLenPair(int L, int N)
        {
            if (L == 2048)
            {
                return (N == 224 || N == 256);
            }
            else if (L == 3072)
            {
                return (N == 256);
            }

            return false;
        }
    }
}

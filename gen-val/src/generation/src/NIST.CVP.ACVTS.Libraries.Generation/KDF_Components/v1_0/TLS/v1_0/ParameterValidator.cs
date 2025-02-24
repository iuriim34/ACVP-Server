﻿using System;
using System.Collections.Generic;
using System.Linq;
using NIST.CVP.ACVTS.Libraries.Common.ExtensionMethods;
using NIST.CVP.ACVTS.Libraries.Common.Helpers;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.KDF.Components.TLS.Enums;
using NIST.CVP.ACVTS.Libraries.Generation.Core;

namespace NIST.CVP.ACVTS.Libraries.Generation.KDF_Components.v1_0.TLS.v1_0
{
    public class ParameterValidator : ParameterValidatorBase, IParameterValidator<Parameters>
    {
        public static string[] VALID_HASH_ALGS = { "SHA2-256", "SHA2-384", "SHA2-512" };
        public static TlsModes[] VALID_TLS_VERSIONS = EnumHelpers.GetEnums<TlsModes>().Except(new[] { TlsModes.v12_extendedMasterSecret }).ToArray();
        public static int VALID_MAX_KEY_BLOCK_LENGTH = 1024;
        public static int VALID_MIN_KEY_BLOCK_LENGTH = 512;
        public ParameterValidateResponse Validate(Parameters parameters)
        {
            var errors = new List<string>();

            if (!parameters.Algorithm.Equals("kdf-components", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Incorrect algorithm");
            }

            if (!parameters.Mode.Equals("tls", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Incorrect mode");
            }

            string result;
            result = ValidateArray(parameters.TlsVersion, VALID_TLS_VERSIONS, "TLS Version");
            errors.AddIfNotNullOrEmpty(result);

            if (parameters.TlsVersion.Contains(TlsModes.v12))
            {
                result = ValidateArray(parameters.HashAlg, VALID_HASH_ALGS, "Hash Algs");
                errors.AddIfNotNullOrEmpty(result);
            }

            if (parameters.KeyBlockLength != null)
            {
                ValidateDomain(parameters.KeyBlockLength, errors, "Key Block Length", VALID_MIN_KEY_BLOCK_LENGTH, VALID_MAX_KEY_BLOCK_LENGTH);
                ValidateMultipleOf(parameters.KeyBlockLength, errors, 8, "Key Block Length multiple of 8");
            }
            
            return new ParameterValidateResponse(errors);
        }
    }
}

﻿using System;
using System.IO;
using Newtonsoft.Json;
using NIST.CVP.ACVTS.Libraries.Common;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Enums;
using NIST.CVP.ACVTS.Libraries.Generation.Core.Helpers;
using NIST.CVP.ACVTS.Generation.GenValApp.Models;

namespace NIST.CVP.ACVTS.Generation.GenValApp.Helpers
{
    public static class RunningOptionsHelper
    {
        /// <summary>
        /// Gets the running options for the GenValAppRunner
        /// </summary>
        /// <param name="parsedParameters">The parsed command line arguments.</param>
        /// <returns></returns>
        public static GenValRunningOptions GetRunningOptions(ArgumentParsingTarget parsedParameters)
        {
            var genValMode = DetermineRunningMode(parsedParameters);
            var algoMode = DetermineAlgoMode(parsedParameters, genValMode);

            return new GenValRunningOptions(algoMode, genValMode);
        }

        /// <summary>
        /// Determines if the runner is running for generation or validation, 
        /// determined via parsed command arguments.
        /// </summary>
        /// <param name="parsedParameters">The parsed command line arguments.</param>
        public static GenValMode DetermineRunningMode(ArgumentParsingTarget parsedParameters)
        {
            if (parsedParameters.CapabilitiesFile != null)
            {
                return GenValMode.Check;
            }
            if (parsedParameters.RegistrationFile != null)
            {
                return GenValMode.Generate;
            }
            if (parsedParameters.AnswerFile != null && parsedParameters.ResponseFile != null)
            {
                return GenValMode.Validate;
            }

            return GenValMode.Unset;
        }

        /// <summary>
        /// Determine the <see cref="AlgoMode"/> from the <see cref="ArgumentParsingTarget"/> and <see cref="GenValMode"/>.
        /// </summary>
        /// <param name="parsedParameters">The parsed command line arguments.</param>
        /// <param name="genValMode">The running mode.</param>
        /// <returns></returns>
        public static AlgoMode DetermineAlgoMode(ArgumentParsingTarget parsedParameters, GenValMode genValMode)
        {
            switch (genValMode)
            {
                case GenValMode.Check:
                    var capabilities = JsonConvert.DeserializeObject<ParametersBase>(File.ReadAllText(parsedParameters.CapabilitiesFile.FullName));
                    return AlgoModeLookupHelper.GetAlgoModeFromStrings(capabilities.Algorithm, capabilities.Mode, capabilities.Revision);
                case GenValMode.Generate:
                    var parameters = JsonConvert.DeserializeObject<ParametersBase>(File.ReadAllText(parsedParameters.RegistrationFile.FullName));
                    return AlgoModeLookupHelper.GetAlgoModeFromStrings(parameters.Algorithm, parameters.Mode, parameters.Revision);
                case GenValMode.Validate:
                    var internalProjection = JsonConvert.DeserializeObject<TestVectorSetBase>(File.ReadAllText(parsedParameters.AnswerFile.FullName));
                    return AlgoModeLookupHelper.GetAlgoModeFromStrings(internalProjection.Algorithm, internalProjection.Mode, internalProjection.Revision);
                default:
                    throw new ArgumentException(nameof(genValMode));
            }
        }
    }
}

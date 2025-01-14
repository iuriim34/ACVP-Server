﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NIST.CVP.ACVTS.Libraries.Math.JsonConverters;
using NLog;

namespace NIST.CVP.ACVTS.Libraries.Generation.Core.Parsers
{
    public class DynamicParser : IDynamicParser
    {

        protected readonly IList<JsonConverter> _jsonConverters = new List<JsonConverter>();

        public DynamicParser()
        {
            _jsonConverters.Add(new BitstringConverter());
            _jsonConverters.Add(new BigIntegerConverter());
        }

        public ParseResponse<dynamic> Parse(string path)
        {
            if (!File.Exists(path))
            {
                return new ParseResponse<dynamic>($"Could not find file: {path}");
            }

            try
            {
                var parameters = JsonConvert.DeserializeObject<dynamic>(
                    File.ReadAllText(path),
                    new JsonSerializerSettings
                    {
                        Converters = _jsonConverters
                    });
                return new ParseResponse<dynamic>(parameters);
            }
            catch (Exception ex)
            {
                ThisLogger.Error(ex);
                return new ParseResponse<dynamic>($"Could not parse file: {path}");
            }
        }
        private Logger ThisLogger
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }
    }
}

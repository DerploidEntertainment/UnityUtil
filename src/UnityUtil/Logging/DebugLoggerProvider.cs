﻿using System;
using System.Text;

namespace UnityEngine.Logging
{
    public class DebugLoggerProvider : Configurable, ILoggerProvider
    {
        public string EnrichedLogSeparator = " | ";
        public LogEnricher[] LogEnrichers = Array.Empty<LogEnricher>();

        public ILogger GetLogger(object source)
        {
            return (source == null) ? throw new ArgumentNullException(nameof(source)) : new DebugLogger(enrich);


            string enrich() {
                var sb = new StringBuilder();
                for (int e = 0; e < LogEnrichers.Length; ++e) {
                    string log = LogEnrichers[e].GetEnrichedLog(source);
                    if (!string.IsNullOrEmpty(log))
                        sb.Append(log).Append(EnrichedLogSeparator);
                }

                return sb.ToString();
            }
        }

    }
}

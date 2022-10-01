using Moq;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Logging;
using UnityUtil.Configuration;
using UnityUtil.Editor;

namespace UnityUtil.Test.EditMode.Logging
{
    public class DebugLoggerProviderTest : BaseEditModeTestFixture
    {
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated when added to test Game Objects")]
        private class HerpLogEnricher : LogEnricher
        {
            public override string GetEnrichedLog(object source) => "Herp";
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated when added to test Game Objects")]
        private class DerpLogEnricher : LogEnricher
        {
            public override string GetEnrichedLog(object source) => "Derp";
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated when added to test Game Objects")]
        private class SourceNameLogEnricher : LogEnricher
        {
            public override string GetEnrichedLog(object source) => (source as GameObject)?.name ?? "";
        }

        [Test]
        public void CanEnrichLogs_DiffEnrichers()
        {
            HerpLogEnricher herpEnricher = ScriptableObject.CreateInstance<HerpLogEnricher>();
            DerpLogEnricher derpEnricher = ScriptableObject.CreateInstance<DerpLogEnricher>();
            var sourceObj = new GameObject("source");
            DebugLoggerProvider loggerProvider;
            ILogger logger;
            string msg;

            loggerProvider = getDebugLoggerProvider(logEnrichers: herpEnricher);
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Herp | {msg}");

            loggerProvider = getDebugLoggerProvider(logEnrichers: derpEnricher);
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Derp | {msg}");

            loggerProvider = getDebugLoggerProvider(logEnrichers: new LogEnricher[] { herpEnricher, derpEnricher });
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Herp | Derp | {msg}");
        }

        [Test]
        public void CanEnrichLogs_DiffSeparators()
        {
            HerpLogEnricher herpEnricher = ScriptableObject.CreateInstance<HerpLogEnricher>();
            DerpLogEnricher derpEnricher = ScriptableObject.CreateInstance<DerpLogEnricher>();
            var sourceObj = new GameObject("source");
            DebugLoggerProvider loggerProvider;
            ILogger logger;
            string msg;

            loggerProvider = getDebugLoggerProvider(separator: " | ", herpEnricher, derpEnricher);
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Herp | Derp | {msg}");

            loggerProvider = getDebugLoggerProvider(separator: " ", herpEnricher, derpEnricher);
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Herp Derp {msg}");

            loggerProvider = getDebugLoggerProvider(separator: "->", herpEnricher, derpEnricher);
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Herp->Derp->{msg}");
        }

        [Test]
        public void CanEnrichLogs_DiffSourceObjects()
        {
            SourceNameLogEnricher sourceNameEnricher = ScriptableObject.CreateInstance<SourceNameLogEnricher>();
            DebugLoggerProvider loggerProvider = getDebugLoggerProvider(logEnrichers: sourceNameEnricher);
            GameObject sourceObj;
            ILogger logger;
            string msg;

            sourceObj = new GameObject("source");
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"source | {msg}");

            sourceObj = new GameObject("object");
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"object | {msg}");

            sourceObj = new GameObject("something");
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"something | {msg}");
        }

        [Test]
        public void LogEnricherOrderPreserved()
        {
            HerpLogEnricher herpEnricher = ScriptableObject.CreateInstance<HerpLogEnricher>();
            DerpLogEnricher derpEnricher = ScriptableObject.CreateInstance<DerpLogEnricher>();
            var sourceObj = new GameObject("source");
            DebugLoggerProvider loggerProvider;
            ILogger logger;
            string msg;

            loggerProvider = getDebugLoggerProvider(logEnrichers: new LogEnricher[] { herpEnricher, derpEnricher });
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Herp | Derp | {msg}");

            loggerProvider = getDebugLoggerProvider(logEnrichers: new LogEnricher[] { derpEnricher, herpEnricher });
            logger = loggerProvider.GetLogger(sourceObj);
            msg = EditModeTestHelpers.GetUniqueLog("message");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Derp | Herp | {msg}");
        }

        private static DebugLoggerProvider getDebugLoggerProvider(string separator = " | ", params LogEnricher[] logEnrichers)
        {
            var obj = new GameObject();
            DebugLoggerProvider loggerProvider = obj.AddComponent<DebugLoggerProvider>();
            loggerProvider.Inject(Mock.Of<IConfigurator>(), loggerProvider);
            loggerProvider.EnrichedLogSeparator = separator;
            loggerProvider.LogEnrichers = logEnrichers;

            return loggerProvider;
        }

    }

}

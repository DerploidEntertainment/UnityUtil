using NUnit.Framework;
using UnityEngine;
using UnityEngine.Logging;
using UnityUtil.Editor;

namespace UnityUtil.Test.EditMode.Logging {

    public class DebugLoggerProviderTest {

        private class HerpLogEnricher : LogEnricher {
            public override string GetEnrichedLog(object source) => "Herp";
        }
        private class DerpLogEnricher : LogEnricher {
            public override string GetEnrichedLog(object source) => "Derp";
        }
        private class SourceNameLogEnricher : LogEnricher {
            public override string GetEnrichedLog(object source) => (source as GameObject)?.name ?? "";
        }

        [Test]
        public void CanEnrichLogs_DiffEnrichers() {
            EditModeTestHelpers.ResetScene();

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
        public void CanEnrichLogs_DiffSeparators() {
            EditModeTestHelpers.ResetScene();

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
        public void CanEnrichLogs_DiffSourceObjects() {
            EditModeTestHelpers.ResetScene();

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
        public void LogEnricherOrderPreserved() {
            EditModeTestHelpers.ResetScene();

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

        private DebugLoggerProvider getDebugLoggerProvider(string separator = " | ", params LogEnricher[] logEnrichers) {
            var obj = new GameObject();
            Configurator configurator = obj.AddComponent<Configurator>();

            DebugLoggerProvider loggerProvider = obj.AddComponent<DebugLoggerProvider>();
            loggerProvider.Inject(configurator, loggerProvider);
            loggerProvider.EnrichedLogSeparator = separator;
            loggerProvider.LogEnrichers = logEnrichers;

            return loggerProvider;
        }

    }

}

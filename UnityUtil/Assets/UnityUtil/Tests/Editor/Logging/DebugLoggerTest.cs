using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityUtil.Logging;
using static System.Globalization.CultureInfo;

namespace UnityUtil.Editor.Tests.Logging
{

    [SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "They're just for testing...")]
    public class DebugLoggerTest : BaseEditModeTestFixture
    {

        [Test]
        public void CanEnrichLogs()
        {
            var testObj = new GameObject("test");
            string[] enrichTxts = { "Enriched0", "Enriched1", "Enriched2" };

            for (int e = 0; e < enrichTxts.Length; ++e) {
                string enrichTxt = enrichTxts[e];
                var logger = new DebugLogger(() => $"{enrichTxt} | ");
                string msg;

                // LogType methods
                msg = EditModeTestHelpers.GetUniqueLog("Log+Type");
                logger.Log(LogType.Log, msg);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Log+Type+Context");
                logger.Log(LogType.Log, msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Log+Type");
                logger.Log(LogType.Log, tag: "Tag", msg);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Log+Type+Context");
                logger.Log(LogType.Log, tag: "Tag", msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {enrichTxt} | {msg}");

                // Log methods
                msg = EditModeTestHelpers.GetUniqueLog("Log");
                logger.Log(msg);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Log+Context");
                logger.Log(msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Log");
                logger.Log(tag: "Tag", msg);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Log+Context");
                logger.Log(tag: "Tag", msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {enrichTxt} | {msg}");

                // LogFormat methods
                string format = EditModeTestHelpers.GetUniqueLog("Log {0} {1}");
                logger.LogFormat(LogType.Log, format, "A", 5);
                EditModeTestHelpers.ExpectLog(LogType.Log, $"{enrichTxt} | {string.Format(InvariantCulture, format, "A", 5)}");

                format = EditModeTestHelpers.GetUniqueLog("Log {0} {1} {2}");
                logger.LogFormat(LogType.Log, context: testObj, format, "A", 5, "Context");
                EditModeTestHelpers.ExpectLog(LogType.Log, $"{enrichTxt} | {string.Format(InvariantCulture, format, "A", 5, "Context")}");

                // LogError methods
                msg = EditModeTestHelpers.GetUniqueLog("Error");
                logger.LogError(msg);
                EditModeTestHelpers.ExpectLog(LogType.Error, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Error+Context");
                logger.LogError(msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Error, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Error");
                logger.LogError(tag: "Tag", msg);
                EditModeTestHelpers.ExpectLog(LogType.Error, $"Tag: {enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Error+Context");
                logger.LogError(tag: "Tag", msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Error, $"Tag: {enrichTxt} | {msg}");

                // LogWarning methods
                msg = EditModeTestHelpers.GetUniqueLog("Warning");
                logger.LogWarning(msg);
                EditModeTestHelpers.ExpectLog(LogType.Warning, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Warning+Context");
                logger.LogWarning(msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Warning, $"{enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Warning");
                logger.LogWarning(tag: "Tag", msg);
                EditModeTestHelpers.ExpectLog(LogType.Warning, $"Tag: {enrichTxt} | {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Warning+Context");
                logger.LogWarning(tag: "Tag", msg, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Warning, $"Tag: {enrichTxt} | {msg}");
            }
        }

        [Test]
        public void CannotEnrichExceptionLogs()
        {
            var testObj = new GameObject("test");
            string[] enrichTxts = { "Enriched0", "Enriched1", "Enriched2" };
            string msg;
            Exception ex;

            for (int e = 0; e < enrichTxts.Length; ++e) {
                string enrichTxt = enrichTxts[e];
                var logger = new DebugLogger(() => $"{enrichTxt} | ");

                msg = EditModeTestHelpers.GetUniqueLog("Test exception");
                ex = new Exception(msg);
                logger.LogException(ex);
                EditModeTestHelpers.ExpectLog(LogType.Exception, $"Exception: {msg}");

                msg = EditModeTestHelpers.GetUniqueLog("Test exception");
                ex = new Exception(msg);
                logger.LogException(ex, context: testObj);
                EditModeTestHelpers.ExpectLog(LogType.Exception, $"Exception: {msg}");
            }
        }

        [Test]
        public void LogTypeMethodsLogToConsole()
        {
            var logger = new DebugLogger(() => string.Empty);
            var testObj = new GameObject("test");
            string msg;

            // LogType.Log
            msg = EditModeTestHelpers.GetUniqueLog("Log");
            logger.Log(LogType.Log, msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Log+Context");
            logger.Log(LogType.Log, msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Log, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Log");
            logger.Log(LogType.Log, tag: "Tag", msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {msg}");

            msg = EditModeTestHelpers.GetUniqueLog("Log+Context");
            logger.Log(LogType.Log, tag: "Tag", msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {msg}");

            // LogType.Warning
            msg = EditModeTestHelpers.GetUniqueLog("Warning");
            logger.Log(LogType.Warning, msg);
            EditModeTestHelpers.ExpectLog(LogType.Warning, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Warning+Context");
            logger.Log(LogType.Warning, msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Warning, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Warning");
            logger.Log(LogType.Warning, tag: "Tag", msg);
            EditModeTestHelpers.ExpectLog(LogType.Warning, $"Tag: {msg}");

            msg = EditModeTestHelpers.GetUniqueLog("Warning+Context");
            logger.Log(LogType.Warning, tag: "Tag", msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Warning, $"Tag: {msg}");
        }

        [Test]
        public void LogMethodsLogToConsole()
        {
            var logger = new DebugLogger(() => string.Empty);
            var testObj = new GameObject("test");
            string msg;

            msg = EditModeTestHelpers.GetUniqueLog("Log");
            logger.Log(msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Log+Context");
            logger.Log(msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Log, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Log");
            logger.Log(tag: "Tag", msg);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {msg}");

            msg = EditModeTestHelpers.GetUniqueLog("Log+Context");
            logger.Log(tag: "Tag", msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Log, $"Tag: {msg}");
        }

        [Test]
        public void LogFormatMethodsLogToConsole()
        {
            var logger = new DebugLogger(() => string.Empty);
            var testObj = new GameObject("test");
            string format;

            format = EditModeTestHelpers.GetUniqueLog("Log {0} {1}");
            logger.LogFormat(LogType.Log, format, "A", 5);
            EditModeTestHelpers.ExpectLog(LogType.Log, string.Format(InvariantCulture, format, "A", 5));

            format = EditModeTestHelpers.GetUniqueLog("Warning {0} {1}");
            logger.LogFormat(LogType.Warning, format, "A", 5);
            EditModeTestHelpers.ExpectLog(LogType.Warning, string.Format(InvariantCulture, format, "A", 5));

            format = EditModeTestHelpers.GetUniqueLog("Log {0} {1}");
            logger.LogFormat(LogType.Log, context: testObj, format, "A", 5, "Context");
            EditModeTestHelpers.ExpectLog(LogType.Log, string.Format(InvariantCulture, format, "A", 5, "Context"));

            format = EditModeTestHelpers.GetUniqueLog("Warning {0} {1}");
            logger.LogFormat(LogType.Warning, context: testObj, format, "A", 5, "Context");
            EditModeTestHelpers.ExpectLog(LogType.Warning, string.Format(InvariantCulture, format, "A", 5, "Context"));
        }

        [Test]
        public void ErrorMethodsLogToConsole()
        {
            var logger = new DebugLogger(() => string.Empty);
            var testObj = new GameObject("test");
            string msg;

            msg = EditModeTestHelpers.GetUniqueLog("Error");
            logger.LogError(msg);
            EditModeTestHelpers.ExpectLog(LogType.Error, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Error+Context");
            logger.LogError(msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Error, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Error");
            logger.LogError(tag: "Tag", msg);
            EditModeTestHelpers.ExpectLog(LogType.Error, $"Tag: {msg}");

            msg = EditModeTestHelpers.GetUniqueLog("Error+Context");
            logger.LogError(tag: "Tag", msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Error, $"Tag: {msg}");
        }

        [Test]
        public void ExceptionMethodsLogToConsole()
        {
            var logger = new DebugLogger(() => string.Empty);
            var testObj = new GameObject("test");
            string msg;
            Exception ex;

            msg = EditModeTestHelpers.GetUniqueLog("Test exception");
            ex = new Exception(msg);
            logger.LogException(ex);
            EditModeTestHelpers.ExpectLog(LogType.Exception, $"Exception: {msg}");

            msg = EditModeTestHelpers.GetUniqueLog("Test exception");
            ex = new Exception(msg);
            logger.LogException(ex, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Exception, $"Exception: {msg}");
        }

        [Test]
        public void WarningMethodsLogToConsole()
        {
            var logger = new DebugLogger(() => string.Empty);
            var testObj = new GameObject("test");
            string msg;

            msg = EditModeTestHelpers.GetUniqueLog("Warning");
            logger.LogWarning(msg);
            EditModeTestHelpers.ExpectLog(LogType.Warning, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Warning+Context");
            logger.LogWarning(msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Warning, msg);

            msg = EditModeTestHelpers.GetUniqueLog("Warning");
            logger.LogWarning(tag: "Tag", msg);
            EditModeTestHelpers.ExpectLog(LogType.Warning, $"Tag: {msg}");

            msg = EditModeTestHelpers.GetUniqueLog("Warning+Context");
            logger.LogWarning(tag: "Tag", msg, context: testObj);
            EditModeTestHelpers.ExpectLog(LogType.Warning, $"Tag: {msg}");
        }

        [Test]
        public void CanAccessNonLoggingMembers()
        {
            var logger = new DebugLogger(() => string.Empty);

            Assert.That(logger.logHandler, Is.SameAs(Debug.unityLogger.logHandler));

            logger.logEnabled = false;
            Assert.That(logger.logEnabled, Is.EqualTo(Debug.unityLogger.logEnabled));
            logger.logEnabled = true;
            Assert.That(logger.logEnabled, Is.EqualTo(Debug.unityLogger.logEnabled));
        }

        [Test]
        public void CanFilterLogType()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            var logger = new DebugLogger(() => string.Empty);
#pragma warning restore IDE0017 // Simplify object initialization

            logger.filterLogType = LogType.Log;
            Assert.That(logger.filterLogType, Is.EqualTo(Debug.unityLogger.filterLogType));
            logger.filterLogType = LogType.Warning;
            Assert.That(logger.filterLogType, Is.EqualTo(Debug.unityLogger.filterLogType));

            logger.filterLogType = LogType.Error;
            Assert.That(logger.IsLogTypeAllowed(LogType.Error));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Assert));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Warning));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Log));
            Assert.That(logger.IsLogTypeAllowed(LogType.Exception));

            logger.filterLogType = LogType.Assert;
            Assert.That(logger.IsLogTypeAllowed(LogType.Error));
            Assert.That(logger.IsLogTypeAllowed(LogType.Assert));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Warning));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Log));
            Assert.That(logger.IsLogTypeAllowed(LogType.Exception));

            logger.filterLogType = LogType.Warning;
            Assert.That(logger.IsLogTypeAllowed(LogType.Error));
            Assert.That(logger.IsLogTypeAllowed(LogType.Assert));
            Assert.That(logger.IsLogTypeAllowed(LogType.Warning));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Log));
            Assert.That(logger.IsLogTypeAllowed(LogType.Exception));

            logger.filterLogType = LogType.Log;
            Assert.That(logger.IsLogTypeAllowed(LogType.Error));
            Assert.That(logger.IsLogTypeAllowed(LogType.Assert));
            Assert.That(logger.IsLogTypeAllowed(LogType.Warning));
            Assert.That(logger.IsLogTypeAllowed(LogType.Log));
            Assert.That(logger.IsLogTypeAllowed(LogType.Exception));

            logger.filterLogType = LogType.Exception;
            Assert.That(!logger.IsLogTypeAllowed(LogType.Error));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Assert));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Warning));
            Assert.That(!logger.IsLogTypeAllowed(LogType.Log));
            Assert.That(logger.IsLogTypeAllowed(LogType.Exception));
        }

    }

}

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityUtil.Editor {

    public static class EditModeTestHelpers {

        private static MethodInfo? s_clearConsoleMethod;
        private static uint s_numLogs;

        public static void ResetScene() {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            ClearEditorConsole();
            Debug.unityLogger.logEnabled = true;
            Debug.unityLogger.filterLogType = LogType.Log;
        }

        public static string GetUniqueLog(string message) => $"#{s_numLogs++} {message}";
        public static void ExpectLog(LogType logType, string message) {
            LogAssert.Expect(logType, message);
            LogAssert.NoUnexpectedReceived();
        }
        public static void ExpectLog(LogType logType, Regex message) {
            LogAssert.Expect(logType, message);
            LogAssert.NoUnexpectedReceived();
        }

        public static void ClearEditorConsole() {
            // See here: https://answers.unity.com/questions/578393/clear-console-through-code-in-development-build.html

            if (s_clearConsoleMethod is null) {
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                s_clearConsoleMethod = logEntries.GetMethod("Clear");
            }
            s_clearConsoleMethod.Invoke(new object(), null);
        }

    }

}

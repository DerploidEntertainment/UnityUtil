using System;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace HighHandHoldem.Unity
{
    public class GameCrasher : MonoBehaviour
    {
        public void ManagedException() => throw new Exception("AAHHH, MANAGED EXCEPTION!!!! JK, everything is fine.");

        public void ForceCrashAbort() => Utils.ForceCrash(ForcedCrashCategory.Abort);

        public void ForceCrashAccessViolation() => Utils.ForceCrash(ForcedCrashCategory.AccessViolation);

        public void ForceCrashFatalError() => Utils.ForceCrash(ForcedCrashCategory.FatalError);

        public void ForceCrashPureVirtualFunction() => Utils.ForceCrash(ForcedCrashCategory.PureVirtualFunction);

        public void NativeAssert() => Utils.NativeAssert("AAHHH NATIVE ASSERT!!! JK, everything is fine.");

        public void NativeError() => Utils.NativeError("AAHHH NATIVE ERROR!!! JK, everything is fine.");

        public void AndroidUncaughtException()
        {
            if (Application.platform != RuntimePlatform.Android) {
                Debug.LogWarning("Can't call the Android uncaught exception handler if we're not on Android, silly");
                return;
            }

            // Copied from Unity Forums: https://forum.unity.com/threads/how-to-force-crash-on-android-to-test-crash-reporting-systems.653845/
            // Itself from StackOverflow: https://stackoverflow.com/questions/17511070/android-force-crash-with-uncaught-exception-in-thread
            var message = new AndroidJavaObject("java.lang.String", "AAHHH ANDROID UNCAUGHT EXCEPTION!!! JK, everything is fine.");
            var exception = new AndroidJavaObject("java.lang.Exception", message);

            AndroidJavaObject mainThread = new AndroidJavaClass("android.os.Looper")
                .CallStatic<AndroidJavaObject>("getMainLooper")
                .Call<AndroidJavaObject>("getThread");
            AndroidJavaObject exceptionHandler = mainThread.Call<AndroidJavaObject>("getUncaughtExceptionHandler");
            exceptionHandler.Call("uncaughtException", mainThread, exception);
        }
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace UnityUtil;

public class GameCrasher : MonoBehaviour
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void UncaughtExceptionClr()
    {
        Debug.Log("Attempting crash via uncaught CLR exception...");
        throw new InvalidOperationException("AAHHH, MANAGED EXCEPTION!!!! JK, everything is fine.");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void ForceCrashAbort()
    {
        Debug.Log($"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.Abort)}...");
        Utils.ForceCrash(ForcedCrashCategory.Abort);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void ForceCrashAccessViolation()
    {
        Debug.Log($"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.AccessViolation)}...");
        Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void ForceCrashFatalError()
    {
        Debug.Log($"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.FatalError)}...");
        Utils.ForceCrash(ForcedCrashCategory.FatalError);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void ForceCrashPureVirtualFunction()
    {
        Debug.Log($"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.PureVirtualFunction)}...");
        Utils.ForceCrash(ForcedCrashCategory.PureVirtualFunction);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void NativeAssert()
    {
        Debug.Log("Attempting crash via native assert...");
        Utils.NativeAssert("AAHHH NATIVE ASSERT!!! JK, everything is fine.");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void NativeError()
    {
        Debug.Log("Attempting crash via native error...");
        Utils.NativeError("AAHHH NATIVE ERROR!!! JK, everything is fine.");
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void UncaughtExceptionAndroid()
    {
        if (Application.platform != RuntimePlatform.Android) {
            Debug.LogWarning("Can't call the Android uncaught exception handler on non-Android platform.");
            return;
        }

        Debug.Log("Attempting crash via uncaught Android exception...");

        // Copied from Unity Forums: https://forum.unity.com/threads/how-to-force-crash-on-android-to-test-crash-reporting-systems.653845/
        // Itself from StackOverflow: https://stackoverflow.com/questions/17511070/android-force-crash-with-uncaught-exception-in-thread
        var message = new AndroidJavaObject("java.lang.String", "AAHHH ANDROID UNCAUGHT EXCEPTION!!! JK, everything is fine.");
        var exception = new AndroidJavaObject("java.lang.Exception", message);

        using AndroidJavaClass looperClass = new("android.os.Looper");
        using AndroidJavaObject mainThread = looperClass.CallStatic<AndroidJavaObject>("getMainLooper").Call<AndroidJavaObject>("getThread");
        using AndroidJavaObject exceptionHandler = mainThread.Call<AndroidJavaObject>("getUncaughtExceptionHandler");
        exceptionHandler.Call("uncaughtException", mainThread, exception);
    }
}

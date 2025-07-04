using System;
using Microsoft.Extensions.Logging;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(ApplicationCrasher)}", fileName = "application-crasher")]
public class ApplicationCrasher : ScriptableObject
{
    private ILogger<ApplicationCrasher>? _logger;

    private ILogger<ApplicationCrasher> Logger =>
        _logger ??= (DependencyInjector.Instance.LoggerFactory ?? new UnityDebugLoggerFactory()).CreateLogger(this);

    public void UncaughtExceptionClr()
    {
        log_UncaughtExceptionClr();
        throw new InvalidOperationException("AAHHH, MANAGED EXCEPTION!!!! JK, everything is fine.");
    }

    public void ForceCrashAbort()
    {
        log_ForceCrash(ForcedCrashCategory.Abort);
        Utils.ForceCrash(ForcedCrashCategory.Abort);
    }

    public void ForceCrashMonoAbort()
    {
        log_ForceCrash(ForcedCrashCategory.MonoAbort);
        Utils.ForceCrash(ForcedCrashCategory.MonoAbort);
    }

    public void ForceCrashAccessViolation()
    {
        log_ForceCrash(ForcedCrashCategory.AccessViolation);
        Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
    }

    public void ForceCrashFatalError()
    {
        log_ForceCrash(ForcedCrashCategory.FatalError);
        Utils.ForceCrash(ForcedCrashCategory.FatalError);
    }

    public void ForceCrashPureVirtualFunction()
    {
        log_ForceCrash(ForcedCrashCategory.PureVirtualFunction);
        Utils.ForceCrash(ForcedCrashCategory.PureVirtualFunction);
    }

    public void NativeAssert()
    {
        log_NativeAssert();
        Utils.NativeAssert("AAHHH NATIVE ASSERT!!! JK, everything is fine.");
    }

    public void NativeError()
    {
        log_NativeError();
        Utils.NativeError("AAHHH NATIVE ERROR!!! JK, everything is fine.");
    }

    public void UncaughtExceptionAndroid()
    {
        if (Application.platform != RuntimePlatform.Android) {
            log_AndroidExceptionNotOnAndroid();
            return;
        }

        log_UncaughtExceptionAndroid();

        // Copied from Unity Forums: https://forum.unity.com/threads/how-to-force-crash-on-android-to-test-crash-reporting-systems.653845/
        // Itself from StackOverflow: https://stackoverflow.com/questions/17511070/android-force-crash-with-uncaught-exception-in-thread
        using var message = new AndroidJavaObject("java.lang.String", "AAHHH ANDROID UNCAUGHT EXCEPTION!!! JK, everything is fine.");
        using var exception = new AndroidJavaObject("java.lang.Exception", message);

        using AndroidJavaClass looperClass = new("android.os.Looper");
        using AndroidJavaObject mainThread = looperClass.CallStatic<AndroidJavaObject>("getMainLooper").Call<AndroidJavaObject>("getThread");
        using AndroidJavaObject exceptionHandler = mainThread.Call<AndroidJavaObject>("getUncaughtExceptionHandler");
        exceptionHandler.Call("uncaughtException", mainThread, exception);
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_UNCAUGHT_EXCEPTION_CLR_ACTION =
        LoggerMessage.Define(Information,
            new EventId(id: 0, nameof(log_UncaughtExceptionClr)),
            "Attempting crash via uncaught CLR exception..."
        );
    private void log_UncaughtExceptionClr() => LOG_UNCAUGHT_EXCEPTION_CLR_ACTION(Logger, null);


    private static readonly Action<MEL.ILogger, ForcedCrashCategory, Exception?> LOG_FORCE_CRASH_ACTION =
        LoggerMessage.Define<ForcedCrashCategory>(Information,
            new EventId(id: 0, nameof(log_ForceCrash)),
            $"Attempting crash via {nameof(ForcedCrashCategory)} {{{nameof(ForcedCrashCategory)}}}..."
        );
    private void log_ForceCrash(ForcedCrashCategory forcedCrashCategory) => LOG_FORCE_CRASH_ACTION(Logger, forcedCrashCategory, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_NATIVE_ASSERT_ACTION =
        LoggerMessage.Define(Information,
            new EventId(id: 0, nameof(log_NativeAssert)),
            "Attempting crash via native assert..."
        );
    private void log_NativeAssert() => LOG_NATIVE_ASSERT_ACTION(Logger, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_NATIVE_ERROR_ACTION =
        LoggerMessage.Define(Information,
            new EventId(id: 0, nameof(log_NativeError)),
            "Attempting crash via native error..."
        );
    private void log_NativeError() => LOG_NATIVE_ERROR_ACTION(Logger, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_UNCAUGHT_EXCEPTION_ANDROID_ACTION =
        LoggerMessage.Define(Information,
            new EventId(id: 0, nameof(log_UncaughtExceptionAndroid)),
            "Attempting crash via uncaught Android exception..."
        );
    private void log_UncaughtExceptionAndroid() => LOG_UNCAUGHT_EXCEPTION_ANDROID_ACTION(Logger, null);


    private static readonly Action<MEL.ILogger, RuntimePlatform, Exception?> LOG_ANDROID_EXCEPTION_NON_ANDROID_ACTION =
        LoggerMessage.Define<RuntimePlatform>(Warning,
            new EventId(id: 0, nameof(log_AndroidExceptionNotOnAndroid)),
            "Can't call the Android uncaught exception handler on non-Android platform {Platform}"
        );
    private void log_AndroidExceptionNotOnAndroid() =>
        LOG_ANDROID_EXCEPTION_NON_ANDROID_ACTION(Logger, Application.platform, null);

    #endregion
}

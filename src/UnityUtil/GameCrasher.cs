using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityUtil.DependencyInjection;

namespace UnityUtil;

public class GameCrasher : MonoBehaviour
{
    private RootLogger<GameCrasher>? _logger;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory, context: this);

    public void UncaughtExceptionClr()
    {
        _logger!.GameCrasherUncaughtExceptionClr();
        throw new InvalidOperationException("AAHHH, MANAGED EXCEPTION!!!! JK, everything is fine.");
    }

    public void ForceCrashAbort()
    {
        _logger!.GameCrasherForceCrashAbort();
        Utils.ForceCrash(ForcedCrashCategory.Abort);
    }

    public void ForceCrashAccessViolation()
    {
        _logger!.GameCrasherForceCrashAccessViolation();
        Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
    }

    public void ForceCrashFatalError()
    {
        _logger!.GameCrasherForceCrashFatalError();
        Utils.ForceCrash(ForcedCrashCategory.FatalError);
    }

    public void ForceCrashPureVirtualFunction()
    {
        _logger!.GameCrasherForceCrashPureVirtualFunction();
        Utils.ForceCrash(ForcedCrashCategory.PureVirtualFunction);
    }

    public void NativeAssert()
    {
        _logger!.GameCrasherNativeAssert();
        Utils.NativeAssert("AAHHH NATIVE ASSERT!!! JK, everything is fine.");
    }

    public void NativeError()
    {
        _logger!.GameCrasherNativeError();
        Utils.NativeError("AAHHH NATIVE ERROR!!! JK, everything is fine.");
    }

    public void UncaughtExceptionAndroid()
    {
        if (Application.platform != RuntimePlatform.Android) {
            _logger!.GameCrasherAndroidExceptionOnNonAndroidPlatform();
            return;
        }

        _logger!.GameCrasherUncaughtExceptionAndroid();

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

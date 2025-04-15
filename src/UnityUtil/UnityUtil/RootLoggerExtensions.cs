using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityUtil.DependencyInjection;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil;

/// <inheritdoc/>
internal static class RootLoggerExtensions
{
    #region Information

    private static readonly Dictionary<string, object?> SCOPE_PROPS_REGISTERED_SERVICE = []; // Cheaper than allocating new dictionary each time
    public static void RegisteredService(this MEL.ILogger logger, Service service, Scene? scene)
    {
        SCOPE_PROPS_REGISTERED_SERVICE["scene"] = scene?.name;
        using (logger.BeginScope(SCOPE_PROPS_REGISTERED_SERVICE))
            logger.LogInformation(new EventId(id: 0, nameof(RegisteredService)), "Successfully registered service of type '{serviceType}' and tag '{tag}'", service.ServiceType.Name, service.InjectTag);
    }

    public static void ToggledRecordingDependencyResolution(this MEL.ILogger logger, bool isRecording) =>
        logger.LogInformation(new EventId(id: 0, nameof(ToggledRecordingDependencyResolution)), $"Recording dependency resolutions: {{{nameof(isRecording)}}}", isRecording);

    public static void UnregisteringSceneServices(this MEL.ILogger logger, Scene scene) =>
        logger.LogInformation(new EventId(id: 0, nameof(UnregisteringSceneServices)), $"Unregistering services from scene '{{{nameof(scene)}}}'...", scene.name);

    public static void UnregisteredAllSceneServices(this MEL.ILogger logger, Scene scene, int count) =>
        logger.LogInformation(new EventId(id: 0, nameof(UnregisteredAllSceneServices)), $"Successfully unregistered all {{{nameof(count)}}} services from scene '{{{nameof(scene)}}}'", count, scene.name);

    public static void ResolvedMethodServiceParameter(this MEL.ILogger logger, string clientName, string tag, ParameterInfo parameter) =>
        logger.LogInformation(new EventId(id: 0, nameof(ResolvedMethodServiceParameter)), $"{{{nameof(clientName)}}} had dependency of Type {{serviceType}} and tag '{{{nameof(tag)}}}' injected into {{{nameof(parameter)}}}", clientName, parameter.ParameterType.FullName, tag, parameter.Name);

    public static void Spawning(this MEL.ILogger logger, string spawnedObjectName) =>
        logger.LogInformation(new EventId(id: 0, nameof(Spawning)), $"Spawning {{{nameof(spawnedObjectName)}}}", spawnedObjectName);

    public static void GameCrasherUncaughtExceptionClr(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherUncaughtExceptionClr)), "Attempting crash via uncaught CLR exception...");

    public static void GameCrasherForceCrashAbort(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherForceCrashAbort)), $"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.Abort)}...");

    public static void GameCrasherForceCrashAccessViolation(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherForceCrashAccessViolation)), $"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.AccessViolation)}...");

    public static void GameCrasherForceCrashFatalError(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherForceCrashFatalError)), $"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.FatalError)}...");

    public static void GameCrasherForceCrashPureVirtualFunction(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherForceCrashPureVirtualFunction)), $"Attempting crash via {nameof(ForcedCrashCategory)}.{nameof(ForcedCrashCategory.PureVirtualFunction)}...");

    public static void GameCrasherNativeAssert(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherNativeAssert)), "Attempting crash via native assert...");

    public static void GameCrasherNativeError(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherNativeError)), "Attempting crash via native error...");

    public static void GameCrasherUncaughtExceptionAndroid(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(GameCrasherUncaughtExceptionAndroid)), "Attempting crash via uncaught Android exception...");

    #endregion

    #region Warning

    public static void UnregisterMissingSceneService(this MEL.ILogger logger, Scene scene) =>
        logger.LogWarning(new EventId(id: 0, nameof(UnregisterMissingSceneService)), $"Cannot unregister services from scene '{{{nameof(scene)}}}' as none have been registered. Are you trying to destroy multiple service collections from the same scene?", scene.name);

    public static void MethodHasMultipleDependenciesOfType(this MEL.ILogger logger, string clientName, Type serviceType, string tag) =>
        logger.LogWarning(new EventId(id: 0, nameof(MethodHasMultipleDependenciesOfType)), $"{{{nameof(clientName)}}} has multiple dependencies of Type {{{nameof(serviceType)}}} with tag {{{nameof(tag)}}}", clientName, serviceType.FullName, tag);

    public static void GameCrasherAndroidExceptionOnNonAndroidPlatform(this MEL.ILogger logger) =>
        logger.LogWarning(new EventId(id: 0, nameof(GameCrasherAndroidExceptionOnNonAndroidPlatform)), "Can't call the Android uncaught exception handler on non-Android {platform}",
        Application.platform
    );

    #endregion

}

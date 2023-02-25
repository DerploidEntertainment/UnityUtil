using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;

namespace UnityUtil;

/// <inheritdoc/>
internal class RootLogger<T> : BaseUnityUtilLogger<T>
{
    public RootLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 0) { }

    #region Information

    public void RegisteredService(Service service, Scene? scene)
    {
        string msg = $"Successfully registered service of type {{serviceType}} and tag {{tag}}{(scene.HasValue ? $" from {{{nameof(scene)}}}" : "")}";
        LogInformation(id: 0, nameof(RegisteredService), msg, service.ServiceType.Name, service.Tag, scene?.name);
    }

    public void ToggledRecordingDependencyResolution(bool isRecording) =>
        LogInformation(id: 1, nameof(ToggledRecordingDependencyResolution), $"Recording dependency resolutions: {{{nameof(isRecording)}}}", isRecording);

    public void UnregisteringSceneServices(Scene scene) =>
        LogInformation(id: 2, nameof(UnregisteringSceneServices), $"Unregistering services from {{{nameof(scene)}}}...", scene.name);

    public void UnregisteredAllSceneServices(Scene scene, int count) =>
        LogInformation(id: 3, nameof(UnregisteredAllSceneServices), $"Successfully unregistered all {{{nameof(count)}}} services from {{{nameof(scene)}}}", count, scene.name);

    public void ResolvedMethodServiceParameter(string clientName, string tag, ParameterInfo parameter) =>
        LogInformation(id: 4, nameof(ResolvedMethodServiceParameter), $"{{{nameof(clientName)}}} had dependency of Type {{serviceType}}{(string.IsNullOrEmpty(tag) ? "" : $" with tag {{{nameof(tag)}}}")} injected into {{{nameof(parameter)}}}", clientName, parameter.ParameterType.FullName, tag, parameter.Name);

    public void Spawning(string spawnedObjectName) =>
        LogInformation(id: 5, nameof(Spawning), $"Spawning {{{nameof(spawnedObjectName)}}}", spawnedObjectName);

    #endregion

    #region Warning

    public void UnregisterMissingSceneService(Scene scene) =>
        LogWarning(id: 0, nameof(UnregisterMissingSceneService), $"Cannot unregister services from {{{nameof(scene)}}}, as none have been registered. Are you trying to destroy multiple service collections from the same scene?", scene.name);

    public void MethodHasMultipleDependenciesOfType(string clientName, Type serviceType, string tag) =>
        LogWarning(id: 1, nameof(MethodHasMultipleDependenciesOfType), $"{{{nameof(clientName)}}} has multiple dependencies of Type {{{nameof(serviceType)}}} with tag {{{nameof(tag)}}}", clientName, serviceType.FullName, tag);

    #endregion

    #region Error

    public void AsyncCallerDisposeFail(Exception exception) =>
        LogError(id: 0, nameof(AsyncCallerDisposeFail), exception, "Threw an exception during Dispose");

    #endregion
}

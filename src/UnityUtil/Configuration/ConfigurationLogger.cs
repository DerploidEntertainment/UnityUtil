using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using UnityEngine;
using UnityUtil.Logging;

namespace UnityUtil.Configuration;

/// <inheritdoc/>
internal class ConfigurationLogger<T> : BaseUnityUtilLogger<T>
{
    public ConfigurationLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 8000) { }

    #region Information

    public void ConfigSourceUnsupportedSynchronicity(ConfigurationSource configurationSource, bool isLoadingAsync) =>
        LogInformation(id: 0, nameof(ConfigSourceUnsupportedSynchronicity), $"{{{nameof(configurationSource)}}} will not be loaded because it does not support {(isLoadingAsync ? "a" : "")}synchronous loading", configurationSource.name);

    public void ConfigSourceUnsupportedLoadContext(ConfigurationSource configurationSource, ConfigurationLoadContext loadContext) =>
        LogInformation(id: 1, nameof(ConfigSourceUnsupportedLoadContext), $"{{{nameof(configurationSource)}}} will not be loaded because it was not set to load in {{{nameof(loadContext)}}}", configurationSource.name, loadContext);

    public void ConfiguredMember<TMember>(TMember member, Type clientType, string? clientName, string configKey, object? value) where TMember : MemberInfo =>
        LogInformation(id: 2, nameof(ConfiguredMember),
            $"{{{nameof(member)}}} of {{{nameof(clientType)}}} client {{{nameof(clientName)}}} with {{{nameof(configKey)}}} will be configured with {{{nameof(value)}}}",
            member.Name, clientType.Name, clientName, configKey, value
        );

    public void ConfigurationRecordingToggled(bool isRecording) =>
        LogInformation(id: 3, nameof(ConfigurationRecordingToggled), $"Recording configurations: {{{nameof(isRecording)}}}", isRecording);

    public void RemoteConfigLoadingAsync(string environment) =>
        LogInformation(id: 4, nameof(RemoteConfigLoadingAsync), $"Loading configs asynchronously from Remote Config {{{nameof(environment)}}}...", environment);

    public void RemoteConfigNothingLoaded(string environment) =>
        LogInformation(id: 5, nameof(RemoteConfigNothingLoaded), $"No configuration settings loaded from Remote Config {{{nameof(environment)}}}. Using default values.", environment);

    public void RemoteConfigUsingCache(string environment, int count) =>
        LogInformation(id: 6, nameof(RemoteConfigUsingCache), $"No configuration settings loaded from Remote Config {{{nameof(environment)}}}. Using {{{nameof(count)}}} cached values from a previous session.", environment, count);

    public void RemoteConfigLoadSuccess(string environment, int count) =>
        LogInformation(id: 7, nameof(RemoteConfigLoadSuccess), $"Successfully loaded {{{nameof(count)}}} configuration settings from Remote Config {{{nameof(environment)}}}", count, environment);

    public void CsvConfigLoadingSync(string file) =>
        LogInformation(id: 8, nameof(CsvConfigLoadingSync), $"Loading configs synchronously from CSV configuration {{{nameof(file)}}}...", file);

    public void CsvConfigLoadingAsync(string file) =>
        LogInformation(id: 9, nameof(CsvConfigLoadingAsync), $"Loading configs asynchronously from CSV configuration {{{nameof(file)}}}...", file);

    public void CsvConfigLoadFailMissingFile(string file) =>
        LogInformation(id: 10, nameof(CsvConfigLoadFailMissingFile), $"CSV configuration {{{nameof(file)}}} could not be found. If this was not expected, make sure that the file exists and is not locked by another application.", file);

    public void CsvConfigLoadSuccess(string file, int count) =>
        LogInformation(id: 11, nameof(CsvConfigLoadSuccess), $"Successfully loaded {{{nameof(count)}}} configs from CSV configuration {{{nameof(file)}}}", count, file);

    public void ScriptableObjectConfigLoadingSync(string file) =>
        LogInformation(id: 12, nameof(ScriptableObjectConfigLoadingSync), $"Loading configs synchronously from {nameof(ScriptableObject)} configuration {{{nameof(file)}}}...", file);

    public void ScriptableObjectConfigLoadingAsync(string file) =>
        LogInformation(id: 13, nameof(ScriptableObjectConfigLoadingAsync), $"Loading configs asynchronously from {nameof(ScriptableObject)} configuration {{{nameof(file)}}}...", file);

    public void ScriptableObjectConfigLoadFailMissingFile(string file) =>
        LogInformation(id: 14, nameof(ScriptableObjectConfigLoadFailMissingFile), $"{nameof(ScriptableObject)} configuration {{{nameof(file)}}} could not be found. If this was not expected, make sure that the file exists and is not locked by another application.", file);

    public void ScriptableObjectConfigLoadSuccess(string file, int count) =>
        LogInformation(id: 15, nameof(ScriptableObjectConfigLoadSuccess), $"Successfully loaded {{{nameof(count)}}} configs from {nameof(ScriptableObject)} configuration {{{nameof(file)}}}", count, file);

    #endregion

    #region Warning

    public void ConfiguringMemberAsTypeFailed(object value, Type type, string key, string? errorMessage) =>
        LogWarning(id: 0, nameof(ConfiguringMemberAsTypeFailed), $"Error converting {{{nameof(value)}}} to type {{{nameof(type)}}} for member {{{nameof(key)}}}; config will be skipped. {{{nameof(errorMessage)}}}", value, type.FullName, key, errorMessage);

    public void RemoteConfigLoadingFail() =>
        LogWarning(id: 1, nameof(RemoteConfigLoadingFail), "Something went wrong while loading configuration settings from Remote Config. Using default values.");

    public void CsvConfigDuplicateKey(string key, string file) =>
        LogWarning(id: 2, nameof(CsvConfigLoadingAsync), $"Duplicate config {{{nameof(key)}}} detected in CSV configuration {{{nameof(file)}}}. Keeping the last value...", key, file);

    public void ScriptableObjectConfigDuplicateKey(string key, string file) =>
        LogWarning(id: 3, nameof(ScriptableObjectConfigDuplicateKey), $"Duplicate config {{{nameof(key)}}} detected in {nameof(ScriptableObject)} configuration {{{nameof(file)}}}. Keeping the last value...", key, file);

    #endregion

    #region Error

    public void RemoteConfigLoadingFailMultipleEnvironments(int count) =>
        LogError(id: 0, nameof(RemoteConfigLoadingFailMultipleEnvironments), $"Attempt to load configs from {{{nameof(count)}}} Remote Config environments. Only one environment should ever be loaded.", count);

    #endregion

}

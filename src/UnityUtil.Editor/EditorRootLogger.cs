using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil;

/// <inheritdoc/>
internal class EditorRootLogger<T> : BaseUnityUtilLogger<T>
{
    public EditorRootLogger(ILoggerFactory loggerFactory, ObjectNameLogEnrichSettings objectNameLogEnrichSettings, T context)
        : base(loggerFactory, objectNameLogEnrichSettings, context, eventIdOffset: 16_000) { }

    #region Trace

    #endregion

    #region Debug

    #endregion

    #region Information

    public void ContextualGameObjectsRemoved(RemoveFromBuild[] targetsRemoved, RemoveFromBuild[] targetsRemovable, GameObject parent, Scene scene) =>
        Log(id: 0, nameof(ContextualGameObjectsRemoved), Information, $"Removed {{{nameof(targetsRemoved)}}} GameObjects out of {{{nameof(targetsRemovable)}}} marked for contextual removal under {{{nameof(parent)}}} in {{{nameof(scene)}}}", targetsRemoved.Length, targetsRemovable.Length, parent.name, scene.name);

    public void ContextualGameObjectsNotRemoved(Scene scene) =>
        Log(id: 1, nameof(ContextualGameObjectsNotRemoved), Information, $"No GameObjects marked for contextual removal in {{{nameof(scene)}}}", scene.name);

    public void LoadingAsciiCharacterSprites(string pathTemplate) =>
        Log(id: 2, nameof(LoadingAsciiCharacterSprites), Information, $"Loading character Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadingAsciiNumberSprites(string pathTemplate) =>
        Log(id: 3, nameof(LoadingAsciiNumberSprites), Information, $"Loading number Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadingAsciiUppercaseSprites(string pathTemplate) =>
        Log(id: 4, nameof(LoadingAsciiUppercaseSprites), Information, $"Loading uppercase letter Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadingAsciiLowercaseSprites(string pathTemplate) =>
        Log(id: 5, nameof(LoadingAsciiLowercaseSprites), Information, $"Loading lowercase letter Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadedAsciiSpritesWithWarnings(int loadedCount, int attemptedCount) =>
        Log(id: 6, nameof(LoadedAsciiSpritesWithWarnings), Information, $"Loaded {{{nameof(loadedCount)}}} / {{{nameof(attemptedCount)}}} character Sprites. See warnings above.", loadedCount, attemptedCount);

    public void LoadedAsciiSprites(int loadedCount) =>
        Log(id: 7, nameof(LoadedAsciiSprites), Information, $"Successfully loaded all {{{nameof(loadedCount)}}} character Sprites!", loadedCount);

    #endregion

    #region Warning

    public void LoadingAsciiSpriteFailed(char character, string assetFileName) =>
        Log(id: 0, nameof(LoadingAsciiSpriteFailed), Warning, $"Could not locate Sprite for {{{nameof(character)}}} from {{{nameof(assetFileName)}}}", character, assetFileName);

    #endregion

    #region Error

    #endregion

    #region Critical

    #endregion
}

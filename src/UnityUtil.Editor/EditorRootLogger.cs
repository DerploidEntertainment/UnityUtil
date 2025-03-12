using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;

namespace UnityUtil.Editor;

/// <inheritdoc/>
internal class EditorRootLogger<T>(ILoggerFactory loggerFactory, T context)
    : BaseUnityUtilLogger<T>(loggerFactory, context, eventIdOffset: 16_000)
{

    #region Information

    public void ContextualGameObjectsRemoved(RemoveFromBuild[] targetsRemoved, RemoveFromBuild[] targetsRemovable, GameObject parent, Scene scene) =>
        LogInformation(id: 0, nameof(ContextualGameObjectsRemoved), $"Removed {{{nameof(targetsRemoved)}}} GameObjects out of {{{nameof(targetsRemovable)}}} marked for contextual removal under {{{nameof(parent)}}} in {{{nameof(scene)}}}", targetsRemoved.Length, targetsRemovable.Length, parent.name, scene.name);

    public void LoadingAsciiCharacterSprites(string pathTemplate) =>
        LogInformation(id: 2, nameof(LoadingAsciiCharacterSprites), $"Loading character Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadingAsciiNumberSprites(string pathTemplate) =>
        LogInformation(id: 3, nameof(LoadingAsciiNumberSprites), $"Loading number Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadingAsciiUppercaseSprites(string pathTemplate) =>
        LogInformation(id: 4, nameof(LoadingAsciiUppercaseSprites), $"Loading uppercase letter Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadingAsciiLowercaseSprites(string pathTemplate) =>
        LogInformation(id: 5, nameof(LoadingAsciiLowercaseSprites), $"Loading lowercase letter Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public void LoadedAsciiSpritesWithWarnings(int loadedCount, int attemptedCount) =>
        LogInformation(id: 6, nameof(LoadedAsciiSpritesWithWarnings), $"Loaded {{{nameof(loadedCount)}}} / {{{nameof(attemptedCount)}}} character Sprites. See warnings above.", loadedCount, attemptedCount);

    public void LoadedAsciiSprites(int loadedCount) =>
        LogInformation(id: 7, nameof(LoadedAsciiSprites), $"Successfully loaded all {{{nameof(loadedCount)}}} character Sprites!", loadedCount);

    public void DependencyInjectorPrintRecording(IReadOnlyDictionary<Type, int> uncachedCounts, IReadOnlyDictionary<Type, int> cachedCounts)
    {
        LogInformation(id: 8, nameof(DependencyInjectorPrintRecording), $@"
Uncached dependency resolution counts:
(If any of these counts are greater than 1, consider caching resolutions for that Type on the {nameof(DependencyInjector)} to improve performance)
{{countUncached}}

Cached dependency resolution counts:
(If any of these counts equal 1, consider NOT caching resolutions for that Type on the {nameof(DependencyInjector)}, to speed up its resolutions and save memory)
{{countCached}}
            ",
            getCountLines(uncachedCounts), getCountLines(cachedCounts)
        );

        static string getCountLines(IEnumerable<KeyValuePair<Type, int>> counts) => string.Join(
            Environment.NewLine,
            counts.OrderByDescending(x => x.Value).Select(x => $"    {x.Key.FullName}: {x.Value}")
        );
    }

    #endregion

    #region Warning

    public void LoadingAsciiSpriteFailed(char character, string assetFileName) =>
        LogWarning(id: 0, nameof(LoadingAsciiSpriteFailed), $"Could not locate Sprite for {{{nameof(character)}}} from {{{nameof(assetFileName)}}}", character, assetFileName);

    #endregion
}

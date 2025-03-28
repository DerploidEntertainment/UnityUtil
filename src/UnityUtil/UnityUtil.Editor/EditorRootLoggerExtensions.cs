using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtil.DependencyInjection;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Editor;

/// <inheritdoc/>
internal static class EditorRootLoggerExtensions
{
    #region Information

    public static void ContextualGameObjectsRemoved(this MEL.ILogger logger, RemoveFromBuild[] targetsRemoved, RemoveFromBuild[] targetsRemovable, GameObject parent, Scene scene) =>
        logger.LogInformation(new EventId(id: 0, nameof(ContextualGameObjectsRemoved)), $"Removed {{{nameof(targetsRemoved)}}} GameObjects out of {{{nameof(targetsRemovable)}}} marked for contextual removal under {{{nameof(parent)}}} in {{{nameof(scene)}}}", targetsRemoved.Length, targetsRemovable.Length, parent.name, scene.name);

    public static void LoadingAsciiCharacterSprites(this MEL.ILogger logger, string pathTemplate) =>
        logger.LogInformation(new EventId(id: 0, nameof(LoadingAsciiCharacterSprites)), $"Loading character Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public static void LoadingAsciiNumberSprites(this MEL.ILogger logger, string pathTemplate) =>
        logger.LogInformation(new EventId(id: 0, nameof(LoadingAsciiNumberSprites)), $"Loading number Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public static void LoadingAsciiUppercaseSprites(this MEL.ILogger logger, string pathTemplate) =>
        logger.LogInformation(new EventId(id: 0, nameof(LoadingAsciiUppercaseSprites)), $"Loading uppercase letter Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public static void LoadingAsciiLowercaseSprites(this MEL.ILogger logger, string pathTemplate) =>
        logger.LogInformation(new EventId(id: 0, nameof(LoadingAsciiLowercaseSprites)), $"Loading lowercase letter Sprites using {{{nameof(pathTemplate)}}}...", pathTemplate);

    public static void LoadedAsciiSpritesWithWarnings(this MEL.ILogger logger, int loadedCount, int attemptedCount) =>
        logger.LogInformation(new EventId(id: 0, nameof(LoadedAsciiSpritesWithWarnings)), $"Loaded {{{nameof(loadedCount)}}} / {{{nameof(attemptedCount)}}} character Sprites. See warnings above.", loadedCount, attemptedCount);

    public static void LoadedAsciiSprites(this MEL.ILogger logger, int loadedCount) =>
        logger.LogInformation(new EventId(id: 0, nameof(LoadedAsciiSprites)), $"Successfully loaded all {{{nameof(loadedCount)}}} character Sprites!", loadedCount);

    public static void DependencyInjectorPrintRecording(this MEL.ILogger logger, IReadOnlyDictionary<Type, int> uncachedCounts, IReadOnlyDictionary<Type, int> cachedCounts)
    {
        logger.LogInformation(new EventId(id: 0, nameof(DependencyInjectorPrintRecording)), $@"
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

    public static void LoadingAsciiSpriteFailed(this MEL.ILogger logger, char character, string assetFileName) =>
        logger.LogWarning(new EventId(id: 0, nameof(LoadingAsciiSpriteFailed)), $"Could not locate Sprite for {{{nameof(character)}}} from {{{nameof(assetFileName)}}}", character, assetFileName);

    #endregion
}

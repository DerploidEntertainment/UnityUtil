using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Microsoft.Extensions.Logging.LogLevel;
using U = UnityEngine;

namespace UnityUtil.Logging;

/// <summary>
/// Base class for custom loggers in <see cref="UnityUtil"/> components.
/// Derived types can define methods that provide statically-typed parameters to specific log messages
/// and abstract away their log levels and other log properties.
/// </summary>
/// <typeparam name="TCategoryName">The type that is logging. It's type name will be appended as the "category" of all its log messages.</typeparam>
/// <remarks>
/// <para>
/// Defining all log messages in a single class means that developers only have to look within a single type
/// to find log <see cref="EventId"/> conflicts, inconsistent log property names, inconsistent language/grammar, etc.
/// It also provides a single type to modify when migrating log frameworks
/// (though this is less of an issue with the <see cref="Microsoft.Extensions.Logging.Abstractions"/> namespace).
/// </para>
/// <para>
/// Each method of a derived class should be referenced by at most one call site;
/// if a method is no longer referenced then it should be deleted, effectively deleting its inner log message.
/// </para>
/// <para>
/// Every log message must have a unique integer ID, so that specific messages can be searched in log files and storage systems.
/// This means that all log events are sharing the same "space" of event IDs, both within components of the <see cref="UnityUtil"/> namespaces,
/// and the components of your own application.
/// To prevent conflicts, we divide this space into six "subspaces" or "ranges" for each <see cref="LogLevel"/>, with 100,000,000 unique IDs each.
/// </para>
/// These ID ranges are:
/// <list type="table">
///     <listheader>
///         <term>Log level</term>
///         <term>ID Range Start</term>
///         <term>ID Range End</term>
///     </listheader>
///     <item>
///         <term><see cref="Trace"/></term>
///         <term>0</term>
///         <term>99,999,999</term>
///     </item>
///     <item>
///         <term><see cref="Debug"/></term>
///         <term>100,000,000</term>
///         <term>199,999,999</term>
///     </item>
///     <item>
///         <term><see cref="Information"/></term>
///         <term>200,000,000</term>
///         <term>299,999,999</term>
///     </item>
///     <item>
///         <term><see cref="Warning"/></term>
///         <term>300,000,000</term>
///         <term>399,999,999</term>
///     </item>
///     <item>
///         <term><see cref="Error"/></term>
///         <term>400,000,000</term>
///         <term>499,999,999</term>
///     </item>
///     <item>
///         <term><see cref="Critical"/></term>
///         <term>500,000,000</term>
///         <term>599,999,999</term>
///     </item>
/// </list>
/// <para>
/// Each namespace of <see cref="UnityUtil"/>, then, is given 1,000 unique event IDs to work with in each <see cref="LogLevel"/>-subspace (by default),
/// starting from some "base offset".
/// For example, the event ID offset of the <see cref="UnityUtil"/> root namespace is 0,
/// so it can use information event IDs 200,000,000 - 200,000,999 and warning event IDs 300,000,000 - 300,000,999.
/// Likewise, if the event ID offset of another namespace is 1000,
/// then it can use information event IDs 200,001,000 - 200,001,999 and warning event IDs 300,001,000 - 300,001,999.
/// In other words, a namespace's event ID offset sets the lowest offset relative to the start of each <see cref="LogLevel"/>-range that a namespace can use.
/// </para>
/// <para>
/// The authoritative list of namespace event ID offsets is maintained in the <a href="https://github.com/DerploidEntertainment/UnityUtil">UnityUtil repo</a>.
/// You <i>must</i> avoid using event IDs that collide with these ranges when creating new log messages in libraries that reference <see cref="UnityUtil"/>,
/// so that you don't pass these conflicts on to consumers.
/// If you are only referencing <see cref="UnityUtil"/> in an application, then you <i>should</i> still avoid ID collisions;
/// ID collisions will not break your application's logging, but they will make it more difficult to search for specific logs.
/// </para>
/// <b>Note to Inheritors</b>
/// <para>
/// Inheriting from this class will make all of the above ID-management easier.
/// The <see cref="BaseUnityUtilLogger{TCategoryName}.BaseUnityUtilLogger(ILoggerFactory, int)"/> constructor accepts an offset parameter,
/// and your custom log methods can use it by calling the <see langword="protected"/> <see cref="BaseUnityUtilLogger{TCategoryName}.Log(int, string, LogLevel, string?, object?[])"/> meethod.
/// That way, your custom log methods can all use simple monotonically increasing event IDs (0, 1, 2, etc.),
/// and the actual integer value is computed (<see cref="LogLevel"/>- and namespace-offsets added) in one place.
/// </para>
/// <para>
/// By convention, the custom logger for each <see cref="UnityUtil"/> namespace is a class called <c><![CDATA[<Namespace>Logger]]></c> within that namespace.
/// See the namespaces in this repo for examples.
/// </para>
/// <para>
/// When using dependency injection, consumers should not inject this class or any of its derived types directly.
/// Instead, consumers should inject <see cref="ILoggerFactory"/> and new up these types <i>in-situ</i> explicitly.
/// Otherwise, the recommendation would be to create an interface for the custom logger <i>and</i> an implementing type,
/// and that would just create a lot of friction when adding a new log message.
/// </para>
/// </remarks>
public abstract class BaseUnityUtilLogger<TCategoryName>
{
    private readonly ILogger<TCategoryName> _logger;
    private readonly ObjectNameLogEnrichSettings _objectNameLogEnrichSettings;
    private readonly TCategoryName _context;
    private readonly int _eventIdOffset;

    /// <summary>
    /// Creates a new instance of <see cref="BaseUnityUtilLogger{TCategoryName}"/>
    /// </summary>
    /// <param name="loggerFactory">Used to create the internal <see cref="ILogger{TCategoryName}"/> instance</param>
    /// <param name="eventIdOffset">This offset will be added to all logged event IDs, to ensure unique IDs across all systems.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="eventIdOffset"/> is negative, not a multiple of <see cref="ComponentAllowedIdCount"/> greater than the highest allowed offset for a single <see cref="LogLevel"/>
    /// </exception>
    public BaseUnityUtilLogger(ILoggerFactory loggerFactory, ObjectNameLogEnrichSettings objectNameLogEnrichSettings, TCategoryName context, int eventIdOffset)
    {
        int maxOffset = LogLevelAllowedIdCount - ComponentAllowedIdCount;
        if (eventIdOffset < 0 || eventIdOffset >= maxOffset)
            throw new ArgumentOutOfRangeException(nameof(eventIdOffset), $"Must be >=0 and < {maxOffset}");
        if (eventIdOffset % ComponentAllowedIdCount != 0)
            throw new ArgumentOutOfRangeException(nameof(eventIdOffset), $"Must be a multiple of {ComponentAllowedIdCount}");

        _logger = loggerFactory.CreateLogger<TCategoryName>();
        _objectNameLogEnrichSettings = objectNameLogEnrichSettings;
        _context = context;
        _eventIdOffset = eventIdOffset;
    }

    /// <summary>
    /// The max number of log event IDs that each <see cref="LogLevel"/> may use.
    /// </summary>
    /// <remarks>
    /// Note to contributors: BEWARE!! Changing this value is a SERIOUS BREAKING CHANGE, altering most of the log <see cref="EventId"/>s generated by this library.
    /// </remarks>
    public const int LogLevelAllowedIdCount = 100_000_000;

    /// <summary>
    /// The max number of log event IDs that each <see cref="UnityUtil"/> component (namespace or referencing library) may use.
    /// </summary>
    /// <remarks>
    /// Note to contributors: BEWARE!! Changing this value is a SERIOUS BREAKING CHANGE, altering most of the log <see cref="EventId"/>s generated by this library.
    /// </remarks>
    public const int ComponentAllowedIdCount = 1000;

    /// <summary>
    /// Base event ID for <see cref="Trace"/> logs.
    /// </summary>
    public const int BaseEventIdTrace = (int)Trace * LogLevelAllowedIdCount;

    /// <summary>
    /// Base event ID for <see cref="Debug"/> logs.
    /// </summary>
    public const int BaseEventIdDebug = (int)LogLevel.Debug * LogLevelAllowedIdCount;

    /// <summary>
    /// Base event ID for <see cref="Information"/> logs.
    /// </summary>
    public const int BaseEventIdInformation = (int)Information * LogLevelAllowedIdCount;

    /// <summary>
    /// Base event ID for <see cref="Warning"/> logs.
    /// </summary>
    public const int BaseEventIdWarning = (int)Warning * LogLevelAllowedIdCount;

    /// <summary>
    /// Base event ID for <see cref="Error"/> logs.
    /// </summary>
    public const int BaseEventIdError = (int)Error * LogLevelAllowedIdCount;

    /// <summary>
    /// Base event ID for <see cref="Critical"/> logs.
    /// </summary>
    public const int BaseEventIdCritical = (int)Critical * LogLevelAllowedIdCount;

    /// <summary>
    /// Formats and writes a log message at the specified <paramref name="logLevel"/>,
    /// with the specified <paramref name="id"/> and <paramref name="name"/>.
    /// </summary>
    /// <param name="id">
    /// Unique ID <i>within the logging namespace</i>.
    /// The <see cref="LogLevel"/> and namespace event ID offsets will be automatically appended
    /// </param>
    /// <param name="name">
    /// Distinctive (ideally unique) human-readable name for the log event.
    /// The name of the custom log method is usually preferrable (using <see langword="nameof"/>).
    /// </param>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    protected void Log(int id, string name, LogLevel logLevel, string? message, params object?[] args) =>
        Log(id, name, logLevel, exception: null, message, args);

    /// <summary>
    /// Formats and writes a log message at the specified <paramref name="logLevel"/>,
    /// with the specified <paramref name="id"/> and <paramref name="name"/>.
    /// </summary>
    /// <param name="id">
    /// Unique ID <i>within the logging namespace</i>.
    /// The <see cref="LogLevel"/> and namespace event ID offsets will be automatically appended
    /// </param>
    /// <param name="name">
    /// Distinctive (ideally unique) human-readable name for the log event.
    /// The name of the custom log method is usually preferrable (using <see langword="nameof"/>).
    /// </param>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    [SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "This method assumes that it has been passed static expressions")]
    protected void Log(int id, string name, LogLevel logLevel, Exception? exception, string? message, params object?[] args)
    {
        EventId eventId = new((int)logLevel * LogLevelAllowedIdCount + _eventIdOffset + id, name);

        if (_context is not U.Object contextObj) {
            _logger.Log(logLevel, eventId, exception, message, args);
            return;
        }

        var scopeProps = new Dictionary<string, object> {
            // Include the Unity Object context as a scope property.
            // Using the propertys name expected by Serilog.Sinks.Unity3D, to make integration with that popular log framework easier.
            // See https://github.com/KuraiAndras/Serilog.Sinks.Unity3D/blob/master/Serilog.Sinks.Unity3D/Assets/Serilog.Sinks.Unity3D/UnityObjectEnricher.cs
            { "%_DO_NOT_USE_UNITY_ID_DO_NOT_USE%", contextObj },
            { "%_DO_NOT_USE_UNITY_TAG_DO_NOT_USE%", typeof(TCategoryName).Name },
            { "UnityObjectPath", getUnityObjectPath(contextObj) },
        };

        using (_logger.BeginScope(scopeProps))
            _logger.Log(logLevel, eventId, exception, message, args);
    }

    private string getUnityObjectPath(U.Object context) =>
        context is not Component component
            ? context.name
            : component.GetHierarchyName(
                _objectNameLogEnrichSettings!.NumParents,
                _objectNameLogEnrichSettings!.AncestorNameSeparator,
                _objectNameLogEnrichSettings!.FormatString
            );
}

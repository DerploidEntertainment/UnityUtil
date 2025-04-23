using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Unity.Extensions.Logging;
using UnityEditor;
using UnityEngine;
using static Microsoft.Extensions.Logging.LogLevel;
using E = UnityEditor;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Editor;

[CustomEditor(typeof(AsciiSprites))]
public class AsciiSpritesEditor : E.Editor
{
    private readonly ILogger<AsciiSpritesEditor>? _logger = new UnityDebugLoggerFactory().CreateLogger<AsciiSpritesEditor>();
    private SerializedProperty? _pathProp;
    private readonly Dictionary<char, SerializedProperty> _charProps = [];

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnEnable()
    {
        _pathProp = serializedObject.FindProperty(nameof(AsciiSprites.AutoLoadSpritePath));

        // Find serialized properties corresponding to these card value/suit pairs
        for (char ch = ' '; ch <= '~'; ++ch) {
            string propPath = ch switch {
                ' ' => nameof(AsciiSprites.Space),
                '!' => nameof(AsciiSprites.Exclamation),
                '"' => nameof(AsciiSprites.DoubleQuote),
                '#' => nameof(AsciiSprites.Hashtag),
                '$' => nameof(AsciiSprites.Dollar),
                '%' => nameof(AsciiSprites.Percent),
                '&' => nameof(AsciiSprites.Ampersand),
                '\'' => nameof(AsciiSprites.Apostrophe),
                '(' => nameof(AsciiSprites.ParenthesisOpen),
                ')' => nameof(AsciiSprites.ParenthesisClose),
                '*' => nameof(AsciiSprites.Asterisk),
                '+' => nameof(AsciiSprites.Plus),
                ',' => nameof(AsciiSprites.Comma),
                '-' => nameof(AsciiSprites.Hyphen),
                '.' => nameof(AsciiSprites.Period),
                '/' => nameof(AsciiSprites.Slash),

                >= '0' and <= '9' => "Num" + ch,

                ':' => nameof(AsciiSprites.Colon),
                ';' => nameof(AsciiSprites.Semicolon),
                '<' => nameof(AsciiSprites.Less),
                '=' => nameof(AsciiSprites.Equal),
                '>' => nameof(AsciiSprites.Greater),
                '?' => nameof(AsciiSprites.Question),
                '@' => nameof(AsciiSprites.AtSign),

                >= 'A' and <= 'Z' => "Upper" + ch,

                '[' => nameof(AsciiSprites.BracketOpen),
                '\\' => nameof(AsciiSprites.Backslash),
                ']' => nameof(AsciiSprites.BracketClose),
                '^' => nameof(AsciiSprites.Caret),
                '_' => nameof(AsciiSprites.Underscore),
                '`' => nameof(AsciiSprites.GraveAccent),

                >= 'a' and <= 'z' => "Lower" + ch.ToString().ToUpperInvariant(),

                '{' => nameof(AsciiSprites.CurlyBraceOpen),
                '|' => nameof(AsciiSprites.Pipe),
                '}' => nameof(AsciiSprites.CurlyBraceClose),
                '~' => nameof(AsciiSprites.Tilde),

                _ => throw new ArgumentOutOfRangeException(nameof(ch), ch, $"Can only define Sprites for the {AsciiSprites.NumPrintables} printable ASCII characters (codes 32-{32 + AsciiSprites.NumPrintables}), but character code '{ch}' was requested!")
            };
            _charProps[ch] = serializedObject.FindProperty(propPath);
        }
    }
    public override void OnInspectorGUI()
    {
        // Draw controls for auto-loading Card Sprites
        EditorGUILayout.LabelField("Auto-Load Card Sprites", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Use these options to automatically load every character's Sprite asset from the specified path under the Assets/ folder. " +
            "This path is case-insensitive, and must use forward-slashes ('/'), even on Windows machines." +
            "\n\n" + """
            Use one of the following sets of placeholders for the numeric representations of ASCII characters:
              - '{10}', '{dec}', or '{decimal}' for the decimal value (e.g., '37' will be expected for '%', or '65' for 'A').
              - '{16}', '{hex}', or '{hexadecimal}' for the hexadecimal value (e.g., '3F' will be expected for '?')."
              - '{8}', '{oct}' or '{octal}' for the octal value (e.g., '155' will be expected for 'm')."
              - '{2}', '{bin}' or '{binary}' for the binary value (e.g., '1010111' will be expected for 'W')."
            In all of these cases, the file names MUST NOT contain leading zeros (e.g., '52.png' may represent '*' in octal, but '052.png' and '000052.png' may not).
            """ + "\n\n" +
            "For example, the default of 'ascii/{dec}.png' will locate card Sprites in the Assets/ascii/ folder, and the Sprite asset for, say, " +
            "the ',' character must be named '44.png', the resource for 'B' must be named '66.png', and so on."
        , MessageType.None);

        // Draw controls for auto-loading Card Sprites
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        _pathProp!.stringValue = EditorGUILayout.TextField("Auto-Load Sprite Path", _pathProp.stringValue);

        if (GUILayout.Button("Auto-Load Character Sprites"))
            loadSpriteAssets(log_LoadingChars, ' ', '~');

        if (GUILayout.Button("Auto-Load Number Sprites"))
            loadSpriteAssets(log_LoadingNumbers, '0', '9');

        if (GUILayout.Button("Auto-Load Uppercase Sprites"))
            loadSpriteAssets(log_LoadingUppercase, 'A', 'Z');

        if (GUILayout.Button("Auto-Load Lowercase Sprites"))
            loadSpriteAssets(log_LoadingLowercase, 'a', 'z');

        _ = serializedObject.ApplyModifiedProperties();
        _ = EditorGUI.EndChangeCheck();

        // Draw remaining controls in default way
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Character Sprites", EditorStyles.boldLabel);
        _ = DrawDefaultInspector();
    }

    private void loadSpriteAssets(Action<string> logAction, char firstChar, char lastChar)
    {
        logAction(_pathProp!.stringValue);

        if (lastChar < firstChar)
            throw new InvalidOperationException($"{nameof(lastChar)} must be >= {nameof(firstChar)}");

        int numLoaded = 0;

        if (string.IsNullOrEmpty(_pathProp!.stringValue))
            throw new InvalidOperationException($"Cannot auto-load sprites if {nameof(AsciiSprites.AutoLoadSpritePath)} is null or empty");
        if (_pathProp.stringValue.Contains('\\', StringComparison.Ordinal))
            throw new InvalidOperationException($"{nameof(AsciiSprites.AutoLoadSpritePath)} must not contain backslashes ('\\'), even on Windows; use forward slash ('/') instead");

        // Attempt to load requested character Sprites
        for (char ch = firstChar; ch <= lastChar; ++ch) {
            string assetFileName = GetAssetName(ch, _pathProp.stringValue);
            Sprite? sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetFileName);
            if (sprite == null)
                log_LoadFailed(ch, assetFileName);
            else {
                _charProps[ch].objectReferenceValue = sprite;
                ++numLoaded;
            }
        }

        // Log success/warnings
        int numAttempted = lastChar - firstChar + 1;
        if (numLoaded < numAttempted)
            log_LoadedWithWarnings(numLoaded, numAttempted);
        else
            log_Loaded(numLoaded);
    }
    internal static string GetAssetName(char character, string templateFileName)
    {
        string fileName = $"Assets/{templateFileName}";

        // Replace numeric placeholders
        string decStr = Convert.ToString(character, 10);
        fileName = Regex.Replace(fileName, @"\{10\}", decStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{dec\}", decStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{decimal\}", decStr, RegexOptions.IgnoreCase);

        string hexStr = Convert.ToString(character, 16);
        fileName = Regex.Replace(fileName, @"\{16\}", hexStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{hex\}", hexStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{hexadecimal\}", hexStr, RegexOptions.IgnoreCase);

        string octStr = Convert.ToString(character, 8);
        fileName = Regex.Replace(fileName, @"\{8\}", octStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{oct\}", octStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{octal\}", octStr, RegexOptions.IgnoreCase);

        string binStr = Convert.ToString(character, 2);
        fileName = Regex.Replace(fileName, @"\{2\}", binStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{bin\}", binStr, RegexOptions.IgnoreCase);
        fileName = Regex.Replace(fileName, @"\{binary\}", binStr, RegexOptions.IgnoreCase);

        return fileName;
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, Exception?> LOG_LOADING_CHARS_ACTION =
        LoggerMessage.Define<string>(Information,
            new EventId(id: 0, nameof(log_LoadingChars)),
            "Loading character Sprites using path template '{PathTemplate}'..."
        );
    private void log_LoadingChars(string pathTemplate) => LOG_LOADING_CHARS_ACTION(_logger!, pathTemplate, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_LOADING_NUMBERS_ACTION =
        LoggerMessage.Define<string>(Information,
            new EventId(id: 0, nameof(log_LoadingNumbers)),
            "Loading number Sprites using path template '{PathTemplate}'..."
        );
    private void log_LoadingNumbers(string pathTemplate) => LOG_LOADING_NUMBERS_ACTION(_logger!, pathTemplate, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_LOADING_UPPERCASE_ACTION =
        LoggerMessage.Define<string>(Information,
            new EventId(id: 0, nameof(log_LoadingUppercase)),
            "Loading uppercase letter Sprites using {PathTemplate}..."
        );
    private void log_LoadingUppercase(string pathTemplate) => LOG_LOADING_UPPERCASE_ACTION(_logger!, pathTemplate, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_LOADING_LOWERCASE_ACTION =
        LoggerMessage.Define<string>(Information,
            new EventId(id: 0, nameof(log_LoadingLowercase)),
            "Loading lowercase letter Sprites using {PathTemplate}..."
        );
    private void log_LoadingLowercase(string pathTemplate) => LOG_LOADING_LOWERCASE_ACTION(_logger!, pathTemplate, null);


    private static readonly Action<MEL.ILogger, int, int, Exception?> LOG_LOADED_WITH_WARNINGS_ACTION =
        LoggerMessage.Define<int, int>(Information,
            new EventId(id: 0, nameof(log_LoadedWithWarnings)),
            "Loaded {LoadedCount} / {AttemptedCount} character Sprites. See warnings above."
        );
    private void log_LoadedWithWarnings(int loadedCount, int attemptedCount) => LOG_LOADED_WITH_WARNINGS_ACTION(_logger!, loadedCount, attemptedCount, null);


    private static readonly Action<MEL.ILogger, int, Exception?> LOG_LOADED_ACTION =
        LoggerMessage.Define<int>(Information,
            new EventId(id: 0, nameof(log_Loaded)),
            "Successfully loaded all {LoadedCount} character Sprites!"
        );
    private void log_Loaded(int loadedCount) => LOG_LOADED_ACTION(_logger!, loadedCount, null);



    private static readonly Action<MEL.ILogger, char, string, Exception?> LOG_LOAD_FAILED_ACTION =
        LoggerMessage.Define<char, string>(Warning,
            new EventId(id: 0, nameof(log_LoadFailed)),
            "Could not locate Sprite for '{Character}' from asset file '{AssetFileName}'"
        );
    private void log_LoadFailed(char character, string assetFileName) => LOG_LOAD_FAILED_ACTION(_logger!, character, assetFileName, null);

    #endregion
}

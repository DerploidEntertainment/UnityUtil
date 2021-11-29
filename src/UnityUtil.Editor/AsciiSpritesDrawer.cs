using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.DependencyInjection;
using UnityEngine.Logging;
using E = UnityEditor;

namespace UnityUtil.Editor {

    [CustomEditor(typeof(AsciiSprites))]
    public class AsciiSpritesDrawer : E.Editor {

        private ILogger _logger;
        private SerializedProperty _pathProp;
        private readonly IDictionary<char, SerializedProperty> _charProps = new Dictionary<char, SerializedProperty>();

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() {
            DependencyInjector.Instance.ResolveDependenciesOf(this);
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnEnable() {
            _pathProp = serializedObject.FindProperty(nameof(AsciiSprites.AutoLoadSpritePath));

            // Find serialized properties corresponding to these card value/suit pairs
            for (char ch = ' '; ch <= '~'; ++ch) {
                string propPath = ch switch
                {
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

                    char ch2 when ('0' <= ch2 && ch2 <= '9') => "Num" + ch2,

                    ':' => nameof(AsciiSprites.Colon),
                    ';' => nameof(AsciiSprites.Semicolon),
                    '<' => nameof(AsciiSprites.Less),
                    '=' => nameof(AsciiSprites.Equal),
                    '>' => nameof(AsciiSprites.Greater),
                    '?' => nameof(AsciiSprites.Question),
                    '@' => nameof(AsciiSprites.AtSign),

                    char ch2 when ('A' <= ch2 && ch2 <= 'Z') => "Upper" + ch2.ToString().ToUpperInvariant(),

                    '[' => nameof(AsciiSprites.BracketOpen),
                    '\\' => nameof(AsciiSprites.Backslash),
                    ']' => nameof(AsciiSprites.BracketClose),
                    '^' => nameof(AsciiSprites.Caret),
                    '_' => nameof(AsciiSprites.Underscore),
                    '`' => nameof(AsciiSprites.GraveAccent),

                    char ch2 when ('a' <= ch2 && ch2 <= 'z') => "Lower" + ch2.ToString().ToLowerInvariant(),

                    '{' => nameof(AsciiSprites.CurlyBraceOpen),
                    '|' => nameof(AsciiSprites.Pipe),
                    '}' => nameof(AsciiSprites.CurlyBraceClose),
                    '~' => nameof(AsciiSprites.Tilde),

                    _ => throw new ArgumentOutOfRangeException(nameof(ch), ch, $"Can only define Sprites for the {AsciiSprites.NumPrintables} printable ASCII characters (codes 32-{32 + AsciiSprites.NumPrintables}), but character code '{ch}' was requested!")
                };
                _charProps[ch] = serializedObject.FindProperty(propPath);
            }
        }
        public override void OnInspectorGUI() {
            // Draw controls for auto-loading Card Sprites
            EditorGUILayout.LabelField("Auto-Load Card Sprites", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use these options to automatically load every character's Sprite asset from the specified path under the Assets/ folder." +
                "This path is case-insensitive, and must use forward-slashes ('/'), even on Windows machines." +
                "\n" +
                "\nUse one of the following sets of placeholders for the numeric representations of ASCII characters:" +
                "\n  '{10}', '{dec}', or '{decimal}' for the decimal value (e.g., '37' will be expected for '%', or '65' for 'A')." +
                "\n  '{16}', '{hex}', or '{hexadecimal}' for the hexadecimal value (e.g., '3F' will be expected for '?')." +
                "\n  '{8}', '{oct}' or '{octal}' for the octal value (e.g., '155' will be expected for 'm')." +
                "\n  '{2}', '{bin}' or '{binary}' for the binary value (e.g., '1010111' will be expected for 'W')." +
                "\nIn all of these cases, the file names MUST NOT contain leading zeros (e.g., '52.png' may represent '*' in octal, but '052.png' and '000052.png' may not)." +
                "\n" +
                "\nFor example, the default of 'ascii/{dec}.png' will locate card Sprites in the Assets/ascii/ folder, and the Sprite asset for, say," +
                " the ',' character must be named '44.png', the resource for 'B' must be named '66.png', and so on."
            , MessageType.None);

            // Draw controls for auto-loading Card Sprites
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            _pathProp.stringValue = EditorGUILayout.TextField("Auto-Load Sprite Path", _pathProp.stringValue);

            if (GUILayout.Button("Auto-Load Character Sprites"))
                loadAllSpriteAssets();
            if (GUILayout.Button("Auto-Load Number Sprites"))
                loadNumberSpriteAssets();
            if (GUILayout.Button("Auto-Load Uppercase Sprites"))
                loadUppercaseSpriteAssets();
            if (GUILayout.Button("Auto-Load Lowercase Sprites"))
                loadLowercaseSpriteAssets();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();

            // Draw remaining controls in default way
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Character Sprites", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }

        private void loadAllSpriteAssets() {
            _logger.Log($"Loading character Sprites using path template '{_pathProp.stringValue}'...", context: this);
            loadSpriteAssets(' ', '~');
        }
        private void loadNumberSpriteAssets() {
            _logger.Log($"Loading number Sprites using path template '{_pathProp.stringValue}'...", context: this);
            loadSpriteAssets('0', '9');
        }
        private void loadUppercaseSpriteAssets() {
            _logger.Log($"Loading uppercase letter Sprites using path template '{_pathProp.stringValue}'...", context: this);
            loadSpriteAssets('A', 'Z');
        }
        private void loadLowercaseSpriteAssets() {
            _logger.Log($"Loading lowercase letter Sprites using path template '{_pathProp.stringValue}'...", context: this);
            loadSpriteAssets('a', 'z');
        }
        private void loadSpriteAssets(char firstChar, char lastChar) {
            int numLoaded = 0;

            if (string.IsNullOrEmpty(_pathProp.stringValue))
                throw new InvalidOperationException($"Cannot auto-load sprites if {nameof(AsciiSprites.AutoLoadSpritePath)} is null or empty");
            if (_pathProp.stringValue.Contains("\\"))
                throw new InvalidOperationException($"{nameof(AsciiSprites.AutoLoadSpritePath)} must not contain backslashes ('\\'), even on Windows; use forward slash ('/') instead");

            // Attempt to load requested character Sprites
            for (char ch = firstChar; ch <= lastChar; ++ch) {
                string assetFileName = GetAssetName(ch, _pathProp.stringValue);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetFileName);
                if (sprite is null)
                    _logger.LogWarning($"Could not locate Sprite for character '{ch}' (expected at '{assetFileName}').", context: this);
                else {
                    _charProps[ch].objectReferenceValue = sprite;
                    ++numLoaded;
                }
            }

            // Log success/warnings
            string msg;
            int numAttempted = lastChar - firstChar + 1;
            msg =
                numLoaded == 0 ? $"Character Sprites were not loaded. See warnings above." :
                numLoaded < numAttempted ? $"Loaded {numLoaded} / {numAttempted} character Sprites. See warnings above." :
                $"Successfully loaded all {numAttempted} character Sprites!";
            _logger.Log(msg, context: this);
        }
        internal static string GetAssetName(char character, string templateFileName) {
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

    }

}

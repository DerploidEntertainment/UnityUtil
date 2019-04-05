using UnityEditor;
using UnityEngine;

namespace UnityUtil.Editor {

    [CustomPropertyDrawer(typeof(InjectAttribute))]
    public class InjectDrawer : PropertyDrawer {

        private const int FIELD_WIDTH = 150;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Cache GUI properties that will need to be reset at the end
            int origIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Color origColor = GUI.contentColor;
            GUI.contentColor = Color.yellow;
            string tag = (attribute as InjectAttribute).Tag;

            // Draw Label
            GUI.enabled = false;
            var serviceRect = new Rect(position.x, position.y, FIELD_WIDTH, position.height);
            string serviceTxt = $"{label.text}: Dependency {(string.IsNullOrEmpty(tag) ? string.Empty : $", requires tag '{tag}'")}";
            EditorGUI.SelectableLabel(serviceRect, serviceTxt);

            // Clean up
            GUI.enabled = true;
            GUI.contentColor = origColor;
            EditorGUI.indentLevel = origIndent;
        }
    }

}

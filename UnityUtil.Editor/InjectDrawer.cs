using UnityEditor;
using UnityEngine;
using UnityUtil;

namespace HighHandHoldem {

    [CustomPropertyDrawer(typeof(InjectAttribute))]
    public class InjectDrawer : PropertyDrawer {

        private const int FIELD_WIDTH = 150;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Using BeginProperty/EndProperty on this property will allow prefab override logic
            EditorGUI.BeginProperty(position, label, property);

            // Cache the indentLevel so that it can be reset at the end
            int origIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw Label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Draw fields
            string tag = (attribute as InjectAttribute).Tag;
            string serviceStr = "Dependency" + (string.IsNullOrEmpty(tag) ? string.Empty : $", requires tag '{tag}'");
            var serviceRect = new Rect(position.x, position.y, FIELD_WIDTH, position.height);
            var serviceLbl = new GUIContent(serviceStr);
            position = EditorGUI.PrefixLabel(serviceRect, GUIUtility.GetControlID(FocusType.Passive), serviceLbl);

            // Clean up
            EditorGUI.indentLevel = origIndent;
            EditorGUI.EndProperty();
        }
    }

}

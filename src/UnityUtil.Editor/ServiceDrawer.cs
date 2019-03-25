using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Editor {

    public static class TypeExtensions {
        public static IEnumerable<Type> GetBaseTypes(this Type type) {
            Type baseType = type.BaseType;
            Type[] interfaces = type.GetInterfaces();
            if (baseType == null)
                return interfaces;

            return Enumerable.Repeat(baseType, 1)
                .Concat(interfaces)
                .Concat(interfaces.SelectMany(GetBaseTypes))
                .Concat(baseType.GetBaseTypes());
        }
    }

    [CustomPropertyDrawer(typeof(Service))]
    public class ServiceDrawer : PropertyDrawer {

        private int _selIndex = 0;
        private string[] _typeNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty instanceProp = property.FindPropertyRelative(nameof(Service.Instance));
            SerializedProperty typeNameProp = property.FindPropertyRelative(nameof(Service.TypeName));
            if (_typeNames == null)
                _typeNames = getTypeNames(instanceProp.objectReferenceValue, typeNameProp.stringValue);

            var singleLineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Show ObjectField for the service instance
            Debug.Log($"Instance 1: {instanceProp.objectReferenceValue?.name}");
            EditorGUI.BeginChangeCheck();
            U.Object instance = EditorGUI.ObjectField(singleLineRect, nameof(Service.Instance), instanceProp.objectReferenceValue, typeof(U.Object), allowSceneObjects: true);
            Debug.Log($"Instance var 1: {instance?.name}");
            instanceProp.objectReferenceValue = instance;
            Debug.Log($"Instance var 2: {instance?.name}");
            Debug.Log($"Instance 2: {instanceProp.objectReferenceValue?.name}");
            if (EditorGUI.EndChangeCheck()) {
                Debug.Log($"Instance 3: {instanceProp.objectReferenceValue?.name}");
                if (instance != null) {
                    Debug.Log($"Instance 4: {instanceProp.objectReferenceValue?.name}");
                    _typeNames = getTypeNames(instance, typeNameProp.stringValue);
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            // If an instance was provided, then show the type name popup
            if (instanceProp.objectReferenceValue != null) {
                Debug.Log($"Instance 5: {instanceProp.objectReferenceValue?.name}");
                singleLineRect.position = new Vector2(singleLineRect.x, singleLineRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                EditorGUI.BeginChangeCheck();
                _selIndex = EditorGUI.Popup(singleLineRect, nameof(Service.TypeName), _selIndex, _typeNames);
                if (_typeNames[_selIndex] != typeNameProp.stringValue) {
                    typeNameProp.stringValue = _typeNames[_selIndex];
                    Debug.Log($"Instance 6: {instanceProp.objectReferenceValue?.name}");
                }
                EditorGUI.EndChangeCheck();
            }

            Debug.Log($"Instance 7: {instanceProp.objectReferenceValue?.name}");
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty instanceProp = property.FindPropertyRelative(nameof(Service.Instance));
            int numLines = 1 + (instanceProp.objectReferenceValue == null ? 0 : 1);
            return numLines * EditorGUIUtility.singleLineHeight + (numLines - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        private string[] getTypeNames(U.Object instance, string currentTypeName) {
            if (instance == null)
                return Array.Empty<string>();

            // Get the instance's GameObject (unless it was an asset)
            GameObject gameObj = null;
            if (instance is Component component) {
                gameObj = component.gameObject;
                BetterLogger.Log($"Getting base types from Component!");
            }
            else if (instance is GameObject) {
                gameObj = instance as GameObject;
                BetterLogger.Log($"Getting base types from GameObject!");
            }

            // Get the list of base type names, organized by component on the GameObject
            string[] typeNames;
            if (gameObj == null)
                typeNames = getPopupTypeNames(instance.GetType()).ToArray();
            else {
                IEnumerable<string> gameObjTypeNames = getPopupTypeNames(typeof(GameObject));
                IEnumerable<string> compTypeNames = gameObj
                    .GetComponents<Component>()
                    .Select(c => c.GetType())
                    .SelectMany(t => getPopupTypeNames(t));
                typeNames = gameObjTypeNames.Concat(compTypeNames).ToArray();
            }

            // Get the selected index of the current type name in that list
            _selIndex = Array.IndexOf(typeNames, currentTypeName);
            if (_selIndex == -1)
                _selIndex = 0;

            return typeNames;


            IEnumerable<string> getPopupTypeNames(Type type) {
                string name = $"{type.Name}/{type.Name}";
                IEnumerable<string> baseNames = type.GetBaseTypes().Select(t => $"{type.Name}/{t.Name}");
                return new[] { name }.Concat(baseNames);
            }
        }
    }

}

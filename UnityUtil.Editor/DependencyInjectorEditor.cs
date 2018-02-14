using UnityEditor;
using UnityEngine;
using UnityUtil;

namespace HighHandHoldem {

    [CustomEditor(typeof(DependencyInjector))]
    public class DependencyInjectorEditor : Editor {

        public override void OnInspectorGUI() {
            GUILayout.Label($"Make sure you tag this {nameof(UnityUtil.DependencyInjector)}\nas '{DependencyInjector.Tag}'!");

            DrawDefaultInspector();
        }

    }

}

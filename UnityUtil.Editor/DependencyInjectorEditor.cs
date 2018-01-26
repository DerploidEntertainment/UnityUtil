using UnityEditor;
using UnityEngine;
using UnityUtil;

namespace HighHandHoldem {

    [CustomEditor(typeof(DependencyInjector))]
    public class DependencyInjectorEditor : Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var injector = (DependencyInjector)target;
            if (GUILayout.Button("Inject"))
                injector.Inject();
        }

    }

}

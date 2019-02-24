using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace UnityUtil.Editor {

    [CustomEditor(typeof(InspectorButtonTrigger))]
    public class InspectorButtonTriggerDrawer : OdinEditor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var trigger = (InspectorButtonTrigger)target;
            if (GUILayout.Button(new GUIContent(nameof(trigger.Trigger))))
                trigger.Trigger();
        }

    }

}

using System;
using UnityEngine.Logging;
using UnityEngine.UI;

namespace UnityEngine {

    public class ErrorAlert : MonoBehaviour {

        public Text Text;

        private void Awake() => this.AssertAssociation(Text, nameof(Text));

        public void ShowError(string message) => Text.text = message;
        public void ShowException(Exception ex) => Text.text = ex.Message;

    }

}

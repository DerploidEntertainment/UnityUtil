using System;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UnityEngine {

    public class ErrorAlert : MonoBehaviour {

        public Text Text;

        private void Awake() {
            Assert.IsNotNull(Text, this.GetDependencyAssertion(nameof(Text)));
        }

        public void ShowError(string message) {
            Text.text = message;
        }
        public void ShowException(Exception ex) {
            Text.text = ex.Message;
        }

    }

}

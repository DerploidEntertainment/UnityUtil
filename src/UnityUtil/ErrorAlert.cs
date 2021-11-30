using Sirenix.OdinInspector;
using System;
using UnityEngine.UI;

namespace UnityEngine {

    public class ErrorAlert : MonoBehaviour
    {
        [Required]
        public Text? Text;

        public void ShowError(string message) => Text.text = message;
        public void ShowException(Exception ex) => Text.text = ex.Message;

    }

}

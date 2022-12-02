using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUtil;

public class ErrorAlert : MonoBehaviour
{
    [Required]
    public Text? Text;

    public void ShowError(string message) => Text!.text = message;
    public void ShowException(Exception ex) => Text!.text = ex.Message;
}

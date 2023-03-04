using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace UnityUtil;

public class ErrorAlert : MonoBehaviour
{
    [Required]
    public TMP_Text? Text;

    public void ShowError(string message) => Text!.text = message;
    public void ShowException(Exception ex) => Text!.text = ex.Message;
}

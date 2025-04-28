using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UnityUtil;

public class ErrorAlert : MonoBehaviour
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public TMP_Text? Text;

    public void ShowError(string message) => Text!.text = message;
    public void ShowException(Exception ex) => Text!.text = ex.Message;
}

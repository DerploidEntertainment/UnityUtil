using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil;

public class Destroyer : MonoBehaviour
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void DoDestroy(Object obj) => Destroy(obj);

}

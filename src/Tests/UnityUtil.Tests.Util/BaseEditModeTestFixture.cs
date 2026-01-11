using NUnit.Framework;
using UnityEngine;

namespace UnityUtil.Editor.Tests;

public class BaseEditModeTestFixture
{
    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        EditModeTestHelpers.ResetScene();
        Debug.Log($"Scene reset by {nameof(BaseEditModeTestFixture)}.{nameof(OneTimeSetUp)}");
    }
}

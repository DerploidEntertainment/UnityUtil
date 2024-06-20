using NUnit.Framework;
using UnityEngine;

namespace UnityUtil.Tests
{
    public class BasePlayModeTestFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            PlayModeTestHelpers.ResetScene();
            Debug.Log($"Scene reset by {nameof(BasePlayModeTestFixture)}.{nameof(OneTimeSetUp)}");
        }
    }
}

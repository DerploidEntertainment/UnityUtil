using NUnit.Framework;
using UnityEngine;

namespace UnityUtil.Editor.Tests
{
    public class BaseEditModeTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            EditModeTestHelpers.ResetScene();
            Debug.Log($"Scene reset by {nameof(BaseEditModeTestFixture)}.{nameof(BaseEditModeTestFixture.SetUp)}");
        }

        [TearDown]
        public void TearDown() { }
    }
}

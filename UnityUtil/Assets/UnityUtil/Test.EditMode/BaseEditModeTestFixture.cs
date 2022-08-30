using NUnit.Framework;
using UnityEngine;
using UnityUtil.Editor;

namespace UnityUtil.Test.EditMode
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

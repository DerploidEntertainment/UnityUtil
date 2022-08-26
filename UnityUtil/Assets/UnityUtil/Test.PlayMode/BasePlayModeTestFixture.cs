﻿using NUnit.Framework;
using UnityEngine;

namespace UnityUtil.Test.PlayMode
{
    public class BasePlayModeTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            PlayModeTestHelpers.ResetScene();
            Debug.Log($"Scene reset by {nameof(BasePlayModeTestFixture)}.{nameof(BasePlayModeTestFixture.SetUp)}");
        }

        [TearDown]
        public void TearDown() { }
    }
}

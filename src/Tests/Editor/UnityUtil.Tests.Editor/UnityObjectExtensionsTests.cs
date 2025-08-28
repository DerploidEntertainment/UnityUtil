using System;
using NUnit.Framework;
using UnityEngine;
using UnityUtil.Editor.Tests;

namespace UnityUtil.Tests.Editor;

public class UnityObjectExtensionsTests : BaseEditModeTestFixture
{
    [Test]
    [TestCase(new[] { "A" }, 0, "/", "A")]
    [TestCase(new[] { "A" }, 1, "/", "A")]
    [TestCase(new[] { "A", "B" }, 0, "/", "B")]
    [TestCase(new[] { "A", "B" }, 1, "/", "A/B")]
    [TestCase(new[] { "A", "B" }, int.MaxValue, "/", "A/B")]
    [TestCase(new[] { "A", "B" }, 1, " > ", "A > B")]
    [TestCase(new[] { "A", "B", "C", "D" }, 1, "/", "C/D")]
    [TestCase(new[] { "A", "B", "C", "D" }, 2, "/", "B/C/D")]
    public void GetHierarchyName_ReturnsExpected(
        string[] objectNames,
        int parentCount,
        string separator,
        string expectedHierarchyName
    ) {
        // ARRANGE
        GameObject? deepestObj = null;
        foreach (string objectName in objectNames) {
            var childObj = new GameObject(objectName);
            childObj.transform.parent = deepestObj != null ? deepestObj.transform : null;
            deepestObj = childObj;
        }

        // ACT
        string objectHierarchyName = deepestObj!.GetHierarchyName(parentCount, separator);
        string transformHierarchyName = deepestObj!.transform.GetHierarchyName(parentCount, separator);

        // ASSERT
        Assert.That(objectHierarchyName, Is.EqualTo(expectedHierarchyName));
        Assert.That(transformHierarchyName, Is.EqualTo(expectedHierarchyName));
    }

    [Test]
    [TestCase(-1, "/")]
    [TestCase(-1, " > ")]
    [TestCase(-100, "/")]
    [TestCase(-100, " > ")]
    public void GetHierarchyName_Throws_NegativeParentCount(int parentCount, string separator)
    {
        // ARRANGE
        var parentObj = new GameObject("parent");
        var childObj = new GameObject("child");
        childObj.transform.parent = parentObj.transform;

        // ACT / ASSERT
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => childObj.GetHierarchyName(parentCount, separator));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => childObj.transform.GetHierarchyName(parentCount, separator));
    }

    [Test]
    [TestCase(true, true, true, false)]
    [TestCase(true, true, false, true)]
    [TestCase(true, false, true, true)]
    [TestCase(true, false, false, true)]
    [TestCase(false, true, true, true)]
    [TestCase(false, true, false, true)]
    [TestCase(false, false, true, true)]
    [TestCase(false, false, false, true)]
    public void ThrowIfInactiveOrDisabled_ThrowsCorrectly(bool isParentActive, bool isActive, bool isEnabled, bool shouldThrow)
    {
        // ARRANGE
        var parent = new GameObject("parent");
        var obj = new GameObject("child");
        obj.transform.parent = parent.transform;

        Behaviour behaviour = obj.AddComponent<Animator>();

        // ACT
        parent.SetActive(isParentActive);
        obj.SetActive(isActive);
        behaviour.enabled = isEnabled;

        InvalidOperationException? exception = null;
        try {
            behaviour.ThrowIfInactiveOrDisabled();
        }
        catch (InvalidOperationException ex) {
            exception = ex;
        }

        // ASSERT
        Assert.That(exception, shouldThrow ? Is.Not.Null : Is.Null);
    }
}

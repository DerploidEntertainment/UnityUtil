using NUnit.Framework;
using System.Globalization;
using UnityEngine;
using UA = UnityEngine.Assertions;
using UnityEngine.Logging;
using UnityEngine.TestTools;
using UnityUtil.Editor;

namespace UnityUtil.Test.EditMode.Logging {

    public class UnityObjectExtensionsTest {

        [Test]
        public void CanGetHierarchyName_DiffNumParents() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 5);
            string name;

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 0);
            Assert.That(name, Is.EqualTo("'test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1);
            Assert.That(name, Is.EqualTo("'parent4>test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 2);
            Assert.That(name, Is.EqualTo("'parent3>parent4>test'"));
        }

        [Test]
        public void CanGetHierarchyName_DiffSeparators() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 1);
            string name;

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, separator: ">");
            Assert.That(name, Is.EqualTo("'parent0>test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, separator: ".");
            Assert.That(name, Is.EqualTo("'parent0.test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, separator: " | ");
            Assert.That(name, Is.EqualTo("'parent0 | test'"));
        }

        [Test]
        public void CanGetHierarchyName_DiffFormatStrings() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 1);
            string name;

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, formatString: "");
            Assert.That(name, Is.EqualTo(""));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, formatString: "{0}");
            Assert.That(name, Is.EqualTo("parent0>test"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, formatString: "'{0}'");
            Assert.That(name, Is.EqualTo("'parent0>test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 1, formatString: "<{0}>");
            Assert.That(name, Is.EqualTo("<parent0>test>"));
        }

        [Test]
        public void CanGetHierarchyName_IncludesOnlyNumParentsThatExist() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 2);
            string name;

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 2);
            Assert.That(name, Is.EqualTo("'parent0>parent1>test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 3);
            Assert.That(name, Is.EqualTo("'parent0>parent1>test'"));

            name = UnityObjectExtensions.GetHierarchyName(obj, numParents: 10);
            Assert.That(name, Is.EqualTo("'parent0>parent1>test'"));
        }

        [Test]
        public void CanGetHierarchyName_SameForAllAttachedComponents() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 1);
            Component component;
            string name;

            component = obj.transform;
            name = UnityObjectExtensions.GetHierarchyName(component, numParents: 1);
            Assert.That(name, Is.EqualTo("'parent0>test'"));

            component = obj.AddComponent<AudioSource>();
            name = UnityObjectExtensions.GetHierarchyName(component, numParents: 1);
            Assert.That(name, Is.EqualTo("'parent0>test'"));

            component = obj.AddComponent<Animator>();
            name = UnityObjectExtensions.GetHierarchyName(component, numParents: 1);
            Assert.That(name, Is.EqualTo("'parent0>test'"));
        }

        [Test]
        public void CanGetHierarchyNameWithType_DiffComponents() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 1);
            Component component;
            string name;

            component = obj.transform;
            name = UnityObjectExtensions.GetHierarchyNameWithType(component, numParents: 1);
            Assert.That(name, Is.EqualTo("Transform 'parent0>test'"));

            component = obj.AddComponent<AudioSource>();
            name = UnityObjectExtensions.GetHierarchyNameWithType(component, numParents: 1);
            Assert.That(name, Is.EqualTo("AudioSource 'parent0>test'"));

            component = obj.AddComponent<Animator>();
            name = UnityObjectExtensions.GetHierarchyNameWithType(component, numParents: 1);
            Assert.That(name, Is.EqualTo("Animator 'parent0>test'"));
        }

        [Test]
        public void CanGetHierarchyNameWithType_DiffFormatString() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 1);
            string name;

            name = UnityObjectExtensions.GetHierarchyNameWithType(obj.transform, numParents: 1, formatString: "");
            Assert.That(name, Is.EqualTo(""));

            name = UnityObjectExtensions.GetHierarchyNameWithType(obj.transform, numParents: 1, formatString: "{0}");
            Assert.That(name, Is.EqualTo("Transform"));

            name = UnityObjectExtensions.GetHierarchyNameWithType(obj.transform, numParents: 1, formatString: "{1}");
            Assert.That(name, Is.EqualTo("parent0>test"));

            name = UnityObjectExtensions.GetHierarchyNameWithType(obj.transform, numParents: 1, formatString: "{0}{1}");
            Assert.That(name, Is.EqualTo("Transformparent0>test"));

            name = UnityObjectExtensions.GetHierarchyNameWithType(obj.transform, numParents: 1, formatString: "component '{1}' (type {0})");
            Assert.That(name, Is.EqualTo("component 'parent0>test' (type Transform)"));
        }

        [Test]
        public void CanAssertActiveAndEnabled() {
            EditModeTestHelpers.ResetScene();

            GameObject obj = getGameObject("test", numParents: 0);
            Behaviour behaviour = obj.AddComponent<Animator>();

            obj.name = EditModeTestHelpers.GetUniqueLog("none");
            obj.SetActive(true);
            behaviour.enabled = true;
            Assert.DoesNotThrow(() => behaviour.AssertActiveAndEnabled());

            obj.name = EditModeTestHelpers.GetUniqueLog("GameObject-Behaviour");
            obj.SetActive(false);
            behaviour.enabled = false;
            Assert.Throws<UA.AssertionException>(() => behaviour.AssertActiveAndEnabled());

            obj.name = EditModeTestHelpers.GetUniqueLog("Behaviour");
            obj.SetActive(true);
            behaviour.enabled = false;
            Assert.Throws<UA.AssertionException>(() => behaviour.AssertActiveAndEnabled());

            obj.name = EditModeTestHelpers.GetUniqueLog("GameObject");
            obj.SetActive(false);
            behaviour.enabled = true;
            Assert.Throws<UA.AssertionException>(() => behaviour.AssertActiveAndEnabled());
        }

        private static GameObject getGameObject(string name, int numParents = 1, string parentNameFormatString = "parent{0}") {
            Transform? lastParentTrans = null;
            for (int p = 0; p < numParents; ++p) {
                Transform t = new GameObject(string.Format(CultureInfo.InvariantCulture, parentNameFormatString, p)).transform;
                t.parent = lastParentTrans;
                lastParentTrans = t;
            }

            var obj = new GameObject(name);
            obj.transform.parent = lastParentTrans;
            return obj;
        }

    }

}

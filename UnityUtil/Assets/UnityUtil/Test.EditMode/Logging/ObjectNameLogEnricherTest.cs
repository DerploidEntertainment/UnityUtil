using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Logging;
using UnityUtil.Editor;

namespace UnityUtil.Test.EditMode.Logging {

    public class ObjectNameLogEnricherTest {

        [Test]
        [SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Unity doesn't support C#8 features")]
        public void NonUnityObjectsReturnEmpty() {
            EditModeTestHelpers.ResetScene();

            ObjectNameLogEnricher enricher = getObjectNameLogEnricher();
            string log;

            object objSource = new object();
            log = enricher.GetEnrichedLog(objSource);
            Assert.That(log, Is.Empty);

            string strSource = "a string";
            log = enricher.GetEnrichedLog(strSource);
            Assert.That(log, Is.Empty);

            int intSource = 0;
            log = enricher.GetEnrichedLog(intSource);
            Assert.That(log, Is.Empty);

            var classSource = new ObjectNameLogEnricherTest();
            log = enricher.GetEnrichedLog(classSource);
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void ScriptableObjectsReturnName() {
            EditModeTestHelpers.ResetScene();

            ObjectNameLogEnricher enricher = getObjectNameLogEnricher();
            string log;

            AudioClip audioClip = AudioClip.Create("audio", lengthSamples: 1, channels: 1, frequency: 1000, stream: false);
            log = enricher.GetEnrichedLog(audioClip);
            Assert.That(log, Is.EqualTo("audio"));

            var avatarMask = new AvatarMask { name = "avatarMask" };
            log = enricher.GetEnrichedLog(avatarMask);
            Assert.That(log, Is.EqualTo("avatarMask"));

            ObjectNameLogEnricher enricherSource = ScriptableObject.CreateInstance<ObjectNameLogEnricher>();
            enricherSource.name = "enricher-asset";
            log = enricher.GetEnrichedLog(enricherSource);
            Assert.That(log, Is.EqualTo("enricher-asset"));
        }

        [Test]
        public void GameObjectsReturnName() {
            EditModeTestHelpers.ResetScene();

            ObjectNameLogEnricher enricher = getObjectNameLogEnricher();
            GameObject obj;
            string log;

            obj = new GameObject("obj0");
            log = enricher.GetEnrichedLog(obj);
            Assert.That(log, Is.EqualTo("obj0"));

            obj = new GameObject("obj1");
            log = enricher.GetEnrichedLog(obj);
            Assert.That(log, Is.EqualTo("obj1"));
        }

        [Test]
        public void ComponentsOnSameGameObjectReturnSame() {
            EditModeTestHelpers.ResetScene();

            ObjectNameLogEnricher enricher = getObjectNameLogEnricher(numParents: 1, ancestorNameSeparator: ">");
            var parent = new GameObject("parent");
            var obj = new GameObject("obj");
            obj.transform.parent = parent.transform;
            Component component;
            string log;

            component = obj.AddComponent<AudioSource>();
            log = enricher.GetEnrichedLog(component);
            Assert.That(log, Is.EqualTo("parent>obj"));

            component = obj.AddComponent<Animator>();
            log = enricher.GetEnrichedLog(component);
            Assert.That(log, Is.EqualTo("parent>obj"));
        }

        private ObjectNameLogEnricher getObjectNameLogEnricher(uint numParents = 1u, string ancestorNameSeparator = ">", string formatString = "{0}") {
            ObjectNameLogEnricher enricher = ScriptableObject.CreateInstance<ObjectNameLogEnricher>();
            enricher.NumParents = numParents;
            enricher.AncestorNameSeparator = ancestorNameSeparator;
            enricher.FormatString = formatString;

            return enricher;
        }

    }

}

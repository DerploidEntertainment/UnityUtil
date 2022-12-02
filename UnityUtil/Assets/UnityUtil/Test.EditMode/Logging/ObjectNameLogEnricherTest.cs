using NUnit.Framework;
using UnityEngine;
using UnityUtil.Logging;

namespace UnityUtil.Test.EditMode.Logging
{

    public class ObjectNameLogEnricherTest : BaseEditModeTestFixture
    {

        [Test]
        public void NonUnityObjectsReturnEmpty()
        {
            ObjectNameLogEnricher enricher = getObjectNameLogEnricher();
            string log;

            object objSource = new();
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
        public void ScriptableObjectsReturnName()
        {
            ObjectNameLogEnricher enricher = getObjectNameLogEnricher();
            string log;

            var audioClip = AudioClip.Create("audio", lengthSamples: 1, channels: 1, frequency: 1000, stream: false);
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
        public void GameObjectsReturnName()
        {
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
        public void ComponentsOnSameGameObjectReturnSame()
        {
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

        private static ObjectNameLogEnricher getObjectNameLogEnricher(uint numParents = 1u, string ancestorNameSeparator = ">", string formatString = "{0}")
        {
            ObjectNameLogEnricher enricher = ScriptableObject.CreateInstance<ObjectNameLogEnricher>();
            enricher.NumParents = numParents;
            enricher.AncestorNameSeparator = ancestorNameSeparator;
            enricher.FormatString = formatString;

            return enricher;
        }

    }

}

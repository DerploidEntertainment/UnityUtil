using NUnit.Framework;
using UnityEngine;
using UnityEngine.Logging;
using UnityEngine.UI;

namespace UnityUtil.Test.EditMode {

    public class ScrollRectVelocityClamperTest
    {

        [Test]
        public void ClampsPositiveVelocities()
        {
            Vector2 vClamped;
            ScrollRectVelocityClamper clamper = getScrollRectVelocityClamper();
            clamper.MinVelocityMagnitude = new Vector2Int(5, 5);

            vClamped = clamper.clampedVelocity(new Vector2(4.9f, 5f));
            Assert.That(vClamped.x, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(5f, 4.9f));
            Assert.That(vClamped.y, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(4.9f, 4.9f));
            Assert.That(vClamped.x, Is.Zero);
            Assert.That(vClamped.y, Is.Zero);

            vClamped = clamper.clampedVelocity(new Vector2(2f, 5f));
            Assert.That(vClamped.x, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(5f, 2f));
            Assert.That(vClamped.y, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(2f, 2f));
            Assert.That(vClamped.x, Is.Zero);
            Assert.That(vClamped.y, Is.Zero);
        }

        [Test]
        public void ClampsNegativeVelocities()
        {
            Vector2 vClamped;
            ScrollRectVelocityClamper clamper = getScrollRectVelocityClamper();
            clamper.MinVelocityMagnitude = new Vector2Int(5, 5);

            vClamped = clamper.clampedVelocity(new Vector2(-4.9f, 5f));
            Assert.That(vClamped.x, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(5f, -4.9f));
            Assert.That(vClamped.y, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(-4.9f, -4.9f));
            Assert.That(vClamped.x, Is.Zero);
            Assert.That(vClamped.y, Is.Zero);

            vClamped = clamper.clampedVelocity(new Vector2(-2f, 5f));
            Assert.That(vClamped.x, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(5f, -2f));
            Assert.That(vClamped.y, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(-2f, -2f));
            Assert.That(vClamped.x, Is.Zero);
            Assert.That(vClamped.y, Is.Zero);
        }

        [Test]
        public void DoesNotClampPositiveVelocities()
        {
            Vector2 vClamped;
            ScrollRectVelocityClamper clamper = getScrollRectVelocityClamper();
            clamper.MinVelocityMagnitude = new Vector2Int(5, 5);

            vClamped = clamper.clampedVelocity(new Vector2(5f, 5f));
            Assert.That(vClamped.x, Is.EqualTo(5f));
            Assert.That(vClamped.y, Is.EqualTo(5f));

            vClamped = clamper.clampedVelocity(new Vector2(5.1f, 5.1f));
            Assert.That(vClamped.x, Is.EqualTo(5.1f));
            Assert.That(vClamped.y, Is.EqualTo(5.1f));

            vClamped = clamper.clampedVelocity(new Vector2(10f, 10f));
            Assert.That(vClamped.x, Is.EqualTo(10f));
            Assert.That(vClamped.y, Is.EqualTo(10f));
        }

        [Test]
        public void DoesNotClampNegativeVelocities()
        {
            Vector2 vClamped;
            ScrollRectVelocityClamper clamper = getScrollRectVelocityClamper();
            clamper.MinVelocityMagnitude = new Vector2Int(5, 5);

            vClamped = clamper.clampedVelocity(new Vector2(-5f, -5f));
            Assert.That(vClamped.x, Is.EqualTo(-5f));
            Assert.That(vClamped.y, Is.EqualTo(-5f));

            vClamped = clamper.clampedVelocity(new Vector2(-5.1f, -5.1f));
            Assert.That(vClamped.x, Is.EqualTo(-5.1f));
            Assert.That(vClamped.y, Is.EqualTo(-5.1f));

            vClamped = clamper.clampedVelocity(new Vector2(-10f, -10f));
            Assert.That(vClamped.x, Is.EqualTo(-10f));
            Assert.That(vClamped.y, Is.EqualTo(-10f));
        }

        [Test]
        public void SupportsDifferentClampValues()
        {
            Vector2 vClamped;
            ScrollRectVelocityClamper clamper = getScrollRectVelocityClamper();

            clamper.MinVelocityMagnitude = new Vector2Int(5, 5);
            vClamped = clamper.clampedVelocity(new Vector2(4f, 4f));
            Assert.That(vClamped.x, Is.Zero);
            Assert.That(vClamped.y, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(6f, 6f));
            Assert.That(vClamped.x, Is.EqualTo(6f));
            Assert.That(vClamped.y, Is.EqualTo(6f));

            clamper.MinVelocityMagnitude = new Vector2Int(10, 10);
            vClamped = clamper.clampedVelocity(new Vector2(9f, 9f));
            Assert.That(vClamped.x, Is.Zero);
            Assert.That(vClamped.y, Is.Zero);
            vClamped = clamper.clampedVelocity(new Vector2(11f, 11f));
            Assert.That(vClamped.x, Is.EqualTo(11f));
            Assert.That(vClamped.y, Is.EqualTo(11f));
        }

        [Test]
        public void SupportsDifferentXAndYClampValues()
        {
            Vector2 vClamped;
            ScrollRectVelocityClamper clamper = getScrollRectVelocityClamper();
            clamper.MinVelocityMagnitude = new Vector2Int(5, 10);

            vClamped = clamper.clampedVelocity(new Vector2(6f, 6f));
            Assert.That(vClamped.x, Is.EqualTo(6f));
            Assert.That(vClamped.y, Is.Zero);

            vClamped = clamper.clampedVelocity(new Vector2(5f, 5f));
            Assert.That(vClamped.x, Is.EqualTo(5f));
            Assert.That(vClamped.y, Is.Zero);
        }

        private ScrollRectVelocityClamper getScrollRectVelocityClamper(ILoggerProvider loggerProvider = null) {
            var clamperObj = new GameObject("test");
            Updater updater = clamperObj.AddComponent<Updater>();
            Configurator configurator = clamperObj.AddComponent<Configurator>();

            ScrollRectVelocityClamper clamper = clamperObj.AddComponent<ScrollRectVelocityClamper>();
            clamper.Inject(updater);
            clamper.Inject(configurator, loggerProvider ?? new TestLoggerProvider());

            return clamper;
        }

    }

}

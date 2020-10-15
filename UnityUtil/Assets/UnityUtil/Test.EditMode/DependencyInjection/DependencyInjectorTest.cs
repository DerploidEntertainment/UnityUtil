using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.DependencyInjection;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;
using UnityUtil.Editor;
using UnityUtil.Test.EditMode.Logging;

namespace UnityUtil.Test.EditMode.DependencyInjection
{
    #region Test service/client types

    public class TestComponent : MonoBehaviour { }

    public class TestClientBase
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This type is just for testing so I don't wanna think about this...")]
        public void Inject(TestComponent componentService) { }
    }

    public class TestClient : TestClientBase
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This type is just for testing so I don't wanna think about this...")]
        public void Inject(object objectService, Animator animatorService) { }
    }

    public interface INoDependenciesClient
    {
        void Inject();
    }

    public interface IMultipleInjectClient
    {
        void Inject(object objectService);
        void Inject(Component componentService);
    }

    internal class RepeatedDependencyClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This type is just for testing so I don't wanna think about this...")]
        public void Inject(TestComponent componentService1, TestComponent componentService2) { }
    }

    #endregion

    public class DependencyInjectorTest
    {

        #region Service registration tests

        [Test]
        public void Can_Register_SameTypeDifferentTag()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();

            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "not-test"));    // Test would fail if this threw or logged an error
        }

        [Test]
        public void Cannot_Register_SameTypeAndTag_SameScene()
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(getComponentService<TestComponent>("test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            // ACT/ASSERT
            Assert.Throws<InvalidOperationException>(() =>
                dependencyInjector.RegisterService(getComponentService<TestComponent>("test")));

            Assert.Throws<InvalidOperationException>(() =>
                dependencyInjector.RegisterService(getComponentService<TestComponent>()));
        }

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void Cannot_Register_SameTypeAndTag_OtherScene()
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(getComponentService<TestComponent>("test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            Scene otherScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(otherScene);

            // ACT/ASSERT
            Assert.Throws<InvalidOperationException>(() =>
                dependencyInjector.RegisterService(getComponentService<TestComponent>("test")));

            Assert.Throws<InvalidOperationException>(() =>
                dependencyInjector.RegisterService(getComponentService<TestComponent>()));

            EditorSceneManager.CloseScene(otherScene, removeScene: true);
        }

        [Test]
        public void Registers_Components_WithInspectorTag()
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();

            var obj = new GameObject { tag = "test" };
            TestComponent serviceInstance = obj.AddComponent<TestComponent>();
            dependencyInjector.RegisterService(serviceInstance);

            // ACT/ASSERT
            DependencyInjector.Service service = dependencyInjector.GetService(typeof(TestComponent), tag: "test", clientName: "Unit test");
            Assert.That(service.Instance, Is.SameAs(serviceInstance));

            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.GetService(typeof(TestComponent), tag: "other", clientName: "Unit test"));
        }

        [Test]
        public void Registers_NonComponents_WithDefaultTag()
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();

            object serviceInstance = new object();
            dependencyInjector.RegisterService(serviceInstance);

            // ACT/ASSERT
            DependencyInjector.Service service = dependencyInjector.GetService(typeof(object), tag: DependencyInjector.DefaultTag, clientName: "Unit test");
            Assert.That(service.Instance, Is.SameAs(serviceInstance));

            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.GetService(typeof(object), tag: "test", clientName: "Unit test"));
        }

        #endregion

        #region Service resolution tests

        [Test]
        public void Can_Resolve_ByType()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();
            object serviceInstance = new object();
            dependencyInjector.RegisterService(serviceInstance);

            DependencyInjector.Service service = dependencyInjector.GetService(typeof(object), tag: DependencyInjector.DefaultTag, clientName: "Unit test");
            Assert.That(service.Instance, Is.SameAs(serviceInstance));
        }

        [Test]
        public void Can_Resolve_DerivedType()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();
            var serviceInstance = new ApplicationException();
            dependencyInjector.RegisterService<Exception>(serviceInstance);

            DependencyInjector.Service service = dependencyInjector.GetService(typeof(Exception), tag: DependencyInjector.DefaultTag, clientName: "Unit test");
            Assert.That(service.Instance, Is.SameAs(serviceInstance));
            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.GetService(typeof(ApplicationException), tag: DependencyInjector.DefaultTag, clientName: "Unit test"));
        }

        [Test]
        public void Can_Resolve_ByTypeAndTag()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();
            var obj = new GameObject { tag = "test" };
            TestComponent serviceInstance = obj.AddComponent<TestComponent>();
            dependencyInjector.RegisterService(serviceInstance);

            DependencyInjector.Service service = dependencyInjector.GetService(typeof(TestComponent), tag: "test", clientName: "Unit test");
            Assert.That(service.Instance, Is.SameAs(serviceInstance));
        }

        [Test]
        public void Cannot_Resolve_MissingService()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();

            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.GetService(typeof(Component), tag: "any", clientName: "Unit test"));
            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.GetService(typeof(Component), tag: "Untagged", clientName: "Unit test"));
            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.GetService(typeof(MonoBehaviour), tag: "any", clientName: "Unit test"));
            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.GetService(typeof(MonoBehaviour), tag: "Untagged", clientName: "Unit test"));
        }

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void Can_Resolve_ServiceInScene_ClientInDifferentScene()
        {
            EditModeTestHelpers.ResetScene();

            Assert.Fail();
        }

        [Test]
        public void Resolve_CallsInject_UpInheritanceHierarchy()
        {
            EditModeTestHelpers.ResetScene();

            Mock<ITypeMetadataProvider> typeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(typeMetadataProvider: typeMetadataProvider.Object);

            var client = new TestClient();
            dependencyInjector.RegisterService(new object());
            dependencyInjector.RegisterService(new GameObject().AddComponent<TestComponent>());
            dependencyInjector.RegisterService(new GameObject().AddComponent<Animator>());

            dependencyInjector.ResolveDependenciesOf(client);

            typeMetadataProvider.Verify(x => x.GetMethod(typeof(TestClient), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()), Times.Once);
            typeMetadataProvider.Verify(x => x.GetMethod(typeof(TestClientBase), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()), Times.Once);
        }

        [Test]
        public void Resolve_WithoutInject_DoesNotThrow()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();
            Assert.DoesNotThrow(() => dependencyInjector.ResolveDependenciesOf(new GameObject()));
        }

        [Test]
        public void Resolve_DoesNotCallInject_WithNoDependencies()
        {
            EditModeTestHelpers.ResetScene();

            Mock<ITypeMetadataProvider> typeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(typeMetadataProvider: typeMetadataProvider.Object);

            var client = new Mock<INoDependenciesClient>();
            client.Setup(x => x.Inject()).Callback(() => { });
            dependencyInjector.ResolveDependenciesOf(client.Object);

            // Client's Inject() method takes no parameters, so it will not be called
            client.Verify(x => x.Inject(), Times.Never);
        }

        [Test]
        public void Cannot_Resolve_MultipleInjectMethods()
        {
            EditModeTestHelpers.ResetScene();

            Mock<ITypeMetadataProvider> typeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(typeMetadataProvider: typeMetadataProvider.Object);
            var client = new Mock<IMultipleInjectClient>();

            Assert.Throws<AmbiguousMatchException>(() => dependencyInjector.ResolveDependenciesOf(client.Object));
        }

        [Test]
        public void Can_CacheResolve_RootType()
        {
            EditModeTestHelpers.ResetScene();

            Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(
                cachedResolutionTypes: new[] { typeof(TestClientBase) },
                typeMetadataProvider: mockTypeMetadataProvider.Object
            );
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            // Initial, uncached resolution
            var uncacheClient = new TestClientBase();
            dependencyInjector.ResolveDependenciesOf(uncacheClient);
            mockTypeMetadataProvider.Verify(x =>
                x.GetMethod(typeof(TestClientBase), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()),
            Times.Once);

            // Cached resolution
            var cacheClient = new TestClientBase();
            dependencyInjector.ResolveDependenciesOf(cacheClient);
            mockTypeMetadataProvider.Verify(x =>
                x.GetMethod(typeof(TestClientBase), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()),
            Times.Once);
        }

        [Test]
        public void Can_CacheResolve_BaseType()
        {
            EditModeTestHelpers.ResetScene();

            Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(
                cachedResolutionTypes: new[] { typeof(TestClientBase) },
                typeMetadataProvider: mockTypeMetadataProvider.Object
            );
            dependencyInjector.RegisterService(getComponentService<TestComponent>());
            dependencyInjector.RegisterService(new object());
            dependencyInjector.RegisterService(getComponentService<Animator>());

            // Initial, uncached resolution
            var uncacheClient = new TestClient();
            dependencyInjector.ResolveDependenciesOf(uncacheClient);
            mockTypeMetadataProvider.Verify(x => 
                x.GetMethod(typeof(TestClient), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()),
            Times.Once);
            mockTypeMetadataProvider.Verify(x =>
                x.GetMethod(typeof(TestClientBase), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()),
            Times.Once);

            // Second, base type resolution cached
            var cacheClient = new TestClient();
            dependencyInjector.ResolveDependenciesOf(cacheClient);
            mockTypeMetadataProvider.Verify(x =>
                x.GetMethod(typeof(TestClient), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()),
            Times.Exactly(2));
            mockTypeMetadataProvider.Verify(x =>
                x.GetMethod(typeof(TestClientBase), DependencyInjector.InjectMethodName, It.IsAny<BindingFlags>()),
            Times.Once);
        }

        [Test]
        public void Resolve_Warns_RepeatedDependencies()
        {
            EditModeTestHelpers.ResetScene();

            var loggerProvider = new Mock<ILoggerProvider>();
            var testLogger = new TestLogger();
            loggerProvider.Setup(x => x.GetLogger(It.IsAny<object>())).Returns(testLogger);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerProvider: loggerProvider.Object);
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            dependencyInjector.ResolveDependenciesOf(new RepeatedDependencyClient());

            Assert.That(testLogger.NumWarnings, Is.EqualTo(1));
        }

        #endregion

        #region Resolution recording tests

        [Test]
        public void Can_Record_Resolutions()
        {
            EditModeTestHelpers.ResetScene();

            Type clientType = typeof(TestClient);
            Type baseType = typeof(TestClientBase);
            Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
            DependencyResolutionCounts counts = null;
            DependencyInjector dependencyInjector = getDependencyInjector(
                cachedResolutionTypes: new[] { typeof(TestClientBase) },
                typeMetadataProvider: mockTypeMetadataProvider.Object
            );
            dependencyInjector.RegisterService(getComponentService<TestComponent>());
            dependencyInjector.RegisterService(new object());
            dependencyInjector.RegisterService(getComponentService<Animator>());
            dependencyInjector.RecordingResolutions = true;

            // Initial, uncached resolution
            var uncacheClient = new TestClient();
            dependencyInjector.ResolveDependenciesOf(uncacheClient);
            dependencyInjector.GetServiceResolutionCounts(ref counts);
            Assert.That(counts.Uncached.Count, Is.EqualTo(1));
            Assert.That(counts.Uncached[clientType], Is.EqualTo(1));
            Assert.That(counts.Cached.Count, Is.EqualTo(1));
            Assert.That(counts.Cached[baseType], Is.EqualTo(1));

            // Second, base type resolution cached
            var cacheClient = new TestClient();
            dependencyInjector.ResolveDependenciesOf(cacheClient);
            dependencyInjector.GetServiceResolutionCounts(ref counts);
            Assert.That(counts.Uncached.Count, Is.EqualTo(1));
            Assert.That(counts.Uncached[clientType], Is.EqualTo(2));
            Assert.That(counts.Cached.Count, Is.EqualTo(1));
            Assert.That(counts.Cached[baseType], Is.EqualTo(2));

            // Clears cached resolutions
            dependencyInjector.RecordingResolutions = false;
            Assert.That(counts.Uncached.Count, Is.Zero);
            Assert.That(counts.Cached.Count, Is.Zero);
        }

        [Test]
        public void Record_Stop_ClearsRecordedResolutions()
        {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector(cachedResolutionTypes: new[] { typeof(TestClientBase) });
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            // ACT
            var client = new TestClientBase();
            dependencyInjector.RecordingResolutions = true;
            dependencyInjector.ResolveDependenciesOf(client);
            dependencyInjector.RecordingResolutions = false;

            // ASSERT
            DependencyResolutionCounts counts = null;
            dependencyInjector.GetServiceResolutionCounts(ref counts);
            Assert.That(counts.Uncached.Count, Is.Zero);
            Assert.That(counts.Cached.Count, Is.Zero);
        }

        [Test]
        public void Resolutions_AreNotStored_WhileNotRecording()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            dependencyInjector.RecordingResolutions = false;
            DependencyResolutionCounts counts = null;
            dependencyInjector.ResolveDependenciesOf(new TestClientBase());
            dependencyInjector.ResolveDependenciesOf(new TestClientBase());
            dependencyInjector.ResolveDependenciesOf(new TestClientBase());
            dependencyInjector.GetServiceResolutionCounts(ref counts);

            Assert.That(counts.Cached, Is.Empty);
            Assert.That(counts.Uncached, Is.Empty);
        }

        #endregion

        #region Service unregistration tests

        [Test]
        public void Can_Unregister_AllServicesFromScene()
        {
            EditModeTestHelpers.ResetScene();

            DependencyInjector dependencyInjector = getDependencyInjector();
            Scene activeScene = SceneManager.GetActiveScene();
            dependencyInjector.RegisterService(new object(), SceneManager.GetActiveScene());
            dependencyInjector.RegisterService(new Exception(), SceneManager.GetActiveScene());

            dependencyInjector.UnregisterSceneServices(activeScene);

            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.GetService(typeof(object), DependencyInjector.DefaultTag, clientName: "Unit test"));
            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.GetService(typeof(Exception), DependencyInjector.DefaultTag, clientName: "Unit test"));
        }

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void Can_Unregister_AllServicesFromScene_LeavesOtherSceneServices()
        {
            EditModeTestHelpers.ResetScene();

            Assert.Fail();
        }

        #endregion

        private DependencyInjector getDependencyInjector(Type[] cachedResolutionTypes = null, ILoggerProvider loggerProvider = null, ITypeMetadataProvider typeMetadataProvider = null)
        {
            var dependencyInjector = new DependencyInjector(cachedResolutionTypes ?? Array.Empty<Type>());
            dependencyInjector.Initialize(loggerProvider ?? new TestLoggerProvider(), typeMetadataProvider ?? new TypeMetadataProvider());

            return dependencyInjector;
        }
        private T getComponentService<T>(string tag = "Untagged") where T : Component => new GameObject { tag = tag }.AddComponent<T>();
        private Mock<ITypeMetadataProvider> getTypeMetadataProvider()
        {
            var mock = new Mock<ITypeMetadataProvider>();
            var typeMetadataProvider = new TypeMetadataProvider();
            mock.Setup(x => x.GetMethod(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<BindingFlags>()))
                .Returns<Type, string, BindingFlags>((type, name, bindingFlags) => typeMetadataProvider.GetMethod(type, name, bindingFlags));

            mock.Setup(x => x.GetMethodParameters(It.IsAny<MethodInfo>()))
                .Returns<MethodInfo>(method => typeMetadataProvider.GetMethodParameters(method));

            mock.Setup(x => x.GetCustomAttribute<InjectTagAttribute>(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(param => typeMetadataProvider.GetCustomAttribute<InjectTagAttribute>(param));

            mock.Setup(x => x.CompileMethodCall(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MethodInfo>(), It.IsAny<object[]>()))
                .Returns<string, string, MethodInfo, object[]>((methodName, paramName, injectMethod, args) =>
                    typeMetadataProvider.CompileMethodCall(methodName, paramName, injectMethod, args));

            return mock;
        }
    }
}

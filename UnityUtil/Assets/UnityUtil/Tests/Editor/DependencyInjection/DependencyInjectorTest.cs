using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;

namespace UnityUtil.Editor.Tests.DependencyInjection
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

    internal class SameTypeNoTagsClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This type is just for testing so I don't wanna think about this...")]
        public void Inject(TestComponent componentService1, TestComponent componentService2) { }
    }

    internal class SameTypeDifferentTagsClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This type is just for testing so I don't wanna think about this...")]
        public void Inject([InjectTag("test")] TestComponent componentService1, [InjectTag("not-test")] TestComponent componentService2) { }
    }

    internal class SameTypeSameTagsClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This type is just for testing so I don't wanna think about this...")]
        public void Inject([InjectTag("test")] TestComponent componentService1, [InjectTag("test")] TestComponent componentService2) { }
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
    internal class NoConstructorClient
    {
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
    internal class EmptyConstructorClient
    {
        public EmptyConstructorClient() { }
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
    internal class MultiConstructorClient
    {
        public int NumParamsInUsedConstructor { get; private set; }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        public MultiConstructorClient(TestComponent componentService, object objectService, Animator animatorService) : this(componentService, objectService) => NumParamsInUsedConstructor = 3;

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        public MultiConstructorClient(TestComponent componentService, object objectService) : this(componentService) => NumParamsInUsedConstructor = 2;

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        public MultiConstructorClient(TestComponent componentService) => NumParamsInUsedConstructor = 1;
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
    internal class ConstructorSameTypeNoTagsClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        public ConstructorSameTypeNoTagsClient(TestComponent componentService1, TestComponent componentService2) { }
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
    internal class ConstructorSameTypeDifferentTagsClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        public ConstructorSameTypeDifferentTagsClient([InjectTag("test")] TestComponent componentService1, [InjectTag("not-test")] TestComponent componentService2) { }
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
    internal class ConstructorSameTypeSameTagsClient
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
        public ConstructorSameTypeSameTagsClient([InjectTag("test")] TestComponent componentService1, [InjectTag("test")] TestComponent componentService2) { }
    }

    #endregion

    public class DependencyInjectorTest : BaseEditModeTestFixture
    {

        #region Initialization tests

        [Test]
        public void Cannot_Initialize_MultipleTimes()
        {
            var dependencyInjector = new DependencyInjector(Array.Empty<Type>());
            dependencyInjector.Initialize(new UnityDebugLoggerFactory());

            Assert.Throws<InvalidOperationException>(() => dependencyInjector.Initialize(new UnityDebugLoggerFactory()));
        }

        [Test]
        public void Cannot_Register_UntilInitialized()
        {
            var dependencyInjector = new DependencyInjector(Array.Empty<Type>());
            object testService = new();

            Assert.Throws<InvalidOperationException>(() => dependencyInjector.RegisterService(testService));

            dependencyInjector.Initialize(new UnityDebugLoggerFactory());
            Assert.DoesNotThrow(() => dependencyInjector.RegisterService(testService));
        }

        [Test]
        public void Cannot_Unegister_UntilInitialized()
        {
            var dependencyInjector = new DependencyInjector(Array.Empty<Type>());
            Scene testScene = SceneManager.GetActiveScene();

            Assert.Throws<InvalidOperationException>(() => dependencyInjector.UnregisterSceneServices(testScene));

            dependencyInjector.Initialize(new UnityDebugLoggerFactory());
            Assert.DoesNotThrow(() => dependencyInjector.UnregisterSceneServices(testScene));
        }

        [Test]
        public void Cannot_Resolve_UntilInitialized()
        {
            var dependencyInjector = new DependencyInjector(Array.Empty<Type>());
            object testClient = new();

            Assert.Throws<InvalidOperationException>(() => dependencyInjector.ResolveDependenciesOf(testClient));

            dependencyInjector.Initialize(new UnityDebugLoggerFactory());
            Assert.DoesNotThrow(() => dependencyInjector.ResolveDependenciesOf(testClient));
        }

        #endregion

        #region Service registration tests

        [Test]
        public void Registers_Instance_SameTypeDifferentTag()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();

            // Test fails if these lines throw or log an error
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "not-test"));
        }

        [Test]
        public void Registers_Factory_SameTypeDifferentTag()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();

            // Test fails if these lines throw or log an error
            dependencyInjector.RegisterService(() => getComponentService<TestComponent>(), "test");
            dependencyInjector.RegisterService(() => getComponentService<TestComponent>(), "not-test");
        }

        [Test]
        public void CannotRegister_Instance_SameTypeAndTag_SameScene()
        {
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

        [Test]
        public void CannotRegister_Factory_SameTypeAndTag_SameScene()
        {
            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(getComponentService<TestComponent>("test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            // ACT/ASSERT
            Assert.Throws<InvalidOperationException>(() =>
                dependencyInjector.RegisterService(() => getComponentService<TestComponent>(), "test"));

            Assert.Throws<InvalidOperationException>(() =>
                dependencyInjector.RegisterService(() => getComponentService<TestComponent>()));
        }

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void CannotRegister_Instance_SameTypeAndTag_OtherScene()
        {
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

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void CannotRegister_Factory_SameTypeAndTag_OtherScene()
        {
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
        public void Registers_ComponentInstances_WithInspectorTag()
        {
            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();

            var obj = new GameObject { tag = "test" };
            TestComponent serviceInstance = obj.AddComponent<TestComponent>();
            dependencyInjector.RegisterService(serviceInstance);

            // ACT/ASSERT
            dependencyInjector.TryGetService(typeof(TestComponent), tag: "test", clientName: "Unit test", out Service service);
            Assert.That(service.Instance, Is.SameAs(serviceInstance));

            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(TestComponent), tag: "other", clientName: "Unit test", out _));
        }

        [Test]
        public void Registers_NonComponentInstances_WithDefaultTag()
        {
            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector();

            object serviceInstance = new();
            dependencyInjector.RegisterService(serviceInstance);

            // ACT/ASSERT
            dependencyInjector.TryGetService(typeof(object), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out Service service);
            Assert.That(service.Instance, Is.SameAs(serviceInstance));

            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(object), tag: "test", clientName: "Unit test", out _));
        }

        #endregion

        #region Service resolution tests

        [Test]
        public void Resolves_Instance_ByType()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            object serviceInstance = new();
            dependencyInjector.RegisterService(serviceInstance);

            dependencyInjector.TryGetService(typeof(object), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out Service service);

            Assert.That(service.Instance, Is.SameAs(serviceInstance));
        }

        [Test]
        public void Resolves_Instance_DerivedType()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            var serviceInstance = new InvalidOperationException();
            dependencyInjector.RegisterService<Exception>(serviceInstance);

            dependencyInjector.TryGetService(typeof(Exception), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out Service service);

            Assert.That(service.Instance, Is.SameAs(serviceInstance));
            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.TryGetService(typeof(ApplicationException), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out _));
        }

        [Test]
        public void Resolves_Instance_ByTypeAndTag()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            var obj = new GameObject { tag = "test" };
            TestComponent serviceInstance = obj.AddComponent<TestComponent>();
            dependencyInjector.RegisterService(serviceInstance);

            dependencyInjector.TryGetService(typeof(TestComponent), tag: "test", clientName: "Unit test", out Service service);

            Assert.That(service.Instance, Is.SameAs(serviceInstance));
        }

        [Test]
        public void Resolves_Factory_ByType()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            object serviceInstance = new();
            dependencyInjector.RegisterService(() => serviceInstance);

            dependencyInjector.TryGetService(typeof(object), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out Service service);

            Assert.That(service.Instance, Is.SameAs(serviceInstance));
        }

        [Test]
        public void Resolves_Factory_DerivedType()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            var serviceInstance = new InvalidOperationException();
            dependencyInjector.RegisterService<Exception, InvalidOperationException>(() => serviceInstance);

            dependencyInjector.TryGetService(typeof(Exception), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out Service service);

            Assert.That(service.Instance, Is.SameAs(serviceInstance));
            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.TryGetService(typeof(ApplicationException), tag: DependencyInjector.DefaultTag, clientName: "Unit test", out _));
        }

        [Test]
        public void Resolves_Factory_ByTypeAndTag()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            TestComponent serviceInstance = new GameObject().AddComponent<TestComponent>();
            dependencyInjector.RegisterService(() => serviceInstance, "test");

            dependencyInjector.TryGetService(typeof(TestComponent), tag: "test", clientName: "Unit test", out Service service);

            Assert.That(service.Instance, Is.SameAs(serviceInstance));
        }

        #endregion

        #region Method injection tests

        [Test]
        public void Cannot_Resolve_MissingService()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();

            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(Component), tag: "any", clientName: "Unit test", out _));
            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(Component), tag: "Untagged", clientName: "Unit test", out _));
            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(MonoBehaviour), tag: "any", clientName: "Unit test", out _));
            Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(MonoBehaviour), tag: "Untagged", clientName: "Unit test", out _));
        }

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void Resolves_ServiceInScene_ClientInDifferentScene() => Assert.Fail();

        [Test]
        public void Resolve_CallsInject_UpInheritanceHierarchy()
        {
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
            DependencyInjector dependencyInjector = getDependencyInjector();
            Assert.DoesNotThrow(() => dependencyInjector.ResolveDependenciesOf(new GameObject()));
        }

        [Test]
        public void Resolve_DoesNotCallInject_WithNoDependencies()
        {
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
            Mock<ITypeMetadataProvider> typeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(typeMetadataProvider: typeMetadataProvider.Object);
            var client = new Mock<IMultipleInjectClient>();

            Assert.Throws<AmbiguousMatchException>(() => dependencyInjector.ResolveDependenciesOf(client.Object));
        }

        [Test]
        public void Resolve_CanCache_RootType()
        {
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
        public void Resolve_CanCache_BaseType()
        {
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
        public void Resolve_Warns_SameTypeNoTags()
        {
            int warnCount = 0;
            ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, () => ++warnCount);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            dependencyInjector.ResolveDependenciesOf(new SameTypeNoTagsClient());

            Assert.That(warnCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_DoesNotWarn_SameTypeDifferentTags()
        {
            int warnCount = 0;
            ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, () => ++warnCount);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "not-test"));

            dependencyInjector.ResolveDependenciesOf(new SameTypeDifferentTagsClient());

            Assert.That(warnCount, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_Warns_SameTypeSameTags()
        {
            int warnCount = 0;
            ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, () => ++warnCount);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "test"));

            dependencyInjector.ResolveDependenciesOf(new SameTypeSameTagsClient());

            Assert.That(warnCount, Is.EqualTo(1));
        }

        #endregion

        #region Constructor injection tests

        [Test]
        public void Construct_TriesConstructors_WithMostParamsFirst()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(new GameObject().AddComponent<TestComponent>());
            dependencyInjector.RegisterService(new object());
            dependencyInjector.RegisterService(getComponentService<Animator>());

            MultiConstructorClient instance = dependencyInjector.Construct<MultiConstructorClient>();

            Assert.That(instance.NumParamsInUsedConstructor, Is.EqualTo(3));
        }

        [Test]
        public void Construct_TriesConstructors_WithDecreasingNumParams()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(new GameObject().AddComponent<TestComponent>());
            dependencyInjector.RegisterService(new object());

            MultiConstructorClient instance = dependencyInjector.Construct<MultiConstructorClient>();

            // Not enough services were registered for 3-param constructor, so falls back to constructor with next most params (2-param constructor not 1-param)
            Assert.That(instance.NumParamsInUsedConstructor, Is.EqualTo(2));
        }

        [Test]
        public void Construct_YieldsSameResult_GenericOrNonGeneric()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(new GameObject().AddComponent<TestComponent>());

            MultiConstructorClient genericInstance = dependencyInjector.Construct<MultiConstructorClient>();
            var nonGenericInstance = (MultiConstructorClient)dependencyInjector.Construct(typeof(MultiConstructorClient));

            Assert.That(genericInstance.NumParamsInUsedConstructor, Is.EqualTo(1));
            Assert.That(nonGenericInstance.NumParamsInUsedConstructor, Is.EqualTo(1));
            Assert.That(genericInstance, Is.Not.SameAs(nonGenericInstance));    // Separate instances of same type
        }

        [Test]
        public void Construct_WithoutConstructor_DoesNotThrow()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            Assert.DoesNotThrow(() => dependencyInjector.Construct<NoConstructorClient>());
        }

        [Test]
        public void Construct_CallsEmptyConstructor_IfOnlyOption()
        {
            Mock<ITypeMetadataProvider> typeMetadataProvider = getTypeMetadataProvider();
            typeMetadataProvider
                .Setup(x => x.GetMethodParameters(It.IsAny<ConstructorInfo>()))
                .Returns((ConstructorInfo constructor) => constructor.GetParameters());
            DependencyInjector dependencyInjector = getDependencyInjector(typeMetadataProvider: typeMetadataProvider.Object);

            // Client constructor takes no parameters, but is still invoked to instantiate an object
            dependencyInjector.Construct<EmptyConstructorClient>();
            typeMetadataProvider.Verify(x => x.GetMethodParameters(It.IsAny<ConstructorInfo>()), Times.Once);
        }

        [Test]
        public void Construct_CanCache()
        {
            Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
            DependencyInjector dependencyInjector = getDependencyInjector(
                cachedResolutionTypes: new[] { typeof(MultiConstructorClient) },
                typeMetadataProvider: mockTypeMetadataProvider.Object
            );
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            // Initial, uncached resolution
            dependencyInjector.Construct<MultiConstructorClient>();
            mockTypeMetadataProvider.Verify(x => x.GetConstructors(typeof(MultiConstructorClient)), Times.Once);

            // Cached resolution
            dependencyInjector.Construct<MultiConstructorClient>();
            mockTypeMetadataProvider.Verify(x => x.GetConstructors(typeof(MultiConstructorClient)), Times.Once);
        }

        [Test]
        public void Construct_Warns_SameTypeNoTags()
        {
            int warnCount = 0;
            ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, () => ++warnCount);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            dependencyInjector.Construct<ConstructorSameTypeNoTagsClient>();

            Assert.That(warnCount, Is.EqualTo(1));
        }

        [Test]
        public void Construct_DoesNotWarn_SameTypeDifferentTags()
        {
            int warnCount = 0;
            ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, () => ++warnCount);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "test"));
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "not-test"));

            dependencyInjector.Construct<ConstructorSameTypeDifferentTagsClient>();

            Assert.That(warnCount, Is.EqualTo(0));
        }

        [Test]
        public void Construct_Warns_SameTypeSameTags()
        {
            int warnCount = 0;
            ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, () => ++warnCount);
            DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
            dependencyInjector.RegisterService(getComponentService<TestComponent>(tag: "test"));

            dependencyInjector.Construct<ConstructorSameTypeSameTagsClient>();

            Assert.That(warnCount, Is.EqualTo(1));
        }

        #endregion

        #region Resolution recording tests

        [Test]
        public void CanRecord_During_ResolveDependenciesOf()
        {
            Type clientType = typeof(TestClient);
            Type baseType = typeof(TestClientBase);
            Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
            DependencyResolutionCounts counts;
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
            counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.EqualTo(1));
            Assert.That(counts.Uncached[clientType], Is.EqualTo(1));
            Assert.That(counts.Cached.Count, Is.EqualTo(1));
            Assert.That(counts.Cached[baseType], Is.EqualTo(1));

            // Second, base type resolution cached
            var cacheClient = new TestClient();
            dependencyInjector.ResolveDependenciesOf(cacheClient);
            counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.EqualTo(1));
            Assert.That(counts.Uncached[clientType], Is.EqualTo(2));
            Assert.That(counts.Cached.Count, Is.EqualTo(1));
            Assert.That(counts.Cached[baseType], Is.EqualTo(2));

            // Clears cached resolutions
            dependencyInjector.RecordingResolutions = false;
            counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.Zero);
            Assert.That(counts.Cached.Count, Is.Zero);
        }

        [Test]
        public void CanRecord_During_Construct()
        {
            Type clientType = typeof(MultiConstructorClient);
            Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
            DependencyResolutionCounts counts;
            DependencyInjector dependencyInjector = getDependencyInjector(
                cachedResolutionTypes: new[] { typeof(MultiConstructorClient) },
                typeMetadataProvider: mockTypeMetadataProvider.Object
            );
            dependencyInjector.RegisterService(getComponentService<TestComponent>());
            dependencyInjector.RegisterService(new object());
            dependencyInjector.RegisterService(getComponentService<Animator>());
            dependencyInjector.RecordingResolutions = true;

            // Initial, uncached resolution
            dependencyInjector.Construct<MultiConstructorClient>();
            counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.EqualTo(0));
            Assert.That(counts.Cached.Count, Is.EqualTo(1));
            Assert.That(counts.Cached[clientType], Is.EqualTo(1));

            // Second, type resolution cached
            dependencyInjector.Construct<MultiConstructorClient>();
            counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.EqualTo(0));
            Assert.That(counts.Cached.Count, Is.EqualTo(1));
            Assert.That(counts.Cached[clientType], Is.EqualTo(2));

            // Clears cached resolutions
            dependencyInjector.RecordingResolutions = false;
            counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.Zero);
            Assert.That(counts.Cached.Count, Is.Zero);
        }

        [Test]
        public void Record_Stop_ClearsRecordedResolutions()
        {
            // ARRANGE
            DependencyInjector dependencyInjector = getDependencyInjector(cachedResolutionTypes: new[] { typeof(TestClientBase) });
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            // ACT
            var client = new TestClientBase();
            dependencyInjector.RecordingResolutions = true;
            dependencyInjector.ResolveDependenciesOf(client);
            dependencyInjector.RecordingResolutions = false;

            // ASSERT
            DependencyResolutionCounts counts = dependencyInjector.ServiceResolutionCounts;
            Assert.That(counts.Uncached.Count, Is.Zero);
            Assert.That(counts.Cached.Count, Is.Zero);
        }

        [Test]
        public void ResolutionsNotStored_WhenNotRecording_During_ResolveDependenciesOf()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(getComponentService<TestComponent>());

            dependencyInjector.RecordingResolutions = false;
            dependencyInjector.ResolveDependenciesOf(new TestClientBase());
            dependencyInjector.ResolveDependenciesOf(new TestClientBase());
            dependencyInjector.ResolveDependenciesOf(new TestClientBase());
            DependencyResolutionCounts counts = dependencyInjector.ServiceResolutionCounts;

            Assert.That(counts.Cached, Is.Empty);
            Assert.That(counts.Uncached, Is.Empty);
        }

        [Test]
        public void ResolutionsNotStored_WhenNotRecording_During_Construct()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            dependencyInjector.RegisterService(getComponentService<TestComponent>());
            dependencyInjector.RegisterService(new object());
            dependencyInjector.RegisterService(getComponentService<Animator>());

            dependencyInjector.RecordingResolutions = false;
            dependencyInjector.Construct<MultiConstructorClient>();
            dependencyInjector.Construct<MultiConstructorClient>();
            dependencyInjector.Construct<MultiConstructorClient>();
            DependencyResolutionCounts counts = dependencyInjector.ServiceResolutionCounts;

            Assert.That(counts.Cached, Is.Empty);
            Assert.That(counts.Uncached, Is.Empty);
        }

        #endregion

        #region Service unregistration tests

        [Test]
        public void Can_Unregister_AllServicesFromScene()
        {
            DependencyInjector dependencyInjector = getDependencyInjector();
            Scene activeScene = SceneManager.GetActiveScene();
            dependencyInjector.RegisterService(new object(), SceneManager.GetActiveScene());
            dependencyInjector.RegisterService(new InvalidOperationException(), SceneManager.GetActiveScene());

            dependencyInjector.UnregisterSceneServices(activeScene);

            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.TryGetService(typeof(object), DependencyInjector.DefaultTag, clientName: "Unit test", out _));
            Assert.Throws<KeyNotFoundException>(() =>
                dependencyInjector.TryGetService(typeof(Exception), DependencyInjector.DefaultTag, clientName: "Unit test", out _));
        }

        [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
        public void Can_Unregister_AllServicesFromScene_LeavesOtherSceneServices() => Assert.Fail();

        #endregion

        private static DependencyInjector getDependencyInjector(
            Type[]? cachedResolutionTypes = null,
            ILoggerFactory? loggerFactory = null,
            ITypeMetadataProvider? typeMetadataProvider = null
        )
        {
            var dependencyInjector = new DependencyInjector(cachedResolutionTypes ?? Array.Empty<Type>());
            dependencyInjector.Initialize(
                loggerFactory ?? new UnityDebugLoggerFactory(),
                typeMetadataProvider ?? new TypeMetadataProvider()
            );

            return dependencyInjector;
        }
        private static T getComponentService<T>(string tag = "Untagged") where T : Component => new GameObject { tag = tag }.AddComponent<T>();
        private static Mock<ITypeMetadataProvider> getTypeMetadataProvider()
        {
            var mock = new Mock<ITypeMetadataProvider>();
            var typeMetadataProvider = new TypeMetadataProvider();

            mock.Setup(x => x.GetMethod(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<BindingFlags>()))
                .Returns<Type, string, BindingFlags>((type, name, bindingFlags) => typeMetadataProvider.GetMethod(type, name, bindingFlags));

            mock.Setup(x => x.GetConstructors(It.IsAny<Type>()))
                .Returns<Type>(type => typeMetadataProvider.GetConstructors(type));

            mock.Setup(x => x.GetMethodParameters(It.IsAny<MethodBase>()))
                .Returns<MethodBase>(method => typeMetadataProvider.GetMethodParameters(method));

            mock.Setup(x => x.GetCustomAttribute<InjectTagAttribute>(It.IsAny<ParameterInfo>()))
                .Returns<ParameterInfo>(param => typeMetadataProvider.GetCustomAttribute<InjectTagAttribute>(param));

            mock.Setup(x => x.CompileMethodCall(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MethodInfo>(), It.IsAny<object[]>()))
                .Returns<string, string, MethodInfo, object[]>((methodName, paramName, injectMethod, args) =>
                    typeMetadataProvider.CompileMethodCall(methodName, paramName, injectMethod, args));

            mock.Setup(x => x.CompileConstructorCall(It.IsAny<ConstructorInfo>(), It.IsAny<object[]>()))
                .Returns<ConstructorInfo, object[]>((constructor, args) =>
                    typeMetadataProvider.CompileConstructorCall(constructor, args));

            return mock;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Unity.Extensions.Logging;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtil.DependencyInjection;
using UnityUtil.Editor.Tests;
using UnityUtil.Tests.Util;

namespace UnityUtil.Tests.Editor.DependencyInjection;

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

// These types are just for testing so I don't wanna think about this...
#pragma warning disable CA1822 // Mark members as static

// Parameters are just for testing
#pragma warning disable CS9113 // Parameter is unread.

internal class SameTypeNoTagsClient
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
    public void Inject(TestComponent componentService1, TestComponent componentService2) { }
}

internal class SameTypeDifferentTagsClient
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
    public void Inject([InjectTag("test")] TestComponent componentService1, [InjectTag("not-test")] TestComponent componentService2) { }
}

internal class SameTypeSameTagsClient
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
    public void Inject([InjectTag("test")] TestComponent componentService1, [InjectTag("test")] TestComponent componentService2) { }
}

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
internal class NoConstructorClient { }

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
internal class EmptyConstructorClient
{
    public EmptyConstructorClient() { }
}

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
internal class MultiConstructorClient(TestComponent componentService)
{
    public int NumParamsInUsedConstructor { get; private set; } = 1;

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
    public MultiConstructorClient(TestComponent componentService, object objectService, Animator animatorService) : this(componentService, objectService) => NumParamsInUsedConstructor = 3;

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are just for testing")]
    public MultiConstructorClient(TestComponent componentService, object objectService) : this(componentService) => NumParamsInUsedConstructor = 2;
}

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
internal class ConstructorSameTypeNoTagsClient(
    TestComponent componentService1,
    TestComponent componentService2
) { }

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
internal class ConstructorSameTypeDifferentTagsClient(
    [InjectTag("test")] TestComponent componentService1,
    [InjectTag("not-test")] TestComponent componentService2
) { }


[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI during test")]
internal class ConstructorSameTypeSameTagsClient(
    [InjectTag("test")] TestComponent componentService1,
    [InjectTag("test")] TestComponent componentService2
) { }

#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore CA1822 // Mark members as static

#endregion

public class DependencyInjectorTest : BaseEditModeTestFixture
{
    #region Initialization tests

    [Test]
    public void Cannot_Initialize_MultipleTimes()
    {
        var dependencyInjector = new DependencyInjector(cachedResolutionTypes: []);
        dependencyInjector.Initialize(new UnityDebugLoggerFactory());

        _ = Assert.Throws<InvalidOperationException>(() => dependencyInjector.Initialize(new UnityDebugLoggerFactory()));
    }

    [Test]
    public void Cannot_Register_UntilInitialized()
    {
        var dependencyInjector = new DependencyInjector(cachedResolutionTypes: []);
        object testService = new();

        _ = Assert.Throws<InvalidOperationException>(() => dependencyInjector.RegisterService(testService));

        dependencyInjector.Initialize(new UnityDebugLoggerFactory());
        Assert.DoesNotThrow(() => dependencyInjector.RegisterService(testService));
    }

    [Test]
    public void Cannot_Unegister_UntilInitialized()
    {
        var dependencyInjector = new DependencyInjector(cachedResolutionTypes: []);
        Scene testScene = SceneManager.GetActiveScene();

        _ = Assert.Throws<InvalidOperationException>(() => dependencyInjector.UnregisterSceneServices(testScene));

        dependencyInjector.Initialize(new UnityDebugLoggerFactory());
        Assert.DoesNotThrow(() => dependencyInjector.UnregisterSceneServices(testScene));
    }

    [Test]
    public void Cannot_Resolve_UntilInitialized()
    {
        var dependencyInjector = new DependencyInjector(cachedResolutionTypes: []);
        object testClient = new();

        _ = Assert.Throws<InvalidOperationException>(() => dependencyInjector.ResolveDependenciesOf(testClient));

        dependencyInjector.Initialize(new UnityDebugLoggerFactory());
        Assert.DoesNotThrow(() => dependencyInjector.ResolveDependenciesOf(testClient));
    }

    #endregion

    #region Service registration tests

    [Test]
    public void Registers_Instance_SameTypeDifferentTag()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        const string TEST_TAG = "test";

        Assert.DoesNotThrow(() => dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG));
        Assert.DoesNotThrow(() => dependencyInjector.RegisterService(getComponentService<TestComponent>(), "not-" + TEST_TAG));
    }

    [Test]
    public void Registers_Factory_SameTypeDifferentTag()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        const string TEST_TAG = "test";

        Assert.DoesNotThrow(() => dependencyInjector.RegisterService(() => getComponentService<TestComponent>(), TEST_TAG));
        Assert.DoesNotThrow(() => dependencyInjector.RegisterService(() => getComponentService<TestComponent>(), "not-" + TEST_TAG));
    }

    [Test]
    public void CannotRegister_Instance_SameTypeAndTag_SameScene([Values] bool explicitScene)
    {
        // ARRANGE
        DependencyInjector dependencyInjector = getDependencyInjector();
        Scene? scene = explicitScene ? SceneManager.GetActiveScene() : null;
        const string TEST_TAG = "test";

        // ACT/ASSERT
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG, scene: scene);
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG, scene: scene));

        dependencyInjector.RegisterService(getComponentService<TestComponent>(), scene: scene);
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>(), scene: scene));
    }

    [Test]
    public void CannotRegister_Factory_SameTypeAndTag_SameScene([Values] bool explicitScene)
    {
        // ARRANGE
        DependencyInjector dependencyInjector = getDependencyInjector();
        Scene? scene = explicitScene ? SceneManager.GetActiveScene() : null;
        const string TEST_TAG = "test";

        // ACT/ASSERT
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG, scene: scene);
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG, scene: scene));

        dependencyInjector.RegisterService(getComponentService<TestComponent>(), scene: scene);
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(() => getComponentService<TestComponent>(), scene: scene));
    }

    [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
    public void CannotRegister_Instance_SameTypeAndTag_OtherScene()
    {
        // ARRANGE
        DependencyInjector dependencyInjector = getDependencyInjector();
        Scene otherScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        _ = SceneManager.SetActiveScene(otherScene);
        const string TEST_TAG = "test";

        // ACT/ASSERT
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG);
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG));

        dependencyInjector.RegisterService(getComponentService<TestComponent>());
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>()));

        _ = EditorSceneManager.CloseScene(otherScene, removeScene: true);
    }

    [Test, Ignore("Haven't figured out a way to open a new Scene while in the unsaved scene created by the Test Runner")]
    public void CannotRegister_Factory_SameTypeAndTag_OtherScene()
    {
        // ARRANGE
        DependencyInjector dependencyInjector = getDependencyInjector();
        Scene otherScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        _ = SceneManager.SetActiveScene(otherScene);
        const string TEST_TAG = "test";

        // ACT/ASSERT
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG);
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG));

        dependencyInjector.RegisterService(getComponentService<TestComponent>());
        _ = Assert.Throws<InvalidOperationException>(() =>
            dependencyInjector.RegisterService(getComponentService<TestComponent>()));

        _ = EditorSceneManager.CloseScene(otherScene, removeScene: true);
    }

    [Test]
    public void Registers_DefaultInjectTag()
    {
        // ARRANGE
        DependencyInjector dependencyInjector = getDependencyInjector();

        object serviceInstance = new();

        // ACT/ASSERT
        dependencyInjector.RegisterService(serviceInstance);
        dependencyInjector.TryGetService(typeof(object), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out Service service);
        Assert.That(service.Instance, Is.SameAs(serviceInstance));

        _ = Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(object), injectTag: "test", clientName: "Unit test", out _));
    }

    #endregion

    #region Service resolution tests

    [Test]
    public void Resolves_Instance_ByType()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        object serviceInstance = new();
        dependencyInjector.RegisterService(serviceInstance);

        dependencyInjector.TryGetService(typeof(object), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out Service service);

        Assert.That(service.Instance, Is.SameAs(serviceInstance));
    }

    [Test]
    public void Resolves_Instance_DerivedType()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        var serviceInstance = new InvalidOperationException();
        dependencyInjector.RegisterService<Exception>(serviceInstance);

        dependencyInjector.TryGetService(typeof(Exception), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out Service service);

        Assert.That(service.Instance, Is.SameAs(serviceInstance));
        _ = Assert.Throws<KeyNotFoundException>(() =>
            dependencyInjector.TryGetService(typeof(ApplicationException), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out _));
    }

    [Test]
    public void Resolves_Instance_ByTypeAndTag()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        TestComponent serviceInstance = getComponentService<TestComponent>();
        const string TEST_TAG = "test";

        dependencyInjector.RegisterService(serviceInstance, TEST_TAG);
        dependencyInjector.TryGetService(typeof(TestComponent), TEST_TAG, clientName: "Unit test", out Service service);

        Assert.That(service.Instance, Is.SameAs(serviceInstance));
    }

    [Test]
    public void Resolves_Factory_ByType()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        object serviceInstance = new();
        dependencyInjector.RegisterService(() => serviceInstance);

        dependencyInjector.TryGetService(typeof(object), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out Service service);

        Assert.That(service.Instance, Is.SameAs(serviceInstance));
    }

    [Test]
    public void Resolves_Factory_DerivedType()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        var serviceInstance = new InvalidOperationException();
        dependencyInjector.RegisterService<Exception, InvalidOperationException>(() => serviceInstance);

        dependencyInjector.TryGetService(typeof(Exception), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out Service service);

        Assert.That(service.Instance, Is.SameAs(serviceInstance));
        _ = Assert.Throws<KeyNotFoundException>(() =>
            dependencyInjector.TryGetService(typeof(ApplicationException), injectTag: DependencyInjector.DefaultInjectTag, clientName: "Unit test", out _));
    }

    [Test]
    public void Resolves_Factory_ByTypeAndTag()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();
        TestComponent serviceInstance = new GameObject().AddComponent<TestComponent>();
        dependencyInjector.RegisterService(() => serviceInstance, "test");

        dependencyInjector.TryGetService(typeof(TestComponent), injectTag: "test", clientName: "Unit test", out Service service);

        Assert.That(service.Instance, Is.SameAs(serviceInstance));
    }

    #endregion

    #region Method injection tests

    [Test]
    public void Cannot_Resolve_MissingService()
    {
        DependencyInjector dependencyInjector = getDependencyInjector();

        _ = Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(Component), injectTag: "any", clientName: "Unit test", out _));
        _ = Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(Component), injectTag: "Untagged", clientName: "Unit test", out _));
        _ = Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(MonoBehaviour), injectTag: "any", clientName: "Unit test", out _));
        _ = Assert.Throws<KeyNotFoundException>(() => dependencyInjector.TryGetService(typeof(MonoBehaviour), injectTag: "Untagged", clientName: "Unit test", out _));
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
        _ = client.Setup(x => x.Inject()).Callback(() => { });
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

        _ = Assert.Throws<AmbiguousMatchException>(() => dependencyInjector.ResolveDependenciesOf(client.Object));
    }

    [Test]
    public void Resolve_CanCache_RootType()
    {
        Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
        DependencyInjector dependencyInjector = getDependencyInjector(
            cachedResolutionTypes: [typeof(TestClientBase)],
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
            cachedResolutionTypes: [typeof(TestClientBase)],
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
        ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, (_, _, _, _) => ++warnCount);
        DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
        dependencyInjector.RegisterService(getComponentService<TestComponent>());

        dependencyInjector.ResolveDependenciesOf(new SameTypeNoTagsClient());

        Assert.That(warnCount, Is.EqualTo(1));
    }

    [Test]
    public void Resolve_DoesNotWarn_SameTypeDifferentTags()
    {
        int warnCount = 0;
        ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, (_, _, _, _) => ++warnCount);
        DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
        const string TEST_TAG = "test";

        dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG);
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), "not-" + TEST_TAG);
        dependencyInjector.ResolveDependenciesOf(new SameTypeDifferentTagsClient());

        Assert.That(warnCount, Is.EqualTo(0));
    }

    [Test]
    public void Resolve_Warns_SameTypeSameTags()
    {
        int warnCount = 0;
        ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, (_, _, _, _) => ++warnCount);
        DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), "test");

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
        MultiConstructorClient nonGenericInstance = dependencyInjector.Construct<MultiConstructorClient>();

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
        _ = typeMetadataProvider
            .Setup(x => x.GetMethodParameters(It.IsAny<ConstructorInfo>()))
            .Returns((ConstructorInfo constructor) => constructor.GetParameters());
        DependencyInjector dependencyInjector = getDependencyInjector(typeMetadataProvider: typeMetadataProvider.Object);

        // Client constructor takes no parameters, but is still invoked to instantiate an object
        _ = dependencyInjector.Construct<EmptyConstructorClient>();
        typeMetadataProvider.Verify(x => x.GetMethodParameters(It.IsAny<ConstructorInfo>()), Times.Once);
    }

    [Test]
    public void Construct_CanCache()
    {
        Mock<ITypeMetadataProvider> mockTypeMetadataProvider = getTypeMetadataProvider();
        DependencyInjector dependencyInjector = getDependencyInjector(
            cachedResolutionTypes: [typeof(MultiConstructorClient)],
            typeMetadataProvider: mockTypeMetadataProvider.Object
        );
        dependencyInjector.RegisterService(getComponentService<TestComponent>());

        // Initial, uncached resolution
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        mockTypeMetadataProvider.Verify(x => x.GetConstructors(typeof(MultiConstructorClient)), Times.Once);

        // Cached resolution
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        mockTypeMetadataProvider.Verify(x => x.GetConstructors(typeof(MultiConstructorClient)), Times.Once);
    }

    [Test]
    public void Construct_Warns_SameTypeNoTags()
    {
        int warnCount = 0;
        ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, (_, _, _, _) => ++warnCount);
        DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);

        dependencyInjector.RegisterService(getComponentService<TestComponent>());
        _ = dependencyInjector.Construct<ConstructorSameTypeNoTagsClient>();

        Assert.That(warnCount, Is.EqualTo(1));
    }

    [Test]
    public void Construct_DoesNotWarn_SameTypeDifferentTags()
    {
        int warnCount = 0;
        ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, (_, _, _, _) => ++warnCount);
        DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);
        const string TEST_TAG = "test";

        dependencyInjector.RegisterService(getComponentService<TestComponent>(), TEST_TAG);
        dependencyInjector.RegisterService(getComponentService<TestComponent>(), "not-" + TEST_TAG);
        _ = dependencyInjector.Construct<ConstructorSameTypeDifferentTagsClient>();

        Assert.That(warnCount, Is.EqualTo(0));
    }

    [Test]
    public void Construct_Warns_SameTypeSameTags()
    {
        int warnCount = 0;
        ILoggerFactory loggerFactory = new LogLevelCallbackLoggerFactory(LogLevel.Warning, (_, _, _, _) => ++warnCount);
        DependencyInjector dependencyInjector = getDependencyInjector(loggerFactory: loggerFactory);

        dependencyInjector.RegisterService(getComponentService<TestComponent>(), "test");
        _ = dependencyInjector.Construct<ConstructorSameTypeSameTagsClient>();

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
            cachedResolutionTypes: [typeof(TestClientBase)],
            typeMetadataProvider: mockTypeMetadataProvider.Object
        );
        dependencyInjector.RegisterService(getComponentService<TestComponent>());
        dependencyInjector.RegisterService(new object());
        dependencyInjector.RegisterService(getComponentService<Animator>());
        dependencyInjector.RecordingResolutions = true;

        // Initial, uncached resolution
        var uncacheClient = new TestClient();
        dependencyInjector.ResolveDependenciesOf(uncacheClient);
        counts = dependencyInjector.GetServiceResolutionCounts();
        Assert.That(counts.Uncached.Count, Is.EqualTo(1));
        Assert.That(counts.Uncached[clientType], Is.EqualTo(1));
        Assert.That(counts.Cached.Count, Is.EqualTo(1));
        Assert.That(counts.Cached[baseType], Is.EqualTo(1));

        // Second, base type resolution cached
        var cacheClient = new TestClient();
        dependencyInjector.ResolveDependenciesOf(cacheClient);
        counts = dependencyInjector.GetServiceResolutionCounts();
        Assert.That(counts.Uncached.Count, Is.EqualTo(1));
        Assert.That(counts.Uncached[clientType], Is.EqualTo(2));
        Assert.That(counts.Cached.Count, Is.EqualTo(1));
        Assert.That(counts.Cached[baseType], Is.EqualTo(2));

        // Clears cached resolutions
        dependencyInjector.RecordingResolutions = false;
        counts = dependencyInjector.GetServiceResolutionCounts();
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
            cachedResolutionTypes: [typeof(MultiConstructorClient)],
            typeMetadataProvider: mockTypeMetadataProvider.Object
        );
        dependencyInjector.RegisterService(getComponentService<TestComponent>());
        dependencyInjector.RegisterService(new object());
        dependencyInjector.RegisterService(getComponentService<Animator>());
        dependencyInjector.RecordingResolutions = true;

        // Initial, uncached resolution
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        counts = dependencyInjector.GetServiceResolutionCounts();
        Assert.That(counts.Uncached.Count, Is.EqualTo(0));
        Assert.That(counts.Cached.Count, Is.EqualTo(1));
        Assert.That(counts.Cached[clientType], Is.EqualTo(1));

        // Second, type resolution cached
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        counts = dependencyInjector.GetServiceResolutionCounts();
        Assert.That(counts.Uncached.Count, Is.EqualTo(0));
        Assert.That(counts.Cached.Count, Is.EqualTo(1));
        Assert.That(counts.Cached[clientType], Is.EqualTo(2));

        // Clears cached resolutions
        dependencyInjector.RecordingResolutions = false;
        counts = dependencyInjector.GetServiceResolutionCounts();
        Assert.That(counts.Uncached.Count, Is.Zero);
        Assert.That(counts.Cached.Count, Is.Zero);
    }

    [Test]
    public void Record_Stop_ClearsRecordedResolutions()
    {
        // ARRANGE
        DependencyInjector dependencyInjector = getDependencyInjector(cachedResolutionTypes: [typeof(TestClientBase)]);
        dependencyInjector.RegisterService(getComponentService<TestComponent>());

        // ACT
        var client = new TestClientBase();
        dependencyInjector.RecordingResolutions = true;
        dependencyInjector.ResolveDependenciesOf(client);
        dependencyInjector.RecordingResolutions = false;

        // ASSERT
        DependencyResolutionCounts counts = dependencyInjector.GetServiceResolutionCounts();
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
        DependencyResolutionCounts counts = dependencyInjector.GetServiceResolutionCounts();

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
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        _ = dependencyInjector.Construct<MultiConstructorClient>();
        DependencyResolutionCounts counts = dependencyInjector.GetServiceResolutionCounts();

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
        dependencyInjector.RegisterService(new object(), scene: SceneManager.GetActiveScene());
        dependencyInjector.RegisterService(new InvalidOperationException(), scene: SceneManager.GetActiveScene());

        dependencyInjector.UnregisterSceneServices(activeScene);

        _ = Assert.Throws<KeyNotFoundException>(() =>
            dependencyInjector.TryGetService(typeof(object), DependencyInjector.DefaultInjectTag, clientName: "Unit test", out _));
        _ = Assert.Throws<KeyNotFoundException>(() =>
            dependencyInjector.TryGetService(typeof(Exception), DependencyInjector.DefaultInjectTag, clientName: "Unit test", out _));
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
        var dependencyInjector = new DependencyInjector(cachedResolutionTypes ?? []);
        dependencyInjector.Initialize(
            loggerFactory ?? new UnityDebugLoggerFactory(),
            typeMetadataProvider ?? new TypeMetadataProvider()
        );

        return dependencyInjector;
    }
    private static T getComponentService<T>() where T : Component => new GameObject().AddComponent<T>();
    private static Mock<ITypeMetadataProvider> getTypeMetadataProvider()
    {
        var mock = new Mock<ITypeMetadataProvider>();
        var typeMetadataProvider = new TypeMetadataProvider();

        _ = mock.Setup(x => x.GetMethod(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<BindingFlags>()))
            .Returns<Type, string, BindingFlags>(typeMetadataProvider.GetMethod);

        _ = mock.Setup(x => x.GetConstructors(It.IsAny<Type>()))
            .Returns<Type>(typeMetadataProvider.GetConstructors);

        _ = mock.Setup(x => x.GetMethodParameters(It.IsAny<MethodBase>()))
            .Returns<MethodBase>(typeMetadataProvider.GetMethodParameters);

        _ = mock.Setup(x => x.GetCustomAttribute<InjectTagAttribute>(It.IsAny<ParameterInfo>()))
            .Returns<ParameterInfo>(typeMetadataProvider.GetCustomAttribute<InjectTagAttribute>);

        _ = mock.Setup(x => x.CompileMethodCall(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MethodInfo>(), It.IsAny<object[]>()))
            .Returns<string, string, MethodInfo, object[]>(typeMetadataProvider.CompileMethodCall);

        _ = mock.Setup(x => x.CompileConstructorCall(It.IsAny<ConstructorInfo>(), It.IsAny<object[]>()))
            .Returns<ConstructorInfo, object[]>(typeMetadataProvider.CompileConstructorCall);

        return mock;
    }
}

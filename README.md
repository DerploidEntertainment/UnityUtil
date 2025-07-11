# UnityUtil

A set of utility classes and components useful to any Unity project, 2D or 3D.

> [!NOTE]
> Work in progress! This package has been under open-source development since ~2017, but only since late 2022 has it been seriously considered for "usability" by 3rd parties,
so documentation content/organization are still in development.

## Contents

- [Installing](#installing)
  - [Updating](#updating)
- [Packages](#packages)
- [Note on IL2CPP](#note-on-il2cpp)
- [Support](#support)
- [Contributing](#contributing)
- [License](#license)

## Installing

1. Make sure you have both [Git](https://git-scm.com/) and [Git LFS](https://git-lfs.github.com/) installed before adding this package to your Unity project.
2. Add the [UnityNuGet](https://github.com/bdovaz/UnityNuGet) scoped registry so that you can install NuGet packages through the Unity Package Manager.
3. Install non-UPM dependencies in your Unity project. This is an opinionated list of 3rd party assets/packages that UnityUtil leverages for certain features.
    Unfortunately, some of these assets cost money. In the future, UnityUtil's features will be broken up into separate packages,
    so that users can ignore specific packages and not spend money on their Asset Store dependencies.
    - [Odin Inspector](https://odininspector.com/) (v3.0.12 or above). We strongly recommend _not_ installing Odin as an
        [embedded UPM package](https://odininspector.com/tutorials/getting-started/install-odin-inspector-as-a-unity-package),
        as it just makes later updates to the asset more difficult.
4. Install UPM dependenies of your desired UnityUtil package(s). This is necessary because UnityUtil packages are installed from Git URLs, so their UPM dependencies are not auto-resolved.
5. In the Unity Editor, open the [Package Manager window](https://docs.unity3d.com/Manual/upm-ui.html), click the `+` button in the upper-left and choose `Add package from git URL...`.
6. Paste a URL in the format `https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/<package>#<branch>` where...
    - `<package>` is one of the [packages](#packages) listed below
    - `<branch>` is...
      - `main` for the latest stable version of `<package>`
      - `unity<unityVersion>` for the latest stable version of `<package>` built against Unity `<unityVersion>` (e.g., `unity6`)
      - `<version>-unity<unityVersion>` for `<version>` of `<package>` built against Unity `<unityVersion>` (e.g., `0.1.0-unity6`)
      - `unity<unityVersion>-dev` for the latest development version of `<package>` built against Unity `<unityVersion>`. **These versions are bleeding-edge and very likely to contain bugs!**
    - For example, to install version `0.2.0` of the `UnityUtil.Triggers` package for Unity 6, you would enter the URL: `https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/UnityUtil.Triggers#0.2.0-unity6`

### Updating

You can update this package from Unity's Package Manager window, even when it is imported as a git repo.
Doing so will update the commit from which changes are imported.
As the API stabilizes, I will move this package to [OpenUPM](https://openupm.com/) and add a changelog to make its versioning more clear.

## Packages

- [Serilog.Enrichers.Unity](./src/Logging/Serilog.Enrichers.Unity/README.md): Implements a [Serilog](https://serilog.net/) enricher that dynamically adds Unity data to your log events
- [Serilog.Sinks.Unity](./src/Logging/Serilog.Sinks.Unity/README.md): Implements a [Serilog](https://serilog.net/) sink that writes logs to the Unity Console window, optionally including `tag` strings and `context` objects
- [Unity.Extensions.Logging](./src/Logging/Unity.Extensions.Logging/README.md): Unity-specific extension methods for logging with [Microsoft.Extensions.Logging](https://www.nuget.org/packages/microsoft.extensions.logging/)
- [Unity.Extensions.Serilog](./src/Logging/Unity.Extensions.Serilog/README.md): Unity-specific logging extension methods for [Serilog](https://serilog.net/)
- [UnityUtil](./src/UnityUtil/UnityUtil/README.md): utility classes and components related to dependency injection, logging, mathematics, data storage, etc.
- [UnityUtil.Configuration.RemoteConfig](./src/UnityUtil/UnityUtil.Configuration.RemoteConfig/README.md): Implementats a `Microsoft.Extensions.Configuration` configuration provider for Unity Remote Config
- [UnityUtil.Inputs](./src/UnityUtil/UnityUtil.Inputs/README.md): abstracted player inputs
- [UnityUtil.Interactors](./src/UnityUtil/UnityUtil.Interactors/README.md): interaction with in-world objects
- [UnityUtil.Inventory](./src/UnityUtil/UnityUtil.Inventory/README.md): in-game item inventories
- [UnityUtil.Legal](./src/UnityUtil/UnityUtil.Legal/README.md): legal/privacy consent
- [UnityUtil.Movement](./src/UnityUtil/UnityUtil.Movement/README.md): in-game movement mechanics
- [UnityUtil.Physics](./src/UnityUtil/UnityUtil.Physics/README.md): 3D physics helpers
- [UnityUtil.Physics2D](./src/UnityUtil/UnityUtil.Physics2D/README.md): 2D physics helpers
- [UnityUtil.Triggers](./src/UnityUtil/UnityUtil.Triggers/README.md): event-based behavior in-game
- [UnityUtil.UI](./src/UnityUtil/UnityUtil.UI/README.md): in-game and in-Editor user interfaces

## Note on IL2CPP

Sometimes, you need to preserve code elements from [managed code stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html) by IL2CPP during builds.
For example, your app may produce runtime code that doesn't exist when Unity performs the static analysis, e.g. through reflection and/or dependency injection.
You can use Unity's `[Preserve]` mechansim to preserve these elements in your own code;
however, UnityUtil intentionally does _not_ annotate any code with `[Preserve]` so that you have total control over the size of your builds.
Therefore, if you need to preserve UnityUtil code elements (types, methods, etc.),
then you must use the [`link.xml` approach](https://docs.unity3d.com/Manual/ManagedCodeStripping.html#LinkXMLAnnotation) described in the Unity Manual.

## Support

For bug reports and feature requests, please search through the existing [Issues](https://github.com/DerploidEntertainment/UnityUtil/issues) first, then create a new one if necessary.

## Contributing

Make sure you have [Git LFS](https://git-lfs.github.com/) installed before cloning this repo.

To build/test changes to this package locally, you can:

- Open the test Unity project under the [`UnityUtil/`](./UnityUtil) subfolder.
    There you can run play/edit mode tests from the [Test Runner window](https://docs.unity3d.com/Packages/com.unity.test-framework@1.3/manual/workflow-run-test.html).
- Open the Visual Studio solution under the [`src/`](./src) subfolder.
    Building that solution will automatically re-export DLLs/PDBs to the above Unity project.
- Import the package locally in a test project. Simply create a new test project (or open an existing one),
    then import this package [from the local folder](https://docs.unity3d.com/Manual/upm-localpath.html) where you cloned it.

See the [Contributing docs](./CONTRIBUTING.md) for more info.

## License

[MIT](./LICENSE.md)

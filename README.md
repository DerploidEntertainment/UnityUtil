# UnityUtil

A set of utility classes and components useful to any Unity project, 2D or 3D.

## Installing

1. Make sure you have both [Git](https://git-scm.com/) and [Git LFS](https://git-lfs.github.com/) installed before adding this package to your Unity project.
2. Install dependencies in your Unity project. This is an opinionated list of 3rd party assets/packages that UnityUtil leverages for certain features.
    Unfortunately, some of these assets cost money. In the future, I will break apart UnityUtil's features into separate packages,
    so that users can ignore specific packages and not spend money on their Asset Store dependencies.
    - [Odin Inspector](https://odininspector.com/) (v3.0.12 or above).
        After installing, close the Editor and copy the `Sirenix/` folder from `Assets/` to a new `odininspector/` folder under `Packages/`.
        Also add a `package.json` file to the new folder as described in [Odin's docs](https://odininspector.com/tutorials/getting-started/install-odin-inspector-as-a-unity-package).
        Re-open Unity to see Odin installed as an embedded package.
3. In the Unity Editor, open the [Package Manager window](https://docs.unity3d.com/Manual/upm-ui.html), click the `+` button in the upper-left and choose `Add package from git URL...`.
4. Paste one of the following URLs:
    - `https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/UnityUtil#main` for the latest stable version
    - `https://github.com/DerploidEntertainment/UnityUtil.git?path=/UnityUtil/Assets/UnityUtil#<branch>` for experimental features on `<branch>`

To keep this package updated, you'll just have to check this repo every now and then.

As the API stabilizes, I will move this package to [OpenUPM](https://openupm.com/) so that the Package Manager window will display available updates more obviously.

## Documentation

**Coming soon!**

This package has been under open-source development since ~2017, but only since late 2022 have I gotten serious about making it "usable" by 3rd parties.

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

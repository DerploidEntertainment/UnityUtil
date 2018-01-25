namespace UnityUtil {

    /// <summary>
    /// Determines how this <see cref="DependencyInjector"/> locates clients
    /// </summary>
    public enum DependencyInjectionMode {
        /// <summary>
        /// Required dependencies will only be injected into the <see cref="GameObject"/>s specified in the <see cref="DependencyInjector.Clients"/> array.  Use this if you know, at design time, exactly which <see cref="GameObject"/>s require dependencies.
        /// </summary>
        SpecifiedClients,

        /// <summary>
        /// Required dependencies will be injected into the <see cref="GameObject"/>s specified in the <see cref="DependencyInjector.Clients"/> array, as well as all of their child objects (recursively).  Use this if you know that all objects requiring dependencies will be parented to a specific <see cref="GameObject"/>.
        /// </summary>
        SpecifiedClientsPlusChildren,

        /// <summary>
        /// Every <see cref="GameObject"/> in the Scene will be checked for required dependencies, and have those dependencies injected.  This is the easiest option, guaranteeing that all <see cref="MonoBehaviour"/>s in the scene will be associated with their correct dependencies, but is also the slowest slowest option, but 
        /// </summary>
        EntireScene
    }

}

using System;

namespace UnityEngine
{

    /// <summary>
    /// Inject the service configured with this field's <see cref="Type"/> and an optional Inspector tag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class InjectTagAttribute : PropertyAttribute {

        /// <summary></summary>
        /// <param name="tag">The service <see cref="UnityEngine.Object"/> with this tag (set in the Inspector) will be injected.  Use when registering multiple services with the same Type.</param>
        public InjectTagAttribute(string tag) {
            Tag = tag;
        }

        /// <summary>
        /// The service <see cref="UnityEngine.Object"/> with this tag (set in the Inspector) will be injected.  Use when registering multiple services with the same Type.
        /// </summary>
        public string Tag { get; }
    }

}

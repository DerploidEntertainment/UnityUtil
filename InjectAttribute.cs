using System;
using UnityEngine;

namespace UnityUtil {

    /// <summary>
    /// Inject the service configured with this field's <see cref="Type"/> and an optional Inspector tag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class InjectAttribute : PropertyAttribute {

        /// <summary></summary>
        public InjectAttribute() { }
        /// <summary></summary>
        /// <param name="tag">The service <see cref="MonoBehaviour"/> with this tag (set in the Inspector) will be injected.  Use when there must be multiple services with the same Type.</param>
        public InjectAttribute(string tag) {
            Tag = tag;
        }

        /// <summary>
        /// The service <see cref="MonoBehaviour"/> with this tag (set in the Inspector) will be injected.  Use when there must be multiple services with the same Type.
        /// </summary>
        public string Tag { get; }
    }

}

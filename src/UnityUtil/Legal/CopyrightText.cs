using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Logging;
using UnityEngine.UI;

namespace UnityEngine.Legal {
    public class CopyrightText : Configurable {
        [Tooltip("This string is used to populate " + nameof(Text) + ". '{0}' will be replaced with the current date and '{1}' will be replaced with " + nameof(Application.companyName) + ", using .NET composite formatting. For example, '{0:yyyy}' would be replaced with just the current 4-digit year. See here for details: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting")]
        public string FormatString;
        public Text Text;

        [Conditional("UNITY_EDITOR")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Reset() => FormatString = "© {0}, {1}";
        protected override void Awake() {
            base.Awake();

            this.AssertAssociation(Text, nameof(Text));

            Text.text = string.Format(FormatString, DateTime.Now, Application.companyName);
        }
    }
}

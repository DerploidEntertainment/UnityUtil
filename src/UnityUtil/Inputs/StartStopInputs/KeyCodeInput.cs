namespace UnityEngine.Inputs {

    [CreateAssetMenu(fileName = "key-code-input", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inputs)}/{nameof(UnityEngine.Inputs.KeyCodeInput)}")]
    public sealed class KeyCodeInput : StartStopInput {

        public KeyCode KeyCode;

        public override bool Started() => Input.GetKeyDown(KeyCode);
        public override bool Happening() => Input.GetKey(KeyCode);
        public override bool Stopped() => Input.GetKeyUp(KeyCode);

    }

}

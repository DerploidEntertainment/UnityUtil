namespace UnityEngine.Movement;

public abstract class RigidbodyMovement : MonoBehaviour
{
    public void Move(Rigidbody rb) => DoMove(rb);
    protected abstract void DoMove(Rigidbody rb);

}

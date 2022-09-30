namespace UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ModifiableGraphic : MonoBehaviour
{

    private Graphic? _graphic;
    private Graphic Graphic => _graphic ??= GetComponent<Graphic>();

    public void SetColorR(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.r = value; Graphic.color = curr; }
    public void SetColorG(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.g = value; Graphic.color = curr; }
    public void SetColorB(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.b = value; Graphic.color = curr; }
    public void SetColorA(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.a = value; Graphic.color = curr; }

    private bool hasGraphic()
    {
        if (Graphic == null) {
            Debug.LogWarning($"Could not get the {nameof(Graphic)} of this {nameof(ModifiableGraphic)}. No values will be changed. Try enterring Play Mode once to correct this.");
            return false;
        }
        return true;
    }

}

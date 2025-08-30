using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UI;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.UI;

[RequireComponent(typeof(Graphic))]
public class ModifiableGraphic : MonoBehaviour
{
    private ILogger<ModifiableGraphic>? _logger;

    private Graphic? _graphic;
    private Graphic Graphic => _graphic = _graphic != null ? _graphic : GetComponent<Graphic>();

    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    public void SetColorR(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.r = value; Graphic.color = curr; }
    public void SetColorG(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.g = value; Graphic.color = curr; }
    public void SetColorB(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.b = value; Graphic.color = curr; }
    public void SetColorA(float value) { if (!hasGraphic()) return; Color curr = Graphic.color; curr.a = value; Graphic.color = curr; }

    private bool hasGraphic()
    {
        if (Graphic == null) {
            log_NoGraphic();
            return false;
        }
        return true;
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_NO_GRAPHIC_ACTION = LoggerMessage.Define(Warning,
        new EventId(id: 0, nameof(log_NoGraphic)),
        $"Could not get the {nameof(Graphic)} of this {nameof(ModifiableGraphic)}. No values will be changed. Try enterring Play Mode once to correct this."
    );
    private void log_NoGraphic() => LOG_NO_GRAPHIC_ACTION(_logger!, null);

    #endregion
}

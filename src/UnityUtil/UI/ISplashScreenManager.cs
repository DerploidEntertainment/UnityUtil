using System.Diagnostics.CodeAnalysis;

namespace UnityUtil.UI;

public interface ISplashScreenManager
{
    void Begin();
    void Draw();

    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "I don't care about VB keywords")]
    void Stop();
}

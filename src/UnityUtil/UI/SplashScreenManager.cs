using UnityEngine.Events;
using UnityEngine.Logging;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    public class SplashScreenManager : MonoBehaviour, ISplashScreenManager
    {
        public SplashScreen.StopBehavior StopBehavior;

        public UnityEvent StartedDrawing = new();
        public UnityEvent StoppedDrawing = new();

        public void Begin()
        {
            Debug.Log("Initializing splash screen...", context: this);
            SplashScreen.Begin();
        }

        public void Draw()
        {
            Debug.Log("Starting to draw splash screen...", context: this);
            SplashScreen.Draw();
            StartedDrawing.Invoke();
        }

        public void Stop()
        {
            Debug.Log("Stopping splash screen...", context: this);
            SplashScreen.Stop(StopBehavior);
            StoppedDrawing.Invoke();
        }
    }
}

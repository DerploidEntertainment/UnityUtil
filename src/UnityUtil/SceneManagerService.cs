using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityUtil;

public class SceneManagerService : MonoBehaviour
{

    public LoadSceneMode LoadSceneMode = LoadSceneMode.Single;

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode);

    public void LoadSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode);

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void UnloadSceneAsync(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UnityEvents can't call static methods")]
    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

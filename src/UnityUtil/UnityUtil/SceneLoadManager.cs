using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityUtil;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(SceneLoadManager)}", fileName = "scene-load-manager")]
public class SceneLoadManager : ScriptableObject, ISceneLoadManager
{
    [Button]
    public void SetActiveScene(string sceneName) => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

    [Button]
    public void LoadSingleScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

    [Button]
    public void LoadSingleSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

    [Button]
    public void LoadAdditiveScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

    [Button]
    public void LoadAdditiveSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

    [Button]
    public void UnloadSceneAsync(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);

    [Button]
    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

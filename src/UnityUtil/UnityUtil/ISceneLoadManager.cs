namespace UnityUtil;

public interface ISceneLoadManager
{
    void LoadAdditiveScene(string sceneName);
    void LoadAdditiveSceneAsync(string sceneName);
    void LoadSingleScene(string sceneName);
    void LoadSingleSceneAsync(string sceneName);
    void RestartScene();
    void UnloadSceneAsync(string sceneName);
}

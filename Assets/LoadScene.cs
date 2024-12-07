using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public SceneAsset Scene;

    public void LoadTheScene()
    {
        SceneManager.LoadScene(Scene.name);
    }
}

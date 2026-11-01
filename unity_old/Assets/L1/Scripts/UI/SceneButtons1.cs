
using UnityEngine;
using UnityEngine.SceneManagement;

// ควบคุมปุ่ม UI และเปลี่ยน Scene ผ่าน FadeManager
public class SceneButtons1 : MonoBehaviour
{
    public void Start()
    {
        RunState.ResetRun();
        LoadScene("Lore");
    }

    public void GoToGame()
    {
        RunState.ResetRun();
        LoadScene("Game");
    }

    public void Title()
    {
        RunState.ResetRun();
        LoadScene("Title");
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void LoadScene(string sceneName)
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

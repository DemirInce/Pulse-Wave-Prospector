using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void Resume()
    {
        GameManager.Instance.UnPause();
    }
    public void QuitToDesktopButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void QQuitToMainmenuButtonuitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScreen");
    }
}

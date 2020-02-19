using UnityEngine.SceneManagement;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    public void StartGamePressed()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitPressed()
    {
        Application.Quit();
    }
}

using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Text HighScore;
    public Text Score;

    void Start()
    {
        HighScore.text = PlayerInfo.HighScore.ToString();
        Score.text = PlayerInfo.CurrentScore.ToString();
    }

    public void RetryPressed()
    {
        SceneManager.LoadScene("Game");
    }

    public void MenuPressed()
    {
        SceneManager.LoadScene("MainScreen");
    }
}

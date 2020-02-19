using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    private float xScale;
    private float yScale;

    public void MenuPressed()
    {
        SceneManager.LoadScene("MainScreen");
    }

    void OnMouseDown()
    {
        xScale = transform.localScale.x;
        yScale = transform.localScale.y;
        transform.localScale = new Vector2(xScale * 1.1f, yScale * 1.1f);
    }

    void OnMouseUp()
    {
        transform.localScale = new Vector2(xScale, yScale);
    }
}

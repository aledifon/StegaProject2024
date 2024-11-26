using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float factorCameraSize;
    public GameObject panelGameOver;
    public GameObject buttonUI;
    bool gameOver;

    public void GameOver()
    {
        panelGameOver.SetActive(true);
        gameOver = true;
        //Enable the ButtonUI once elapsed 1s
        Invoke("ActivateButtonUI", 2);
    }
    void ActivateButtonUI()
    {
        buttonUI.SetActive(true);
    }
    private void Update()
    {
        //When game Over panel is enabled then the Camera Size will increase every frame
        //We'll limit the Camera size between (3,6)
        if (gameOver)
        {
            Camera.main.orthographicSize -= Time.deltaTime * factorCameraSize;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3, 6);
        }
    }
    public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}

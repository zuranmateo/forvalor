using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManagerScript : MonoBehaviour
{
    public GameObject defeatPanel;
    public GameObject victoryPanel;
    public GameObject pausePanel;

    // Start is called before the first frame update
    void Start()
    {
        defeatPanel.SetActive(false);
        victoryPanel.SetActive(false); 
        pausePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            OpenPauseMenu();
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void OpenPauseMenu()
    {
        if (!defeatPanel.activeInHierarchy && !victoryPanel.activeInHierarchy)
        {
            if (pausePanel.activeInHierarchy)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
}

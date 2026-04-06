using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject SettingsMenu;
    public TextMeshProUGUI howtoplayText;

    [Header("music")]
    public AudioSource audiosource;
    public AudioClip happymusic;
    public AudioClip happymusic2;
    public AudioClip battlemusic1;
    public float musicTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        musicTimer += Time.deltaTime;
        if (audiosource.clip == null || musicTimer >= audiosource.clip.length)
        {
            int r = Convert.ToInt32(UnityEngine.Random.Range(0, 3));
            switch (r)
            {
                case 0:
                    audiosource.clip = happymusic;
                    break;
                case 1:
                    audiosource.clip = happymusic2;
                    break;
                case 2:
                    audiosource.clip = battlemusic1;
                    break;
            }
            audiosource.Play();
            musicTimer = 0f;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        MainMenu.SetActive(false);   
        SettingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        MainMenu.SetActive(true);
        SettingsMenu.SetActive(false);
    }

    public void TurnPageOnHowtoplay(Slider slider)
    {
        howtoplayText.pageToDisplay = Convert.ToInt32(slider.value);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}

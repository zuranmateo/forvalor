using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class throneScript : MonoBehaviour
{
    public string throneRedTag = "throneRed";
    public string throneBlueTag = "throneBlue"; 

    public float health = 500;
    public bool destroyed = false;
    public GameObject defeatPanel;
    public GameObject victoryPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            if (CompareTag(throneBlueTag))
            {
                victoryPanel.SetActive(true);
                destroyed = true;
                Time.timeScale = 0;
            }
            else if (CompareTag(throneRedTag))
            {
                defeatPanel.SetActive(true);
                destroyed = true;
                Time.timeScale = 0;
            }
        }
    }
}

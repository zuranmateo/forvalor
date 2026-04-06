using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject AIcontrollerBlue;

    [Header("money")]
    public float playerMoney = 0;
    public float AIblueMoney = 0;
    public float playerDefaultAddValue = 1;
    public float AIblueDefaultAddValue = 1;

    [Header("iron")]
    public float playerIron = 0;
    public float AIblueIron = 0;
    public float playerDefaultIronAddValue = 0.5f;
    public float AIblueDefaultIronAddValue = 0.5f;

    public int numberOfBlue = 0;
    public int numberOfRed = 0;

    [Header("costs")]
    public float spearmanCost = 14;
    public float bowmanCost = 18;
    public float swordmanCost = 20;
    public float gatesRepairCost = 200;
    public float defaultUpgradeCost = 50;
    public float defaultMoneyUpgradeCost = 100;

    [Header("rules")]
    public int maxRedTroops = 200;
    public int maxBlueTroops = 200;

    [Header("tags")]
    public string speardamage = "spearDamage";
    public string spearmanBlueTag = "SpearManBlue";
    public string spearmanRedTag = "SpearManRed";
    public string bowmanRedTag = "BowManRed";
    public string bowmanBlueTag = "BowManBlue";
    public string swordmanRedTag = "SwordManRed";
    public string swordmanBlueTag = "SwordManBlue";
    public string DeadLayer = "dead";
    public string RedDoorTag = "doorRed";
    public string BlueDoorTag = "doorBlue";
    public string throneRedTag = "throneRed";
    public string throneBlueTag = "throneBlue";

    [Header("music")]
    public AudioSource audiosource;
    public AudioClip happymusic;
    public AudioClip happymusic2;
    public AudioClip battlemusic1;

    [Header("timers")]
    public float checkAndTakeMoneyTimer = 60f;
    float timer = 0f;
    public float musicTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        InvokeRepeating(nameof(checkAndTakeMoney), checkAndTakeMoneyTimer, checkAndTakeMoneyTimer);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime; // šteješ èas med frejmi
        musicTimer += Time.deltaTime;

        if (timer >= 1f) // 1 sekunda mimo
        {
            timer = 0f;
            playerMoney += playerDefaultAddValue;
            AIblueMoney += AIblueDefaultAddValue;
            playerIron += playerDefaultIronAddValue;
            AIblueIron += AIblueDefaultIronAddValue;
            //Debug.Log("player: " + playerMoney + " AI: " + AIblueMoney);

            numberOfBlue = CountObjectsWithTag(spearmanBlueTag, bowmanBlueTag, swordmanBlueTag);
            numberOfRed = CountObjectsWithTag(spearmanRedTag, bowmanRedTag, swordmanRedTag);
        }

        if (audiosource.clip == null || musicTimer >= audiosource.clip.length)
        {
            int r = Convert.ToInt32(UnityEngine.Random.Range(0, 3));
            switch(r)
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

    int CountObjectsWithTag(string tag1, string tag2, string tag3)
    {
        return GameObject.FindGameObjectsWithTag(tag1).Length + GameObject.FindGameObjectsWithTag(tag2).Length + GameObject.FindGameObjectsWithTag(tag3).Length;
    }

    void checkAndTakeMoney()
    {
        float takeMoney = 0;
        float takeIron = 0;
        if (playerMoney > 0 && playerIron > 0)
        {
            numberOfRed = CountObjectsWithTag(spearmanRedTag, bowmanRedTag, swordmanRedTag);
            takeMoney = numberOfRed * 0.5f;
            takeIron = numberOfRed * 0.01f;
            playerMoney -= takeMoney;
            playerIron -= takeIron;
        }
        else
        {
            throneScript throne = GameObject.FindGameObjectWithTag(throneRedTag).GetComponent<throneScript>();
            if (throne != null)
            {
                throne.health -= 100;
            }
        }

        takeMoney = 0;
        takeIron = 0;

        if (AIblueMoney > 0 && AIblueIron > 0)
        {
            numberOfBlue = CountObjectsWithTag(spearmanBlueTag, bowmanBlueTag, swordmanBlueTag);
            takeMoney = numberOfBlue * 1f;
            takeIron = numberOfBlue * 0.02f;
            AIblueMoney -= takeMoney;
            AIblueIron -= takeIron;
        }
        else
        {
            throneScript throne = GameObject.FindGameObjectWithTag(throneBlueTag).GetComponent<throneScript>();
            if (throne != null)
            {
                throne.health -= 100;
            }
        }
    }
}


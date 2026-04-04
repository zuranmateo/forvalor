using UnityEngine;

public class MoneyBuldingScript : MonoBehaviour
{
    [Header("buildings")]
    public GameObject unbuilt;
    public GameObject smity;
    public GameObject market;

    public float health = 0;
    public float maxHealth = 750;
    public bool destroyed = true;

    [Header("canvas")]
    public GameObject canvas;

    [Header("sounds")]
    public AudioSource AudioSource;
    public AudioClip UIclick;

    [Header("externals")]
    public GameManagerScript gameManagerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManagerScript = FindAnyObjectByType<GameManagerScript>();
        unbuilt.SetActive(true);
        smity.SetActive(false);
        market.SetActive(false);
        canvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0 && !destroyed)
        {
            destroyBuild();
        }
    }

    private void OnMouseUp()
    {
        if (CompareTag("RedBuilding"))
        {
            /*
            if(canvas.activeInHierarchy == false)
            {
                canvas.SetActive(true);
            }
            else
            {
                canvas.SetActive(false);
            }
            */
            canvas.SetActive(true);
        }
        AudioSource.PlayOneShot(UIclick);

    }

    public void buildSmitty()
    {
        if (CompareTag("RedBuilding"))
        {
            if (gameManagerScript.playerMoney >= gameManagerScript.defaultMoneyUpgradeCost)
            {
                gameManagerScript.playerMoney -= gameManagerScript.defaultMoneyUpgradeCost;
                unbuilt.SetActive(false);
                smity.SetActive(true);
                market.SetActive(false);
                canvas.SetActive(false);

                gameManagerScript.playerDefaultAddValue += 0.5f;
                gameManagerScript.playerDefaultIronAddValue += 0.1f;
                health = maxHealth;
                destroyed = false;
            }
        }
        else if (CompareTag("BlueBuilding"))
        {
            if (gameManagerScript.AIblueMoney >= gameManagerScript.defaultMoneyUpgradeCost)
            {
                gameManagerScript.AIblueMoney -= gameManagerScript.defaultMoneyUpgradeCost;
                unbuilt.SetActive(false);
                smity.SetActive(true);
                market.SetActive(false);
                canvas.SetActive(false);

                gameManagerScript.AIblueDefaultAddValue += 0.5f;
                gameManagerScript.AIblueDefaultIronAddValue += 0.1f;
                health = maxHealth;
                destroyed = false;
            }
        }

    }
    public void buildMarket()
    {
        if (CompareTag("RedBuilding"))
        {
            if (gameManagerScript.playerMoney >= gameManagerScript.defaultMoneyUpgradeCost)
            {
                gameManagerScript.playerMoney -= gameManagerScript.defaultMoneyUpgradeCost;
                unbuilt.SetActive(false);
                smity.SetActive(false);
                market.SetActive(true);
                canvas.SetActive(false);

                gameManagerScript.playerDefaultAddValue += 1;
                health = maxHealth;
                destroyed = false;
            }
        }
        else if (CompareTag("BlueBuilding"))
        {
            if (gameManagerScript.AIblueMoney >= gameManagerScript.defaultMoneyUpgradeCost)
            {
                gameManagerScript.AIblueMoney -= gameManagerScript.defaultMoneyUpgradeCost;
                unbuilt.SetActive(false);
                smity.SetActive(false);
                market.SetActive(true);
                canvas.SetActive(false);

                gameManagerScript.AIblueDefaultAddValue += 1;
                health = maxHealth;
                destroyed = false;
            }
        }
        

    }

    public void destroyBuild()
    {
        if (health <= 0)
        { 
            if (CompareTag("RedBuilding"))
            {
                if (smity.activeInHierarchy)
                {
                    unbuilt.SetActive(true);
                    smity.SetActive(false);
                    market.SetActive(false);
                    canvas.SetActive(false);

                    gameManagerScript.playerDefaultAddValue -= 0.5f;
                    gameManagerScript.playerDefaultIronAddValue -= 0.1f;
                    health = 0;
                }
                else
                {
                    unbuilt.SetActive(true);
                    smity.SetActive(false);
                    market.SetActive(false);
                    canvas.SetActive(false);

                    gameManagerScript.playerDefaultAddValue -= 1;
                    health = 0;
                }
                destroyed = true;
            }
            else if (CompareTag("BlueBuilding"))
            {
                if (smity.activeInHierarchy)
                {
                    unbuilt.SetActive(true);
                    smity.SetActive(false);
                    market.SetActive(false);
                    canvas.SetActive(false);

                    gameManagerScript.AIblueDefaultAddValue -= 0.5f;
                    gameManagerScript.playerDefaultIronAddValue -= 0.1f;
                    health = 0;
                }
                else
                {
                    unbuilt.SetActive(true);
                    smity.SetActive(false);
                    market.SetActive(false);
                    canvas.SetActive(false);

                    gameManagerScript.AIblueDefaultAddValue -= 1;
                    health = 0;
                }
                destroyed = true;
            }
            
        }
    }
}

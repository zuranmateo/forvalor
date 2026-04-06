using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UIplayerController;


[System.Serializable]
public class FormationPreset
{
    public int[] identities;
    public int[] counts;
}

public class AIplayerScrpit : MonoBehaviour
{
    [Header("choices")]
    public float upgrades = 20;
    public float moneyUpgrades = 20;
    public float formations = 20;
    public float reinforceFormations = 20;
    public float repair = 20;

    [Header("timer and ai")]
    public float choiceTime = 40;
    public float stratTime = 5;
    public int turnsWithoutUpgrades = 0; //dont forget to ++ every time upgrades is not selected... if upgrades selected then 0
    public int turnsWithoutRepairs = 0;
    public bool nextIsFormations = false;
    public bool nextIsReinforce = false;
    public int deads = 0;
    public int orderNumber = 0;

    [Header("upgrades")]
    public int skill = 1;
    public int movespeed = 1;
    public int armor = 1;

    [Header("externals")]
    public GameManagerScript gameManagerScript;
    public GameObject AibattleControllerBlue;
    public GameObject FormationPrefab;
    public GameObject Spawner;

    [Header("tags")]
    public string formationRedTag = "formationRed";
    public string formationBlueTag = "formationBlue";
    public string RedDoorTag = "doorRed";
    public string BlueDoorTag = "doorBlue";
    public string SpawnerAllyTag = "SpawnPointBlue";
    public string spearmanBlueTag = "SpearManBlue";
    public string spearmanRedTag = "SpearManRed";
    public string bowmanRedTag = "BowManRed";
    public string bowmanBlueTag = "BowManBlue";
    public string swordmanRedTag = "SwordManRed";
    public string swordmanBlueTag = "SwordManBlue";
    public string AIblueDEFtag = "AImovePointBlueDEF";
    public string AIblueBleftTag = "AImovePointBlueBleft";
    public string AIblueBrightTag = "AImovePointBlueBright";
    public string BlueBuildingTag = "BlueBuilding";
    public string RedBuildingTag = "RedBuilding";
    public string AImoveBlueATK = "AImovePointBlueATK";


    [Header("conmbat")]
    public bool forceDefend = false;
    public bool forceBDefend = false;
    public int stratIndex = 0;
    public class AIformations
    {
        public GameObject formation;
        public GameObject AIcontrollerBlue;
        public AIcontrollerBlue script;
    }

    List<AIformations> aiFormations = new List<AIformations>();

    [SerializeField]
    public List<FormationPreset> presets = new List<FormationPreset>();
    
    List<slots> Slot = new List<slots>();

    void Start()
    {
        gameManagerScript = FindAnyObjectByType<GameManagerScript>();
        InvokeRepeating(nameof(makedecision), choiceTime, choiceTime);
        InvokeRepeating(nameof(useStrategy), stratTime, stratTime);
    }

    // Update is called once per frame

    void Update()
    {
        
    }

    public void makedecision()
    {
        upgrades = 20;
        moneyUpgrades = 20;
        formations = 20;
        reinforceFormations = 20;
        repair = 20;
        //upgrades
        upgrades += turnsWithoutUpgrades * 4;


        //money upgrades
        float diff = gameManagerScript.playerDefaultAddValue - gameManagerScript.AIblueDefaultAddValue;
        if(diff > 0)
        {
            moneyUpgrades += diff * 8;
        }
        else
        {
            moneyUpgrades += diff;
        }

        int x = 0;
        GameObject[] buildings = GameObject.FindGameObjectsWithTag(BlueBuildingTag);
        foreach (GameObject building in buildings)
        {
            MoneyBuldingScript mon = building.GetComponent<MoneyBuldingScript>();   
            if (mon != null)
            {
                if (mon.destroyed)
                {
                    x++;
                }
            }
        }
        if(x <= 0)
        {
            moneyUpgrades = 0;
        }


        //formations
        if(gameManagerScript.numberOfRed < gameManagerScript.numberOfBlue)
        {
            float d = gameManagerScript.numberOfBlue - gameManagerScript.numberOfRed;
            float scaled = Mathf.Sign(d) * Mathf.Sqrt(Mathf.Abs(d));
            int dif = Mathf.RoundToInt(scaled / 3f);
            //dif = -1000;
            formations = formations - dif * 8;
            //Debug.Log("red < blue, diff = " + dif + " formations = " + formations);
        }
        else if(gameManagerScript.numberOfRed == gameManagerScript.numberOfBlue)
        {

        }
        else
        {
            float d = gameManagerScript.numberOfBlue - gameManagerScript.numberOfRed;
            float scaled = Mathf.Sign(d) * Mathf.Sqrt(Mathf.Abs(d));
            int dif = Mathf.RoundToInt(scaled / 3f);
            formations = formations - dif * 8;
            //Debug.Log("blue < red, diff = " + dif + " formations = " + formations);

            if (gameManagerScript.numberOfBlue <= 0) formations = 100;
        }


        //reinforce formations
        GameObject[] blueFormations = GameObject.FindGameObjectsWithTag(formationBlueTag);
        foreach ( GameObject blueFormation in blueFormations)
        {
            FormationController fcon = blueFormation.GetComponent<FormationController>();
            if (fcon != null)
            {
                foreach(var slot in fcon.formationslots)
                {
                    if (!slot.alive)
                    {
                        deads++;
                    }
                }
            }
        }

        if(gameManagerScript.numberOfBlue > 0)
        {
            float deadPecantage = (deads / gameManagerScript.numberOfBlue) * 100;

            if (deadPecantage > 5)
            {
                reinforceFormations = reinforceFormations * ((deads / gameManagerScript.numberOfBlue) + 2);
            }
            else
            {
                reinforceFormations = 0;
            }
        }
        else
        {
            reinforceFormations = 0;
        }
        //repair
        GameObject blueDoor = GameObject.FindGameObjectWithTag(BlueDoorTag).transform.parent.gameObject;
        doorScript bluedoorScript = blueDoor.GetComponent<doorScript>();
        if (bluedoorScript.destroyed)
        {
            repair = repair + turnsWithoutRepairs * 8;
            forceDefend = true;
        }
        else
        {
            repair = 0;
            forceDefend = false;
        }

        moneyUpgrades = moneyUpgrades + upgrades;
        formations = formations + moneyUpgrades;
        reinforceFormations = reinforceFormations + formations;
        repair = repair + reinforceFormations;

        //Debug.Log("upgrades: " + upgrades + " moneyUpgrades: " + moneyUpgrades + " formations: " + formations + " reinforce: " + reinforceFormations + " repair: " + repair);
        int choiceInt = Convert.ToInt32(UnityEngine.Random.Range(0, repair));
        //Debug.Log("choice: " + choiceInt);
        if (!nextIsReinforce && choiceInt >= 0 && choiceInt < upgrades && !nextIsFormations)
        {
            //upgrades
            Debug.Log("upgrades");
            turnsWithoutUpgrades = 0;
            if (bluedoorScript.destroyed)
            {
                turnsWithoutRepairs++;
            }

            // zašèita proti deljenju z 0
            float wArmor = 1f / (armor + 1);
            float wMove = 1f / (movespeed + 1);
            float wSkill = 1f / (skill + 1);

            float total = wArmor + wMove + wSkill;
            float rnd = UnityEngine.Random.value * total;

            if (rnd < wArmor)
            {
                if (gameManagerScript.AIblueMoney > gameManagerScript.defaultUpgradeCost * armor)
                {
                    gameManagerScript.AIblueMoney -= gameManagerScript.defaultUpgradeCost * armor;
                    armor++;
                }
            }
            else if (rnd < wArmor + wMove)
            {
                if (gameManagerScript.AIblueMoney > gameManagerScript.defaultUpgradeCost * movespeed)
                {
                    gameManagerScript.AIblueMoney -= gameManagerScript.defaultUpgradeCost * movespeed;
                    movespeed++;
                }
            }
            else
            {
                if (gameManagerScript.AIblueMoney > gameManagerScript.defaultUpgradeCost * skill)
                {
                    gameManagerScript.AIblueMoney -= gameManagerScript.defaultUpgradeCost * skill;
                    skill++;
                }
            }

            foreach (var f in aiFormations)
            {
                FormationController fc = f.formation.GetComponent<FormationController>();
                if (fc != null)
                {
                    fc.skill = skill;
                    fc.movespeed = movespeed;
                    fc.armor = armor;
                    fc.upgradeSoliders();
                }
            }


        }
        else if (!nextIsReinforce && choiceInt >= upgrades && choiceInt < moneyUpgrades && !nextIsFormations)
        {
            //money upgrades
            Debug.Log("money upgrades");
            if (bluedoorScript.destroyed)
            {
                turnsWithoutRepairs++;
            }
            turnsWithoutUpgrades++;

            foreach (GameObject building in buildings)
            {
                MoneyBuldingScript mon = building.GetComponent<MoneyBuldingScript>();
                if (mon != null)
                {
                    if (mon.destroyed)
                    {
                        int r = UnityEngine.Random.Range(0, 2);
                        if(r == 0)
                        {
                            mon.buildMarket();
                        }
                        else
                        {
                            mon.buildSmitty();
                        }
                        break;
                    }
                }
            }
        }
        else if (!nextIsReinforce && (choiceInt >= moneyUpgrades && choiceInt < formations || nextIsFormations) && gameManagerScript.maxBlueTroops > gameManagerScript.numberOfBlue)
        {
            Debug.Log("spawn formation");
            int nmbrOfSpear = 0;
            int nmbrOfBow = 0;
            int nmbrOfSword = 0;
            //spawn formation
            if (bluedoorScript.destroyed)
            {
                turnsWithoutRepairs++;
            }
            turnsWithoutUpgrades++;

            // id: 1 = spearman, 2 = bowman, 3 = swordman

            int index = UnityEngine.Random.Range(0, presets.Count);

            int[] identities = presets[index].identities;
            int[] counts = presets[index].counts;


            for (int i = 0; i < identities.Length; i++)
            {
                int id = identities[i];
                int count = counts[i];

                if (id == 0) nmbrOfSpear += count;
                else if (id == 1) nmbrOfBow += count;
                else if (id == 2) nmbrOfSword += count;
            }

            float price = (nmbrOfBow * gameManagerScript.bowmanCost) + (nmbrOfSword * gameManagerScript.swordmanCost) + (nmbrOfSpear * gameManagerScript.spearmanCost);
            int ironprice = nmbrOfBow + nmbrOfSpear + (nmbrOfSword * 2);
            if(gameManagerScript.AIblueMoney >= price && gameManagerScript.AIblueIron >= ironprice)
            {
                gameManagerScript.AIblueMoney -= price;
                gameManagerScript.AIblueIron -= ironprice;

                GameObject clone = Instantiate(AibattleControllerBlue, this.transform);
                clone.name = AibattleControllerBlue.name + orderNumber.ToString();

                

                GenerateArmy(identities, counts, clone);

                Spawner = GameObject.FindGameObjectWithTag(SpawnerAllyTag);
                GameObject formation = Instantiate(FormationPrefab);
                formation.transform.position = Spawner.transform.position;
                formation.transform.rotation = Spawner.transform.rotation;

                formation.name = FormationPrefab.name + orderNumber.ToString();

                orderNumber++;
                FormationController formationController = formation.GetComponent<FormationController>();

                formationController.AIbattle = clone;
                formationController.Spawner = Spawner;

                AIcontrollerBlue UIbattle = clone.GetComponent<AIcontrollerBlue>();
                UIbattle.formation = formation;
                UIbattle.AIplayer = gameObject;

                aiFormations.Add(new AIformations
                {
                    formation = formation,
                    AIcontrollerBlue = clone,
                    script = UIbattle
                });

                formationController.GenerateFormation(Slot);

                nextIsFormations = false;
            }
            else 
            {
                if (gameManagerScript.AIblueMoney >= price + 100 && gameManagerScript.AIblueIron <= ironprice)
                {
                    foreach (GameObject building in buildings)
                    {
                        MoneyBuldingScript mon = building.GetComponent<MoneyBuldingScript>();
                        if (mon != null)
                        {
                            if (mon.destroyed)
                            {
                                mon.buildSmitty();
                                break;
                            }
                        }
                    }
                }
                nextIsFormations = true;
            }

        }
        else if (choiceInt >= formations && choiceInt < reinforceFormations || nextIsReinforce)
        {
            //reinforce formation
            Debug.Log("reinforce formations");
            if (bluedoorScript.destroyed)
            {
                turnsWithoutRepairs++;
            }
            turnsWithoutUpgrades++;

            float cost = 0;
            int ironcost = 0;
            GameObject[] BlueFormations = GameObject.FindGameObjectsWithTag(formationBlueTag);
            foreach (GameObject blueFormation in BlueFormations)
            {
                FormationController fcon = blueFormation.GetComponent<FormationController>();
                if (fcon != null)
                {
                    foreach (var slot in fcon.formationslots)
                    {
                        if (!slot.alive)
                        {
                            if (slot.identity == 1)
                            {
                                cost += gameManagerScript.spearmanCost;
                                ironcost++;
                            }
                            else if (slot.identity == 2)
                            {
                                cost += gameManagerScript.bowmanCost;
                                ironcost++;
                            }
                            else if (slot.identity == 3)
                            {
                                cost += gameManagerScript.swordmanCost;
                                ironcost += 2;
                            }
                        }
                    }

                    if (cost <= gameManagerScript.AIblueMoney && gameManagerScript.AIblueIron >= ironcost)
                    {
                        nextIsReinforce = false;
                        gameManagerScript.AIblueMoney -= cost;
                        gameManagerScript.AIblueIron -= ironcost;
                        fcon.reinforce();
                    }
                    else
                    {
                        if(gameManagerScript.AIblueMoney >= cost + 100 && gameManagerScript.AIblueIron <= cost)
                        {
                            foreach (GameObject building in buildings)
                            {
                                MoneyBuldingScript mon = building.GetComponent<MoneyBuldingScript>();
                                if (mon != null)
                                {
                                    if (mon.destroyed)
                                    {
                                        mon.buildSmitty();
                                        break;
                                    }
                                }
                            }
                        }
                        nextIsReinforce = true;
                    }
                }
            }
        }
        else if (choiceInt >= reinforceFormations && choiceInt < repair)
        {
            //repair
            Debug.Log("repair");
            turnsWithoutRepairs = 0;
            turnsWithoutUpgrades++;
            if (gameManagerScript.gatesRepairCost <= gameManagerScript.AIblueMoney)
            {
                gameManagerScript.AIblueMoney -= gameManagerScript.gatesRepairCost;
                bluedoorScript.repairGate();
            }
        }
    }



    public void GenerateArmy(
    int[] rowIdentities,
    int[] rowCounts,
    GameObject battleCommands
)
    {
        int width = 14;
        int positionY = 0;

        int solidersInRow = width - rowCounts[0];

        for (int row = 0; row < rowCounts.Length; row++)
        {
            int identity = rowIdentities[row];
            int count = rowCounts[row];

            int startX = (width - count) / 2;

            for (int i = 0; i < width; i++)
            {
                if (i < startX)
                {
                    Slot.Add(new slots
                    {
                        positionX = startX + i,
                        positionY = positionY,
                        identity = 0,
                        AiCommands = battleCommands
                    });
                }
                else if (i >= startX && i < (width - startX))
                {
                    Slot.Add(new slots
                    {
                        positionX = startX + i,
                        positionY = positionY,
                        identity = identity,
                        AiCommands = battleCommands
                    });
                }
                if (i >= (width - startX))
                {
                    Slot.Add(new slots
                    {
                        positionX = startX + i,
                        positionY = positionY,
                        identity = 0,
                        AiCommands = battleCommands
                    });
                }
            }
            Debug.Log(positionY);
            positionY++;
        }
    }

    public void useStrategy()
    {
        int blue = gameManagerScript.numberOfBlue;
        int red = gameManagerScript.numberOfRed;

        float tolerance = 0.4f; // 50 %

        int aiformations = 0;

        float diff = blue - red;

        if (red != 0 && !forceDefend && ((diff >= 0 && diff <= red * tolerance) || (diff < 0 && Mathf.Abs(diff) <= blue * tolerance / 2f)))
        {
            //bridge defence
            GameObject[] movePointsL = GameObject.FindGameObjectsWithTag(AIblueBleftTag);
            GameObject[] movePointsR = GameObject.FindGameObjectsWithTag(AIblueBrightTag);
            GameObject[] movePoints = GameObject.FindGameObjectsWithTag(AImoveBlueATK);
            GameObject[] redBuildings = GameObject.FindGameObjectsWithTag(RedBuildingTag);
            int index = 0;
            List<GameObject> existingMovePoints = new List<GameObject>();

            foreach (GameObject building in redBuildings)
            {
                MoneyBuldingScript mon = building.GetComponent<MoneyBuldingScript>();
                if (mon != null)
                {
                    if (mon.destroyed)
                    {
                        
                    }
                    else
                    {
                        float closestDistance = float.MaxValue;
                        GameObject closestMovePoint = null;

                        Vector3 buildingPos = building.transform.position;
                        foreach (GameObject movePoint in movePoints)
                        {
                            float dist = Vector3.Distance(buildingPos, movePoint.transform.position);

                            if (dist < closestDistance)
                            {
                                closestDistance = dist;
                                closestMovePoint = movePoint;
                            }
                        }
                        if (closestMovePoint != null)
                        {
                            existingMovePoints.Add(closestMovePoint);
                            //Debug.Log(closestMovePoint + " " + building);
                        }
                    }
                }
            }
            foreach (var aifor in aiFormations)
            {
                if (gameManagerScript.AIblueMoney >= 100 && existingMovePoints.Count > 0 && aiformations < aiFormations.Count - 1)
                {
                    aifor.formation.transform.position = existingMovePoints[index].transform.position;
                    aifor.formation.transform.rotation = existingMovePoints[index].transform.rotation;
                    aifor.script.Charge = false;
                    aifor.script.Engage = false;
                    aifor.script.hold = false;

                    aiformations++;
                    index = (index + 1) % existingMovePoints.Count;
                }
                else if(aiformations < aiFormations.Count - 1)
                {
                    if (index % 2 == 0)
                    {
                        aifor.formation.transform.position = movePointsL[index].transform.position;
                        aifor.formation.transform.rotation = movePointsL[index].transform.rotation;
                        aifor.script.Charge = false;
                    }
                    else
                    {
                        aifor.formation.transform.position = movePointsR[index].transform.position;
                        aifor.formation.transform.rotation = movePointsR[index].transform.rotation;
                        aifor.script.Charge = false;
                    }


                    int i = UnityEngine.Random.Range(0, 2);
                    if (i == 0)
                    {
                        aifor.script.hold = false;
                    }
                    else
                    {
                        aifor.script.hold = true;
                    }
                    aifor.script.Engage = false;
                    index++;
                    if (index == (movePointsL.Length - 1) + (movePointsR.Length - 1))
                    {
                        index = 0;
                    }
                }
                else
                {
                    GameObject[] movePointsDef = GameObject.FindGameObjectsWithTag(AIblueDEFtag);
                    int ind = 0;
                    foreach (var aiFor in aiFormations)
                    {
                        aiFor.formation.transform.position = movePoints[ind].transform.position;
                        aiFor.formation.transform.rotation = movePoints[ind].transform.rotation;
                        aiFor.script.Charge = false;

                        int i = UnityEngine.Random.Range(0, 2);
                        if (i == 0)
                        {
                            aiFor.script.hold = false;
                        }
                        else
                        {
                            aiFor.script.hold = true;
                        }
                        aiFor.script.Engage = false;
                        ind++;
                        if (ind == movePointsDef.Length - 1)
                        {
                            ind = 0;
                        }
                    }
                }
            }
        }
        else if (blue > red || red == 0 && !forceDefend)
        {
            //charge
            GameObject[] movePoints = GameObject.FindGameObjectsWithTag(AImoveBlueATK);
            GameObject[] redBuildings = GameObject.FindGameObjectsWithTag(RedBuildingTag);
            int index = 0;
            int aiformationsindex = 0;
            List<GameObject> existingMovePoints = new List<GameObject>();

            foreach (GameObject building in redBuildings)
            {
                MoneyBuldingScript mon = building.GetComponent<MoneyBuldingScript>();
                if (mon != null)
                {
                    if (mon.destroyed)
                    {

                    }
                    else
                    {
                        float closestDistance = float.MaxValue;
                        GameObject closestMovePoint = null;

                        Vector3 buildingPos = building.transform.position;
                        foreach (GameObject movePoint in movePoints)
                        {
                            float dist = Vector3.Distance(buildingPos, movePoint.transform.position);

                            if (dist < closestDistance)
                            {
                                closestDistance = dist;
                                closestMovePoint = movePoint;
                            }
                        }
                        if (closestMovePoint != null)
                        {
                            existingMovePoints.Add(closestMovePoint);
                            Debug.Log(closestMovePoint + " " + building);
                        }
                    }
                }
            }
            if (existingMovePoints.Count > 0)
            {
                foreach (var aifor in aiFormations)
                {
                    if(aiformationsindex < aiFormations.Count - 1)
                    {
                        aifor.formation.transform.position = existingMovePoints[index].transform.position;
                        aifor.formation.transform.rotation = existingMovePoints[index].transform.rotation;
                        aifor.script.Charge = false;
                        aifor.script.Engage = false;
                        aifor.script.hold = false;

                        index = (index + 1) % existingMovePoints.Count;
                    }
                    else
                    {
                        GameObject[] movePointsDef = GameObject.FindGameObjectsWithTag(AIblueDEFtag);
                        int ind = 0;
                        foreach (var aiFor in aiFormations)
                        {
                            aiFor.formation.transform.position = movePoints[ind].transform.position;
                            aiFor.formation.transform.rotation = movePoints[ind].transform.rotation;
                            aiFor.script.Charge = false;

                            int i = UnityEngine.Random.Range(0, 2);
                            if (i == 0)
                            {
                                aiFor.script.hold = false;
                            }
                            else
                            {
                                aiFor.script.hold = true;
                            }
                            aiFor.script.Engage = false;
                            ind++;
                            if (ind == movePointsDef.Length - 1)
                            {
                                ind = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var aifor in aiFormations)
                {
                    if(aiformationsindex < aiFormations.Count - 1)
                    {
                        aifor.script.Charge = true;
                        aifor.script.hold = false;
                        aifor.script.Engage = false;
                    }
                    else
                    {
                        GameObject[] movePointsDef = GameObject.FindGameObjectsWithTag(AIblueDEFtag);
                        int ind = 0;
                        foreach (var aiFor in aiFormations)
                        {
                            aiFor.formation.transform.position = movePoints[ind].transform.position;
                            aiFor.formation.transform.rotation = movePoints[ind].transform.rotation;
                            aiFor.script.Charge = false;

                            int i = UnityEngine.Random.Range(0, 2);
                            if (i == 0)
                            {
                                aiFor.script.hold = false;
                            }
                            else
                            {
                                aiFor.script.hold = true;
                            }
                            aiFor.script.Engage = false;
                            ind++;
                            if (ind == movePointsDef.Length - 1)
                            {
                                ind = 0;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            //defend

            if (forceDefend)
            {
                nextIsFormations = true;
            }
            GameObject[] movePoints = GameObject.FindGameObjectsWithTag(AIblueDEFtag);
            int index = 0;
            foreach (var aifor in aiFormations)
            {
                aifor.formation.transform.position = movePoints[index].transform.position;
                aifor.formation.transform.rotation = movePoints[index].transform.rotation;
                aifor.script.Charge = false;
                
                int i = UnityEngine.Random.Range(0, 2);
                if (i == 0)
                {
                    aifor.script.hold = false;
                }
                else
                {
                    aifor.script.hold = true;
                }
                aifor.script.Engage = false;
                index++;
                if (index == movePoints.Length - 1)
                {
                    index = 0;
                }
            }
            stratIndex++;
        }


    }
}

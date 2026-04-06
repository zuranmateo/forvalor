using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UIplayerController;

public class UIplayerController : MonoBehaviour
{
    public bool SpawnTroopsView = false;

    public TextMeshProUGUI UnitsText;
    public TextMeshProUGUI RowsText;

    float timer = 0;
    [Header("tags")]
    public string SlotsTag = "SpawningSlots";
    public string SpawningImagePrefabsTag = "SpawningImagePrefabs";
    public string SpawnerAllyTag = "SpawnPointRed";
    public string AllyFormationTag = "formationRed";
    public string EnemyFormationTag = "formationBlue";
    public string RedDoorTag = "doorRed";
    public string throneAllyTag = "throneRed";

    [Header("spawner")]
    public float defaultXlocationValue = -380f;

    public float NumberOfUnits;
    public float NumberOfRows;

    public GameObject SlotsParent;
    public GameObject SlotPrefab;
    public GameObject CostOfFormantionText;
    private TextMeshProUGUI CostOfFormationTextUGUI;
    public TextMeshProUGUI CostOfIronTextUGUI;

    public GameObject SpawningPanel;
    public GameObject SpawnerPanel;

    public GameObject Spawner;
    public GameObject FormationPrefab;

    [Header("costs")]
    private float fullcost = 0;
    private float ironCost = 0;
    public float SpearmanCost = 14;
    public float BowmanCost = 18;
    public float swordmanCost = 20;

    [Header("spawner buttons")]
    public GameObject FormationButton;
    public GameObject CavalryButton;
    public GameObject SupportsButton;
    public GameObject DefenceButton;

    [Header("spawner prefabs")]
    public GameObject SpearmanInventory;
    public GameObject BowmanInventory;
    public GameObject SwordmanInventory;

    [Header("Formations list")]
    public GameObject FormationListComponentPrefab;
    public GameObject FormationList;
    public GameObject FormationaComponentButton;
    public GameObject BattleComands;
    public int orderNumber = 0;
    public int FormationListXPosition = 25;
    public float formationSelectPosition = 50;
    public float formationDeselectPosition = 0;
    public GameObject SelectedFormation;
    public float XancorLocation = -100f;

    [Header("repair gates")]
    public TextMeshProUGUI GatesBrokenText;
    public GameObject RepairGatesButton;
    public GameObject DefencePanel;

    [Header("upgrades")]
    public int skill = 1;
    public int movespeed = 1;
    public int armor = 1;
    public GameObject UpgradesPanel;
    public GameObject skillButton;
    public GameObject armorButton;
    public GameObject movespeedButton;
    public TextMeshProUGUI skillCostLabel;
    public TextMeshProUGUI armorCostLabel;
    public TextMeshProUGUI movespeedCostLabel;
    public TextMeshProUGUI skillLabel;
    public TextMeshProUGUI armorLabel;
    public TextMeshProUGUI movespeedLabel;

    [Header("info panel")]
    public TextMeshProUGUI gateHealthLabel;
    public TextMeshProUGUI throneHealthLabel;
    public TextMeshProUGUI moneyLabel;
    public TextMeshProUGUI IronLabel;

    [Header("sounds")]
    public AudioSource AudioSource;
    public AudioClip swordSound;

    [Header("external objects and scripts")]
    public GameObject commandsPanel;
    public GameManagerScript gamemanager;

    public class slots
    {
        public int positionX;
        public int positionY;
        public int identity;
        public GameObject BattleCommands;
        public GameObject AiCommands;
    }
    public class formations
    {
        public int order;
        public float positionX;
        public GameObject GameObject;
        public GameObject UIobject;
        public GameObject Button;
        public GameObject BattleCommands;
    }
    public List<slots> Slot = new List<slots>();
    public List<formations> Formations = new List<formations>();
    // Start is called before the first frame update
    void Start()
    {
        gamemanager = FindFirstObjectByType<GameManagerScript>();
        CostOfFormationTextUGUI = CostOfFormantionText.GetComponent<TextMeshProUGUI>();
        AudioSource = GetComponent<AudioSource>();
        SpearmanCost = gamemanager.spearmanCost;
        BowmanCost = gamemanager.bowmanCost;
        swordmanCost = gamemanager.swordmanCost;
        SpawningPanel.SetActive(false);
        SpawnerPanel.SetActive(false);
        UpgradesPanel.SetActive(false);
        DefencePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            CloseSpawningFunction();
        }

        timer += Time.deltaTime;

        if (timer > 0.3f)
        {
            if (SpawnerPanel.activeInHierarchy)
            {
                FormationCost();
            }

            displayInfo();
        }
        
    }

    public void ToggleSpawnTroopsView()
    {
        if (SpawnTroopsView)
        {
            Slot.Clear();
            SpawnTroopsView = false;
            CostOfFormationTextUGUI.text = "cost: 0";
            SpawningPanel.SetActive(false);
            DefencePanel.SetActive(false);
            UpgradesPanel.SetActive(false);
            SpawnerPanel.SetActive(false);
        }
        else
        {
            SpawnTroopsView = true;
            DefencePanel.SetActive(false);
            UpgradesPanel.SetActive(false);
            SpawningPanel.SetActive(true);
        }
    }
    public void OpenSpawnerPanelWith(GameObject sender)
    {
        SpawningPanel.SetActive(false);
        SpawnerPanel.SetActive(true);
        //Debug.Log(sender.name);
        if (sender == FormationButton)
        {
            SpawnInventoryPrefab(XancorLocation, SpearmanInventory);
            SpawnInventoryPrefab(XancorLocation + 50f, BowmanInventory);
            SpawnInventoryPrefab(XancorLocation + 100f, SwordmanInventory);
            //isto klices funkcijo samo spearmaninventory zamenjas z drugim objektom
            XancorLocation = 0;
        }
    }
    public void SpawnInventoryPrefab(float XancorLocation, GameObject InventoryPrefab)
    {
        GameObject clone = Instantiate(InventoryPrefab, SpawnerPanel.transform.parent);
        RectTransform rectTransform = clone.GetComponent<RectTransform>();
        //rectTransform.anchoredPosition = new Vector3(XancorLocation, 100f, 0);
        clone.name = InventoryPrefab.name;
    }

    public void FormationCost()
    {
        int numberOfSpearmans = 0;
        int numberOfBowmans = 0;
        int numberOfswordmans = 0;
        Transform spawning = transform.Find("Spawning");
        if (spawning != null)
        {
            foreach (Transform child in spawning)
            {
                if (child.name == "SpearmanInventory")
                {
                    numberOfSpearmans++;
                }
                if(child.name == "BowmanInventory")
                {
                    numberOfBowmans++;
                }
                if (child.name == "SwordmanInventory")
                {
                    numberOfswordmans++;
                }
            }
            if(numberOfSpearmans > 0)
            {
                numberOfSpearmans--;
            }
            if(numberOfBowmans > 0)
            {
                numberOfBowmans--;
            }
            if(numberOfswordmans > 0)
            {
                numberOfswordmans--;
            }
        }
        fullcost = (numberOfSpearmans * SpearmanCost) + (numberOfBowmans*BowmanCost) + (numberOfswordmans*swordmanCost);//+(numberOfSwordmans*SwordmanCost)
        ironCost = numberOfBowmans + numberOfSpearmans + (numberOfswordmans * 2);
        CostOfFormationTextUGUI.text = fullcost.ToString();
        CostOfIronTextUGUI.text = ironCost.ToString();
    }
    public void ApplyAndCreateFormation()
    {
        if(fullcost <= gamemanager.playerMoney && gamemanager.maxRedTroops >= gamemanager.numberOfRed && gamemanager.playerIron >= ironCost)
        {
            gamemanager.playerMoney -= fullcost;
            gamemanager.playerIron -= ironCost;
            int locationX = 0;
            int locationY = 0;
            GameObject[] objekti = GameObject.FindGameObjectsWithTag(SlotsTag);

            //Spawn battle commands for the formation
            GameObject clone = Instantiate(BattleComands, commandsPanel.transform);
            clone.name = BattleComands.name + orderNumber.ToString();

            foreach (GameObject obj in objekti)
            {
                ItemSlot itemslot = obj.GetComponent<ItemSlot>();

                Slot.Add(new slots
                {
                    positionX = locationX,
                    positionY = locationY,
                    identity = itemslot.identity,
                    BattleCommands = clone
                });
                locationX++;
                if (locationX % 14 == 0)
                {
                    locationY++;
                }
            }
            Spawner = GameObject.FindGameObjectWithTag(SpawnerAllyTag);
            GameObject formation = Instantiate(FormationPrefab);
            formation.transform.position = Spawner.transform.position;
            formation.transform.rotation = Spawner.transform.rotation;

            formation.name = FormationPrefab.name + orderNumber.ToString();

            orderNumber++;
            FormationController formationController = formation.GetComponent<FormationController>();


            //zapis vseh podatkov v list formacij
            Formations.Add(new formations
            {
                order = orderNumber,
                GameObject = formation,
                BattleCommands = clone
            });

            formationController.UIbattle = clone;
            formationController.Spawner = Spawner;
            formationController.skill = skill;
            formationController.movespeed = movespeed;
            formationController.armor = armor;

            //zapiši formation v battle controller
            UIbattleController UIbattle = clone.GetComponent<UIbattleController>();
            UIbattle.formation = formation;
            UIbattle.UIplayer = gameObject;

            clone.SetActive(false);

            formationController.GenerateFormation(Slot);
        }
        
        WriteAndOrderFormationsUI();
        CloseSpawningFunction();       //tu se klièe fnkcija da se vse resetira
    }

    public void CloseSpawningFunction()
    {
        GameObject[] SoliderImages = GameObject.FindGameObjectsWithTag(SpawningImagePrefabsTag);
        GameObject[] objekti = GameObject.FindGameObjectsWithTag(SlotsTag);
        Slot.Clear();
        SpawnTroopsView = false;
        SpawningPanel.SetActive(false);
        SpawnerPanel.SetActive(false);
        UpgradesPanel.SetActive(false);
        DefencePanel.SetActive(false);
        
        foreach (GameObject obj in SoliderImages)
        {
            DragAndDrop draganddrop = obj.GetComponent<DragAndDrop>();
            Destroy(obj);
        }
        foreach (GameObject obj in objekti)
        {
            ItemSlot itemslot = obj.GetComponent<ItemSlot>();
            itemslot.identity = 0;
        }
    }

    public void WriteAndOrderFormationsUI()
    {
        DeleteFormationsUI();
        float x = FormationListXPosition;
        foreach (var obj in Formations)
        {
            GameObject clone = Instantiate(FormationListComponentPrefab, FormationList.transform);
            RectTransform rect = clone.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, formationDeselectPosition);
            
            obj.UIobject = clone;
            obj.Button = clone.transform.Find("Button").gameObject;

            obj.Button.GetComponent<Button>().onClick.AddListener(() => SelectFormation(clone));

            obj.positionX = x;

            clone.name = FormationListComponentPrefab.name + obj.order.ToString();
            x += 52;

            Transform child1 = clone.transform.GetChild(0); // 0 = prvi child
            GameObject childObject1 = child1.gameObject;

            Transform child2 = childObject1.transform.GetChild(0); // 0 = prvi child
            GameObject childObject2 = child2.gameObject;

            Transform child = childObject2.transform.GetChild(0); // 0 = prvi child
            GameObject childObject = child.gameObject;

            //Debug.Log(child);

            TextMeshProUGUI TextMesh = childObject.GetComponent<TextMeshProUGUI>();

            //Debug.Log(TextMesh);
            TextMesh.text = obj.order.ToString();
        }
    }

    public void SelectFormation(GameObject sender)
    {
        foreach (var obj in Formations)
        {
            //pridobi tagger
            Transform taggerTransform = obj.GameObject.transform.GetChild(0);
            GameObject tagger = taggerTransform.gameObject;

            RectTransform rect = obj.UIobject.GetComponent<RectTransform>();
            if (sender == obj.UIobject && SelectedFormation != obj.UIobject)
            {
                tagger.SetActive(true);
                obj.BattleCommands.SetActive(true);
                rect.anchoredPosition = new Vector2(obj.positionX, formationSelectPosition);
                SelectedFormation = obj.UIobject;
                AudioSource.PlayOneShot(swordSound);
            }
            else if(sender == obj.UIobject && SelectedFormation == obj.UIobject)
            {
                SelectedFormation = null;
                rect.anchoredPosition = new Vector2(obj.positionX, formationDeselectPosition);
                obj.BattleCommands.SetActive(false);
                tagger.SetActive(false);
                AudioSource.PlayOneShot(swordSound);
            }
            else
            {
                rect.anchoredPosition = new Vector2(obj.positionX, formationDeselectPosition);
                obj.BattleCommands.SetActive(false);
                tagger.SetActive(false);
                AudioSource.PlayOneShot(swordSound);
            }
            //Debug.Log(SelectedFormation);
        }
    }

    public void DeleteFormationsUI()
    {
        foreach (Transform child in FormationList.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void RepairGatesRed()
    {
        doorScript doorScript = GameObject.FindGameObjectWithTag(RedDoorTag).transform.parent.GetComponent<doorScript>();

        if (doorScript != null && doorScript.destroyed)
        {
            if (gamemanager.gatesRepairCost <= gamemanager.AIblueMoney)
            {

                gamemanager.AIblueMoney -= gamemanager.gatesRepairCost;
                doorScript.repairGate();
            }
        }
    }
    public void OpenDefencePanel()
    {
        SpawningPanel.SetActive(false);
        SpawnerPanel.SetActive(false);
        UpgradesPanel.SetActive(false);
        DefencePanel.SetActive(true);
        doorScript doorScript = GameObject.FindGameObjectWithTag(RedDoorTag).transform.parent.GetComponent<doorScript>();

        if (doorScript != null && doorScript.destroyed)
        {
            GatesBrokenText.text = "Gates are: BROKEN";
            GatesBrokenText.color = Color.red;
        }
        else
        {
            GatesBrokenText.text = "Gates are: OK";
            GatesBrokenText.color = Color.green;
        }
    }

    public void OpenUpgradesPanel()
    {
        UpgradesPanel.SetActive(true);
        SpawningPanel.SetActive(false);
        skillCostLabel.text = (gamemanager.defaultUpgradeCost * skill).ToString() + "$";
        movespeedCostLabel.text = (gamemanager.defaultUpgradeCost * movespeed).ToString() + "$";
        armorCostLabel.text = (gamemanager.defaultUpgradeCost * armor).ToString() + "$";
        skillLabel.text = skill.ToString();
        movespeedLabel.text = movespeed.ToString();
        armorLabel.text = armor.ToString();

    }

    public void upgradeButtonPressed(GameObject sender)
    {
        if (sender == null) return;

        if (sender == skillButton)
        {
            if(gamemanager.playerMoney > gamemanager.defaultUpgradeCost + skill)
            {
                gamemanager.playerMoney -= gamemanager.defaultUpgradeCost * skill;
                skill++;
            }
        }
        else if (sender == movespeedButton)
        {
            if (gamemanager.playerMoney > gamemanager.defaultUpgradeCost * movespeed)
            {
                gamemanager.playerMoney -= gamemanager.defaultUpgradeCost * movespeed;
                movespeed++;
            }
        }
        else if (sender == armorButton)
        {
            if (gamemanager.playerMoney > gamemanager.defaultUpgradeCost * armor)
            {
                gamemanager.playerMoney -= gamemanager.defaultUpgradeCost * armor;
                armor++;
            }
        }

        foreach(var f in Formations)
        {
            FormationController fc = f.GameObject.GetComponent<FormationController>();
            if (fc != null)
            {
                fc.skill = skill;
                fc.movespeed = movespeed;
                fc.armor = armor;
                fc.upgradeSoliders();
            }
        }

        OpenUpgradesPanel();
    }

    public void displayInfo()
    {
        IronLabel.text = gamemanager.playerIron.ToString("F1");
        moneyLabel.text = gamemanager.playerMoney.ToString("F1");


        doorScript doorScript = GameObject.FindGameObjectWithTag(RedDoorTag).transform.parent.GetComponent<doorScript>();
        if (doorScript != null)
        {
            gateHealthLabel.text = doorScript.health.ToString();
        }

        throneScript throne = GameObject.FindGameObjectWithTag(throneAllyTag).GetComponent<throneScript>();
        if(throne != null)
        {
            throneHealthLabel.text = throne.health.ToString();
        }
    }
}


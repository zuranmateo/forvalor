using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using static UIplayerController;
using TMPro;

public class UIbattleController : MonoBehaviour
{
    public GameObject ChargeButton;
    public GameObject HoldButton;
    public GameObject EngageButton;
    public GameObject ReinforceButton;
    public GameObject DeleteButton;
    //public string activeColor = "255f/255f, 255f/255f, 255f/255f, 50f/255f";
    public Color ActiveColor = new Color(255f / 255f, 255f / 255f, 0/ 255f, 50f / 100f);
    public Color DisabledColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 50f / 255f);


    public bool Charge = false;
    public bool hold = false;
    public bool Engage = false;

    public bool reinforcePanelActive = false;
    public bool deletePanelActive = false;
    public bool alive = true;

    [Header("moveing formation")]
    [SerializeField] GameObject moveMarker;
    private GameObject moveMarkerclone;
    [SerializeField] LayerMask clickableLayers;
    CustomActions input;
    private Vector3 moveToPosition;
    private Vector3 rotateToPosition;
    private bool holdingButton;
    private GameManagerScript gamemanager;

    public GameObject UIplayer;
    public GameObject formation;

    public GameObject deletePanel;
    public GameObject reinforcePanel;
    public TextMeshProUGUI costText;

    // Start is called before the first frame update
    private void Awake()
    {
        input = new CustomActions();
        gamemanager = FindFirstObjectByType<GameManagerScript>();
        assignInputs();
        deletePanel.SetActive(false);
        reinforcePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (holdingButton)
        {
            if (moveMarkerclone != null)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                //Debug.Log("pressed");
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
                {
                    moveMarkerclone.transform.LookAt(hit.point);
                    moveMarkerclone.transform.eulerAngles = new Vector3(0f, moveMarkerclone.transform.eulerAngles.y, moveMarkerclone.transform.eulerAngles.z);
                    rotateToPosition = hit.point;
                }
            }
        }
    }

    public void SetCharge()
    {
        if (Charge)
        {
            Charge = false;
            ChargeButton.GetComponent<Image>().color = DisabledColor;
            //Debug.Log("off");
        }
        else
        {
            Charge = true;
            hold = false;
            Engage = false;
            ChargeButton.GetComponent<Image>().color = ActiveColor;
            HoldButton.GetComponent<Image>().color = DisabledColor;
            EngageButton.GetComponent<Image>().color = DisabledColor;
            //Debug.Log("on");
        }
    }

    public void SetHold()
    {
        if (hold)
        {
            hold = false;
            HoldButton.GetComponent<Image>().color = DisabledColor;
            //Debug.Log("off");
        }
        else
        {
            hold = true;
            Charge = false;
            Engage = false;
            HoldButton.GetComponent<Image>().color = ActiveColor;
            ChargeButton.GetComponent<Image>().color = DisabledColor;
            EngageButton.GetComponent<Image>().color = DisabledColor;
            //Debug.Log("on");
        }
    }

    public void SetEngage()
    {
        if (Engage)
        {
            Engage = false;
            EngageButton.GetComponent<Image>().color = DisabledColor;
            //Debug.Log("off");
        }
        else
        {
            Engage = true;
            Charge = false;
            hold = false;
            EngageButton.GetComponent<Image>().color = ActiveColor;
            ChargeButton.GetComponent<Image>().color = DisabledColor;
            HoldButton.GetComponent<Image>().color = DisabledColor;
            //Debug.Log("on");
        }
    }

    public void SetReinforce()
    {
        if (reinforcePanelActive)
        {
            reinforcePanelActive = false;
            reinforcePanel.SetActive(false);
        }
        else
        {
            float fullcost = 0;
            FormationController formationController = formation.GetComponent<FormationController>();
            if (formationController != null)
            {
                foreach (var slot in formationController.formationslots)
                {
                    if (slot != null && slot.alive == false)
                    {
                        switch (slot.identity)
                        {
                            case 1:
                                fullcost += gamemanager.spearmanCost;
                                break;
                            case 2:
                                fullcost += gamemanager.bowmanCost;
                                break;
                            case 3:
                                fullcost += gamemanager.swordmanCost;
                                break;
                        }
                    }
                }
                costText.text = fullcost.ToString() + "€";
            }
            reinforcePanelActive = true;
            reinforcePanel.SetActive(true);
        }
    }

    public void SetDeletePanel()
    {
        if (deletePanelActive)
        {
            deletePanelActive = false;
            deletePanel.SetActive(false);
        }
        else
        {
            deletePanelActive = true;
            deletePanel.SetActive(true);
        }
    }
    public void SetAgresiveMode()
    {

    }


    public void assignInputs()
    {
        input.LeftClick.move.started += ctx => ClickMove();
        input.LeftClick.move.canceled += ctx => CancelMove();
    }

    public void ClickMove()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        //Debug.Log("pressed");
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 100, clickableLayers))
        {
            //Debug.Log("triggered and should move");
            //formation.transform.position = hit.point;
            if(moveMarker != null)
            {
                Quaternion q;
                if (rotateToPosition != null)
                {
                    q = Quaternion.Euler(rotateToPosition);
                }
                else
                {
                    q = moveMarker.transform.rotation;
                }
                moveMarkerclone = Instantiate(moveMarker, hit.point += new Vector3(0, 0.1f, 0), q);
                moveMarkerclone.transform.position = hit.point;
                moveToPosition = hit.point;
                holdingButton = true;
            }
        }
    }
    public void CancelMove()
    {
        if (moveMarkerclone != null)
        {
            formation.transform.position = moveToPosition;
            formation.transform.LookAt(rotateToPosition);
            Destroy(moveMarkerclone);
            holdingButton = false;
        }
    }

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }




    public void reinforceFormation()
    {
        float fullcost = 0;
        int ironcost = 0;
        FormationController formationController = formation.GetComponent<FormationController>();
        if (formationController != null)
        {
            foreach(var slot in formationController.formationslots)
            {
                if (slot != null && slot.alive == false)
                {
                    switch (slot.identity)
                    {
                        case 1:
                            fullcost += gamemanager.spearmanCost;
                            ironcost++;
                        break;
                        case 2:
                            fullcost += gamemanager.bowmanCost;
                            ironcost++;
                        break;
                        case 3:
                            fullcost += gamemanager.swordmanCost;
                            ironcost += 2; 
                        break;
                    }
                }
            }

            if (fullcost < gamemanager.playerMoney && gamemanager.playerIron >= ironcost)
            {
                gamemanager.playerMoney -= fullcost;
                gamemanager.playerIron -= ironcost;
                formationController.reinforce();
                reinforcePanelActive = false;
                reinforcePanel.SetActive(false);
                deletePanelActive = false;
                deletePanel.SetActive(false);
            }
            
        }
        
    }


    public void KillFormation()
    {
        alive = false;
        Invoke(nameof(DeleteFormation), 1f);
    }

    public void DeleteFormation()
    {
        FormationController formationController = formation.GetComponent<FormationController>();
        if (formationController != null)
        {
            foreach (var slot in formationController.formationslots)
            {
                Destroy(slot.Solider);
            }
            Destroy(formation);

            UIplayerController uiplayercontroller = UIplayer.GetComponent<UIplayerController>();
            if (uiplayercontroller != null)
            {
                Debug.Log(uiplayercontroller.Formations + " " + gameObject);
                var item = uiplayercontroller.Formations.Find(f => f.BattleCommands == gameObject);
                if (item != null)
                {
                    uiplayercontroller.Formations.Remove(item);
                    Debug.Log("delete");
                }

                uiplayercontroller.WriteAndOrderFormationsUI();
            }

            Destroy(gameObject);
        }
    }




    public void turnOffReinforcePanel()
    {
        deletePanelActive = false;
        reinforcePanelActive = false;
        reinforcePanel.SetActive(false);
        deletePanel.SetActive(false);
    }
}

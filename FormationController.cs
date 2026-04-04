using System.Collections.Generic;
using UnityEngine;
using static UIplayerController;

public class FormationController : MonoBehaviour
{
    [Header("Formacija")]
    //public int unitCount = 12;
    //public int maxRowWidth = 5;
    public float spacing = 1.5f;

    [Header("Vojaki")]
    public GameObject spearmanPrefab;
    public GameObject bowmanPrefab;
    public GameObject swordmanPrefab;
    private GameObject SoliderPrefab;


    [Header("upgrades")]
    public int skill = 1;
    public int movespeed = 1;
    public int armor = 1;


    [Header("BattleController")]
    public GameObject UIbattle;
    public GameObject AIbattle;

    public GameObject Spawner;
    public class FormationSlot
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public int identity;
        public UIbattleController UIbattle;
        public AIcontrollerBlue AIbattle;
        public GameObject Solider;
        public bool alive = true;
    }

    [HideInInspector]
    public List<FormationSlot> formationslots = new List<FormationSlot>();
    void Start()
    {

    }

    public void GenerateFormation(List<slots> slot)
    {
        formationslots.Clear();
        int currentRow = 0;
        int i = 0;

        float halfSpace = (spacing * 14)/2;

        foreach (slots s in slot)
        {
            Vector3 localPos = new Vector3((i * spacing) - halfSpace, 0f, currentRow * spacing);
            Quaternion localRot = Quaternion.identity;
            if (s.BattleCommands)
            {
                UIbattleController UIbattleC = s.BattleCommands.GetComponent<UIbattleController>();
                formationslots.Add(new FormationSlot
                {
                    localPosition = localPos,
                    localRotation = localRot,
                    identity = s.identity,
                    UIbattle = UIbattleC,
                    alive = true
                });
            }
            else if(s.AiCommands)
            {
                AIcontrollerBlue AiBattleC = s.AiCommands.GetComponent<AIcontrollerBlue>();
                formationslots.Add(new FormationSlot
                {
                    localPosition = localPos,
                    localRotation = localRot,
                    identity = s.identity,
                    AIbattle = AiBattleC,
                    alive = true
                });
            }

            i++;
            if (i%14 == 0)
            {
                currentRow++;
                i = 0;
            }
        }
        SpawnFormation();
    }

    public void writeSoliderAsDead(GameObject solider)
    {
        foreach (var slot in formationslots)
        {
            //Debug.Log(solider.name + slot.Solider);
            if (solider == slot.Solider)
            {
                slot.alive = false;
                Debug.Log("dead in formation" + slot.alive);
            }
        }
    }

    void SpawnFormation()
    {
        foreach (var slot in formationslots)
        {
            if (slot.identity != 0)
            {
                Vector3 worldPos = transform.TransformPoint(slot.localPosition);
                Quaternion worldRot = transform.rotation * slot.localRotation;

                switch (slot.identity)
                {
                    case 1:
                        SoliderPrefab = spearmanPrefab;
                        break;
                    case 2:
                        SoliderPrefab = bowmanPrefab;
                        break;
                    case 3:
                        SoliderPrefab = swordmanPrefab;
                        break;
                    default:
                        break;
                }

                if (SoliderPrefab != null)
                {
                    GameObject unit = Instantiate(SoliderPrefab, worldPos, worldRot);
                    if (SoliderPrefab == spearmanPrefab)
                    {
                        SpearmanScript SS = unit.GetComponent<SpearmanScript>();
                        if (SS != null)
                        {
                            if (slot.UIbattle)
                            {
                                SS.UIbattle = slot.UIbattle;
                            }
                            else if (slot.AIbattle)
                            {
                                SS.AI = slot.AIbattle;
                            }
                            SS.skill = skill;
                            SS.movespeed = movespeed;
                            SS.armor = armor;
                            SS.SetFormationHome(slot.localPosition, slot.localRotation, this);
                        }
                    }
                    else if (SoliderPrefab == bowmanPrefab)
                    {
                        BowmanScript SS = unit.GetComponent<BowmanScript>();
                        if (SS != null)
                        {
                            if (slot.UIbattle)
                            {
                                SS.UIbattle = slot.UIbattle;
                            }
                            else if (slot.AIbattle)
                            {
                                SS.AI = slot.AIbattle;
                            }
                            SS.skill = skill;
                            SS.movespeed = movespeed;
                            SS.armor = armor;
                            SS.SetFormationHome(slot.localPosition, slot.localRotation, this);
                        }
                    }
                    else if (SoliderPrefab == swordmanPrefab)
                    {
                        swordmanScript SS = unit.GetComponent<swordmanScript>();
                        if (SS != null)
                        {
                            if (slot.UIbattle)
                            {
                                SS.UIbattle = slot.UIbattle;
                            }
                            else if (slot.AIbattle)
                            {
                                SS.AI = slot.AIbattle;
                            }
                            SS.skill = skill;
                            SS.movespeed = movespeed;
                            SS.armor = armor;
                            SS.SetFormationHome(slot.localPosition, slot.localRotation, this);
                        }
                    }

                    slot.Solider = unit;
                }
            }
        }
    }

    public Vector3 GetWorldPositionFromLocal(Vector3 localPos)
    {
        return transform.TransformPoint(localPos);
    }

    public Quaternion GetWorldRotationFromLocal(Quaternion localRot)
    {
        return transform.rotation * localRot;
    }

    public void reinforce()
    {
        foreach (var slot in formationslots)
        {
            if (slot.identity != 0 && slot.alive == false)
            {
                Vector3 worldPos = transform.TransformPoint(slot.localPosition);
                Quaternion worldRot = transform.rotation * slot.localRotation;

                switch (slot.identity)
                {
                    case 1:
                        SoliderPrefab = spearmanPrefab;
                        break;
                    case 2:
                        SoliderPrefab = bowmanPrefab;
                        break;
                    case 3:
                        SoliderPrefab = swordmanPrefab;
                        break;
                    default:
                        break;
                }

                if (SoliderPrefab != null)
                {
                    GameObject unit = Instantiate(SoliderPrefab, Spawner.transform.position, Spawner.transform.rotation);
                    if (SoliderPrefab == spearmanPrefab)
                    {
                        SpearmanScript SS = unit.GetComponent<SpearmanScript>();
                        if (SS != null)
                        {
                            if (slot.UIbattle)
                            {
                                SS.UIbattle = slot.UIbattle;
                            }
                            else if (slot.AIbattle)
                            {
                                SS.AI = slot.AIbattle;
                            }
                            slot.alive = true;
                            SS.skill = skill;
                            SS.movespeed = movespeed;
                            SS.armor = armor;
                            SS.SetFormationHome(slot.localPosition, slot.localRotation, this);
                        }
                    }
                    else if (SoliderPrefab == bowmanPrefab)
                    {
                        BowmanScript SS = unit.GetComponent<BowmanScript>();
                        if (SS != null)
                        {
                            if (slot.UIbattle)
                            {
                                SS.UIbattle = slot.UIbattle;
                            }
                            else if (slot.AIbattle)
                            {
                                SS.AI = slot.AIbattle;
                            }
                            slot.alive = true;
                            SS.skill = skill;
                            SS.movespeed = movespeed;
                            SS.armor = armor;
                            SS.SetFormationHome(slot.localPosition, slot.localRotation, this);
                        }
                    }
                    else if (SoliderPrefab == swordmanPrefab)
                    {
                        swordmanScript SS = unit.GetComponent<swordmanScript>();
                        if (SS != null)
                        {
                            if (slot.UIbattle)
                            {
                                SS.UIbattle = slot.UIbattle;
                            }
                            else if (slot.AIbattle)
                            {
                                SS.AI = slot.AIbattle;
                            }
                            slot.alive = true;
                            SS.skill = skill;
                            SS.movespeed = movespeed;
                            SS.armor = armor;
                            SS.SetFormationHome(slot.localPosition, slot.localRotation, this);
                        }
                    }

                    slot.Solider = unit;
                }
            }
        }
    }

    public void upgradeSoliders()
    {
        foreach (var slot in formationslots)
        {
            if (slot.identity == 1 && slot.alive)
            {
                SpearmanScript SS = slot.Solider.GetComponent<SpearmanScript>();
                if (SS != null)
                {
                    SS.skill = skill;
                    SS.movespeed = movespeed;
                    SS.armor = armor;
                }
            }
            else if (slot.identity == 2 && slot.alive)
            {
                BowmanScript SS = slot.Solider.GetComponent<BowmanScript>();
                if (SS != null)
                {
                    SS.skill = skill;
                    SS.movespeed = movespeed;
                    SS.armor = armor;
                }
            }
            else if (slot.identity == 3 && slot.alive)
            {
                swordmanScript SS = slot.Solider.GetComponent<swordmanScript>();
                if (SS != null)
                {
                    SS.skill = skill;
                    SS.movespeed = movespeed;
                    SS.armor = armor;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearTipDamage : MonoBehaviour
{
    public string speartag;
    public string spearmanEnemyTag;
    public string spearmanAllyTag;
    public string bowmanAllyTag = "BowManRed";
    public string bowmanEnemyTag = "BowManBlue";
    public string swordmanAllyTag = "SwordManRed";
    public string swordmanEnemyTag = "SwordManBlue";
    public string DeadLayer = "dead";
    public string AllyDoorTag = "doorRed";
    public string EnemyDoorTag = "doorBlue";
    public string throneAllyTag = "throneRed";
    public string throneEnemyTag = "throneBlue";
    public string AllyBuildingTag = "RedBuilding";
    public string EnemyBuildingTag = "BlueBuilding";

    public GameObject AttackedEnemy;
    public GameObject Parent;
    public SpearmanScript spearman;
    public swordmanScript swordman;
    // Start is called before the first frame update
    void Start()
    {
        if (Parent.CompareTag(spearmanAllyTag))
        {
            spearman = Parent.GetComponent<SpearmanScript>();
        }
        else if (Parent.CompareTag(swordmanAllyTag))
        {
            swordman = Parent.GetComponent<swordmanScript>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (spearman != null)
        {
            if ((other.CompareTag(spearmanEnemyTag) || other.CompareTag(bowmanEnemyTag) || other.CompareTag(EnemyDoorTag) || other.CompareTag(swordmanEnemyTag) || other.CompareTag(throneEnemyTag) || other.CompareTag(EnemyBuildingTag)) && CompareTag(speartag) && spearman.HeAttacked)
            {
                AttackedEnemy = other.gameObject;
            }
        }
        else
        {
            if ((other.CompareTag(spearmanEnemyTag) || other.CompareTag(bowmanEnemyTag) || other.CompareTag(EnemyDoorTag) || other.CompareTag(swordmanEnemyTag) || other.CompareTag(throneEnemyTag) || other.CompareTag(EnemyBuildingTag)) && CompareTag(speartag) && swordman.HeAttacked)
            {
                AttackedEnemy = other.gameObject;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (spearman != null)
        {
            if ((other.CompareTag(spearmanEnemyTag) || other.CompareTag(bowmanEnemyTag) || other.CompareTag(EnemyDoorTag) || other.CompareTag(swordmanEnemyTag) || other.CompareTag(throneEnemyTag) || other.CompareTag(EnemyBuildingTag)) && CompareTag(speartag) && spearman.HeAttacked)
            {
                AttackedEnemy = other.gameObject;
            }
        }
        else
        {
            if ((other.CompareTag(spearmanEnemyTag) || other.CompareTag(bowmanEnemyTag) || other.CompareTag(EnemyDoorTag) || other.CompareTag(swordmanEnemyTag) || other.CompareTag(throneEnemyTag) || other.CompareTag(EnemyBuildingTag)) && CompareTag(speartag) && swordman.HeAttacked)
            {
                AttackedEnemy = other.gameObject;
            }
        }
    }
}

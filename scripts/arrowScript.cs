using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowScript : MonoBehaviour
{
    public string spearmanEnemyTag;
    public string spearmanAllyTag;
    public string bowmanAllyTag = "BowManRed";
    public string bowmanEnemyTag = "BowManBlue";
    public string swordmanAllyTag = "SwordManRed";
    public string swordmanEnemyTag = "SwordManBlue";

    public GameObject parentBowman;
    public BowmanScript script;
    public GameObject attackedEnemy;
    public bool alreadyHit = false;

    [SerializeField]
    private float timeToDestroyArrow = 5;
    // Start is called before the first frame update
    void Start()
    {
        script = parentBowman.GetComponent<BowmanScript>();
    }

    // Update is called once per frame
    void Update()
    {
        Invoke(nameof(destroyAfterTime), timeToDestroyArrow);
    }

    private void destroyAfterTime()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.CompareTag(spearmanEnemyTag) || collision.gameObject.CompareTag(bowmanEnemyTag) || collision.gameObject.CompareTag(swordmanEnemyTag)))
        {
            attackedEnemy = collision.gameObject;
            script.DidAttack(gameObject);
        }
    }
}

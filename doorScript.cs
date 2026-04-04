using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class doorScript : MonoBehaviour
{

    [Header("tags")]
    public string speardamage = "spearDamage";
    public string spearmanEnemyTag = "SpearManBlue";
    public string spearmanAllyTag = "SpearManRed";
    public string bowmanAllyTag = "BowManRed";
    public string bowmanEnemyTag = "BowManBlue";
    public string swordmanAllyTag = "SwordManRed";
    public string swordmanEnemyTag = "SwordManBlue";
    public string allyDoorTag = "doorRed";
    public string enemyDoorTag = "doorBlue";
    public string DeadLayer = "dead";

    [Header("Door settings")]
    public GameObject door;
    public Vector3 doorOriginalLocation;
    public Vector3 doorOpenLocation;
    public Vector3 doorDestroyedLocation;
    public bool open = false;

    [Header("Timer settings")]
    public float checkInterval = 1f; // preverjanje vsak 1s
    private float timer = 0f;

    public int armor = 0;
    public float health = 1000f;
    public bool destroyed = false;

    // buffer za OverlapBoxNonAlloc (prednastavljen na max možnih enot v triggerju)
    private Collider[] collidersBuffer = new Collider[50];

    void Start()
    {
        doorOriginalLocation = door.transform.localPosition;
        doorOpenLocation = doorOriginalLocation;
        doorOpenLocation.y += 5f; // višina odprtja

        doorDestroyedLocation = doorOriginalLocation;
        doorDestroyedLocation.y -= 10f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            TryOpenDoor();
            timer = 0f;
        }
        if (open && !destroyed)
        {
            door.transform.localPosition = Vector3.Lerp(door.transform.localPosition, doorOpenLocation, Time.deltaTime * 15);
        }
        else if (!destroyed)
        {
            door.transform.localPosition = Vector3.Lerp(door.transform.localPosition, doorOriginalLocation, Time.deltaTime * 15);
        }
        if(health <= 0)
        {
            door.transform.localPosition = Vector3.Lerp(door.transform.localPosition, doorDestroyedLocation, Time.deltaTime * 15);
            destroyed = true;
        }
    }

    private void TryOpenDoor()
    {
        // Preveri vse colliderje znotraj triggerja (brez dead layer)
        int count = Physics.OverlapBoxNonAlloc(
            GetComponent<Collider>().bounds.center,
            GetComponent<Collider>().bounds.extents,
            collidersBuffer,
            Quaternion.identity
        );

        int allyCount = 0;
        int enemyCount = 0;

        for (int i = 0; i < count; i++)
        {
            Collider c = collidersBuffer[i];

            // ignoriraj mrtve enote
            if (c.gameObject.layer == LayerMask.NameToLayer(DeadLayer))
                continue;

            if (IsAlly(c)) allyCount++;
            if (IsEnemy(c)) enemyCount++;
        }

        // Odpri vrata, èe je vsaj 1 ally in NI enemy
        if (allyCount >= 1 && enemyCount < 1)
            OpenDoor();
        else
            CloseDoor();
    }

    private void OpenDoor()
    {
        open = true;
    }

    private void CloseDoor()
    {
        open = false;
    }

    private bool IsAlly(Collider other)
    {
        return other.CompareTag(spearmanAllyTag)
            || other.CompareTag(bowmanAllyTag)
            || other.CompareTag(swordmanAllyTag);
    }

    private bool IsEnemy(Collider other)
    {
        return other.CompareTag(spearmanEnemyTag)
            || other.CompareTag(bowmanEnemyTag)
            || other.CompareTag(swordmanEnemyTag);
    }

    public void repairGate()
    {
        health = 1000;
        destroyed = false;
        door.transform.localPosition = Vector3.Lerp(door.transform.localPosition, doorOriginalLocation, Time.deltaTime * 15);
    }
}

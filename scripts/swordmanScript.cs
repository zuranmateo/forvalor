using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class swordmanScript : MonoBehaviour
{
    //tags
    [Header("tags")]
    public string speardamage = "spearDamage";
    public string spearmanEnemyTag = "SpearManBlue";
    public string spearmanAllyTag = "SpearManRed";
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

    //detection enemies
    [Header("detecting enemies")]
    public float DetectionRadius = 100f;
    public float targetRadius = 10f;
    private float distanceToTarget = 100f;

    //rotation
    [Header("rotation")]
    public float rotationSpeed = 20f;

    //calcultating offsets
    private Vector3 offsetToTarget;
    private bool hasSetOffset = false;

    //stats
    [Header("stats and upgrades")]
    public int health = 100;
    public int shieldHealth = 200;
    public int damage = 20;

    public int skill = 1;
    public float attackspeed = 3f;
    public int movespeed = 1;
    public int armor = 1;
    private bool changeArmor = true;

    public bool dead = false;

    [Header("move speed")]
    public float forwardMoveSpeed = 3f;
    public float backwardMoveSpeed = 1.5f;

    //commanding statements and varibles
    [Header("commanding statements and varibles")]
    private bool attackable = false;
    public bool canAttack = true;
    private bool agresiveMode = false;
    private bool attackWithFormation = false; //ko bodo napadali skupaj, bodo napadali iz ene strani vedno
    public bool HeAttacked = false;
    private bool isMoving = false;
    private bool inWay = false;
    private bool closeenemy = false;
    private bool isShield = false;

    //navmesh, premikanje AI
    public NavMeshAgent agent;
    public Animator animator;

    //gameobjecti tega spearmana in njegovi childi
    [Header("gameobjects and childs")]
    public GameObject body;
    public GameObject CloseEnemy;
    public GameObject target; // child transform kamor agent gre
    public GameObject shieldArea;
    public GameObject Tip;
    public GameObject AttackedEnemy;
    public UIbattleController UIbattle;
    public AIcontrollerBlue AI;
    private Rigidbody rb;
    public SkinnedMeshRenderer smr;

    [Header("materials")]
    public Material shieldMaterial;
    public Material destroyedShieldMaterial;

    [Header("audio")]
    public AudioSource AudioSource;
    public AudioClip footstep;
    public AudioClip deathSound;
    public AudioClip attackSound;
    public AudioClip armorhitSound;


    //formations
    private Vector3 localFormationPosition;
    private Quaternion localFormationRotation;
    private FormationController formation;

    //extras
    private float timer;
    private float timerMain = 0;
    private float footstepTimer = 0;
    private class formationSlot
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public int identity;
        public UIbattleController UIbattle;
        public bool alive = true;
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        AudioSource = GetComponent<AudioSource>();
        if (CompareTag("SwordManRed") && UIbattle != null)
        {
            //UIbattle = GameObject.FindGameObjectWithTag("battleUI").GetComponent<UIbattleController>();
        }
        else
        {
            //AI = GameObject.FindGameObjectWithTag("AIcontrollerBlue").GetComponent<AIcontrollerBlue>();
        }
        agent.isStopped = true;

        if (target == null)
        {
            target = transform.Find("target")?.gameObject;
            if (target == null)
            {
                Debug.LogError("Child objekt 'target' ni bil najden!");
            }
        }
        Vector3 offset = transform.position - target.transform.position;

        agent.updateRotation = false;

        agent.speed = forwardMoveSpeed + (movespeed / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;

        timerMain += Time.deltaTime;
        footstepTimer += Time.deltaTime;

        if (timerMain < 0.3f) return;

        timerMain = 0;
        GetObjectsWithTagInRadius();

        bool isCharging = false;
        bool hold = false;
        if (isMoving)
        {
            float forwardAmount = Vector3.Dot(agent.velocity.normalized, transform.forward);

            if (forwardAmount > 0.1f)
            {
                // Gre naprej
                if (isShield)
                {
                    agent.speed = forwardMoveSpeed + (movespeed / 2) - 0.5f;
                }
                else
                {
                    agent.speed = forwardMoveSpeed + (movespeed / 2);
                }
            }
            else if (forwardAmount < -0.1f)
            {
                // Gre nazaj
                agent.speed = backwardMoveSpeed;
            }
            if (footstepTimer >= 0.7f)
            {
                AudioSource.PlayOneShot(footstep);
                footstepTimer = 0;
            }
        }

        if (CompareTag("SwordManRed"))
        {
            isCharging = UIbattle != null && UIbattle.Charge;
            hold = UIbattle != null && UIbattle.hold;
            attackWithFormation = UIbattle != null && UIbattle.Engage;
            attackWithFormation = false; //temporaray
        }
        else if (CompareTag("SwordManBlue"))
        {
            isCharging = AI != null && AI.Charge;
            hold = AI != null && AI.hold;
            attackWithFormation = AI != null && AI.Engage;
        }

        if (hold && changeArmor == true)
        {
            armor = armor + 5;
            changeArmor = false;
        }
        else if (!hold && changeArmor == false)
        {
            armor = armor - 5;
            changeArmor = true;
        }
        float targetRadiusT = hold ? 5f : targetRadius;


        if (CloseEnemy != null)
        {
            if (CloseEnemy.CompareTag(bowmanEnemyTag))
            {
                BowmanScript enemyScript = CloseEnemy.GetComponent<BowmanScript>();
                if (enemyScript == null || enemyScript.dead)
                {
                    CloseEnemy = null;
                    attackable = false;
                }
            }
            else if (CloseEnemy.tag == spearmanEnemyTag)
            {
                SpearmanScript enemyScript = CloseEnemy.GetComponent<SpearmanScript>();
                if (enemyScript == null || enemyScript.dead)
                {
                    CloseEnemy = null;
                    attackable = false;
                }
            }
            else if (CloseEnemy.tag == swordmanEnemyTag)
            {
                swordmanScript enemyScript = CloseEnemy.GetComponent<swordmanScript>();
                if (enemyScript == null || enemyScript.dead)
                {
                    CloseEnemy = null;
                    attackable = false;
                }
            }
            else if (CloseEnemy.tag == EnemyDoorTag)
            {
                GameObject parent = GameObject.FindGameObjectWithTag(EnemyDoorTag).transform.parent.gameObject;

                doorScript enemyScript = parent.GetComponent<doorScript>();
                if (enemyScript == null || enemyScript.destroyed)
                {
                    CloseEnemy = null;
                    attackable = false;
                }
            }
            else if (CloseEnemy.CompareTag(throneEnemyTag))
            {
                throneScript script = CloseEnemy.GetComponent<throneScript>();
                if (script == null || script.destroyed)
                {
                    Debug.Log("script is null");
                    CloseEnemy = null;
                    attackable = false;
                }
            }
            else if (CloseEnemy.CompareTag(EnemyBuildingTag))
            {
                MoneyBuldingScript script = CloseEnemy.GetComponent<MoneyBuldingScript>();
                if (script == null || script.destroyed)
                {
                    Debug.Log("script is null");
                    CloseEnemy = null;
                    attackable = false;
                }
            }

            if (CloseEnemy != null)
            {
                inWay = false;
                Vector3 dir = (CloseEnemy.transform.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, CloseEnemy.transform.position);
                RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, dist);

                foreach (var h in hits)
                {
                    GameObject col = h.collider.gameObject;
                    if (col.CompareTag(spearmanEnemyTag) || col.CompareTag(bowmanEnemyTag) || col.CompareTag(swordmanEnemyTag) || col.CompareTag(DeadLayer) || col.CompareTag(speardamage) || col.CompareTag(spearmanAllyTag) || col.CompareTag(bowmanAllyTag) || col.CompareTag(swordmanAllyTag) || col.CompareTag(throneEnemyTag) || col.CompareTag(throneAllyTag) || col.CompareTag(AllyBuildingTag) || col.CompareTag("shield")) continue;

                    //Debug.Log("Blocked by: " + h.collider.name);
                    inWay = true;
                    break;
                }

                if(DetectionRadius >= dist)
                {
                    closeenemy = true;
                }
                else
                {
                    closeenemy = false;
                }
            }
        }
        if (((hold || closeenemy) && !isCharging) && HeAttacked == false && shieldHealth >= 0)
        {
            isShield = true;
        }
        else
        {
            isShield = false;
        }

        if(isShield)
        {
            animator.SetBool("shield", true);
            shieldArea.SetActive(false);
        }
        else
        {
            animator.SetBool("shield", false);
            shieldArea.SetActive(false);
        }

        if (CloseEnemy != null && target != null && ((isCharging || (distanceToTarget <= targetRadiusT && (!inWay || CloseEnemy.CompareTag(EnemyDoorTag)))) || attackWithFormation))
        {
            if (!hold && HeAttacked == false)
            {
                if (!attackWithFormation)
                {
                    // Izracunaj smer od agenta do sovraznika (na XZ ravnini)
                    Vector3 directionFromAgentToEnemy = CloseEnemy.transform.position - transform.position;
                    directionFromAgentToEnemy.y = 0;
                    directionFromAgentToEnemy.Normalize();

                    // Izracunamo dolzino offseta iz zacetka (koliko naj bo target oddaljen od enemy-ja)
                    float offsetDistance = offsetToTarget.magnitude;

                    // Target naj bo tocno v tej smeri, na doloceni razdalji od sovraznika
                    Vector3 finalOffset = -directionFromAgentToEnemy * offsetDistance;
                    target.transform.position = CloseEnemy.transform.position + finalOffset;
                }
                else
                {
                    //NAREDI ENGAGE
                }
                isMoving = true;
                agent.SetDestination(target.transform.position);
            }

            Vector3 dir = CloseEnemy.transform.position - transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            float distToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distToTarget <= 0.6f || hold)
            {
                StopWalking();
            }
            else
            {
                agent.isStopped = false;
                animator.SetBool("isWalking", true);
            }
        }
        else
        {
            //Debug.Log(agent.transform.localPosition + ", " + localFormationPosition);
            target.transform.localPosition = Vector3.zero;
            ReturnToFormation(); // vedno se vraca, ce ni napada
        }
        if (agresiveMode && canAttack && attackable)
        {
            //Debug.Log(agresiveMode.ToString() + canAttack + attackable);
            StopWalking();
            attack();
        }

        if (health <= 0 || (UIbattle && !UIbattle.alive))
        {
            dead = true;
            AudioSource.PlayOneShot(deathSound);
            gameObject.tag = "dead";
            StopWalking();
            animator.SetBool("dead", true);
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(GetComponent<BoxCollider>());
            this.gameObject.layer = LayerMask.NameToLayer(DeadLayer);
            if (formation != null)
            {
                formation.writeSoliderAsDead(this.gameObject);
            }
            Invoke(nameof(afterDeath), 20);
        }
    }


    public void StopWalking()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        isMoving = false;
        animator.SetBool("isWalking", false);
        target.transform.localPosition = Vector3.zero;
        hasSetOffset = false;
    }

    private void afterDeath()
    {
        Destroy(gameObject);
    }
    void GetObjectsWithTagInRadius()
    {
        //timer += Time.deltaTime;

        //if (timer < 0.5f) return;

        //timer = 0f;

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(spearmanEnemyTag);
        GameObject[] taggedSwordmans = GameObject.FindGameObjectsWithTag(swordmanEnemyTag);
        GameObject[] taggedBowmans = GameObject.FindGameObjectsWithTag(bowmanEnemyTag);
        GameObject[] taggedDoors = GameObject.FindGameObjectsWithTag(EnemyDoorTag);
        GameObject[] taggedThrone = GameObject.FindGameObjectsWithTag(throneEnemyTag);
        GameObject[] taggedBuilding = GameObject.FindGameObjectsWithTag(EnemyBuildingTag);

        List<GameObject> list = new List<GameObject>(taggedObjects);
        list.AddRange(taggedSwordmans);
        list.AddRange(taggedBowmans);
        list.AddRange(taggedDoors);
        list.AddRange(taggedThrone);
        list.AddRange(taggedBuilding);

        taggedObjects = list.ToArray();

        float closestDistance = float.MaxValue;
        bool DEAD = false;
        CloseEnemy = null;

        foreach (GameObject obj in taggedObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            bool isCharging = false;
            //bool hold = false;

            if (CompareTag("SpearManRed"))
            {
                isCharging = UIbattle != null && UIbattle.Charge;
                //hold = UIbattle != null && UIbattle.hold;
            }
            else if (CompareTag("SpearManBlue"))
            {
                isCharging = AI != null && AI.Charge;
                //hold = AI != null && AI.hold;
            }


            if (obj.tag == spearmanEnemyTag)
            {
                SpearmanScript script = obj.GetComponent<SpearmanScript>();
                DEAD = script.dead;
            }
            else if (obj.tag == bowmanEnemyTag)
            {
                BowmanScript script = obj.GetComponent<BowmanScript>();
                DEAD = script.dead;

            }
            else if (obj.tag == swordmanEnemyTag)
            {
                swordmanScript script = obj.GetComponent<swordmanScript>();
                DEAD = script.dead;
            }
            else if (obj.tag == EnemyDoorTag)
            {
                GameObject parent = GameObject.FindGameObjectWithTag(EnemyDoorTag).transform.parent.gameObject;

                doorScript script = parent.GetComponent<doorScript>();
                DEAD = script.destroyed;
            }
            else if (obj.CompareTag(throneEnemyTag))
            {
                throneScript script = obj.GetComponent<throneScript>();
                DEAD = script.destroyed;
            }
            else if (obj.CompareTag(EnemyBuildingTag))
            {
                MoneyBuldingScript script = obj.GetComponent<MoneyBuldingScript>();
                DEAD = script.destroyed;
            }

            if ((distance <= DetectionRadius || isCharging) && distance < closestDistance)
            {
                if (DEAD == false)
                {

                    distanceToTarget = distance;
                    closestDistance = distance;
                    CloseEnemy = obj;


                    // Set offset only once when enemy is found
                    if (!hasSetOffset && target != null)
                    {
                        target.transform.localPosition = Vector3.zero;
                        offsetToTarget = transform.position - target.transform.position;
                        hasSetOffset = true;
                    }
                }
            }
        }
        if (distanceToTarget < DetectionRadius)
        {
            agresiveMode = CloseEnemy != null;
            animator.SetBool("agresiveMode", agresiveMode);
            //Debug.Log("worked" + CloseEnemy);
        }
        if (CloseEnemy == null) animator.SetBool("agresiveMode", false);
    }



    public void ReturnToFormation()
    {
        if (formation == null || agent == null) return;

        Vector3 worldPos = formation.GetWorldPositionFromLocal(localFormationPosition);
        Quaternion worldRot = formation.GetWorldRotationFromLocal(localFormationRotation);

        //agent.updateRotation = false; // Zelo pomembno
        agent.SetDestination(worldPos);
        agent.isStopped = false;
        animator.SetBool("isWalking", true);

        if (agresiveMode)
        {
            // Najprej obrni, da hodijo naprej
            agent.updateRotation = false;
            transform.rotation = Quaternion.Slerp(transform.rotation, worldRot, rotationSpeed * Time.deltaTime);
        }

        StartCoroutine(ReturnAndRotate(worldPos, worldRot));
    }
    private IEnumerator ReturnAndRotate(Vector3 destination, Quaternion targetRotation)
    {
        const float closeEnough = 0.6f;
        const float angleThreshold = 5f;

        while (true)
        {
            float dist = Vector3.Distance(transform.position, destination);
            float angle = Quaternion.Angle(transform.rotation, targetRotation);

            // ce se agent res premika, vklopi hojo
            if (agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("isWalking", true);
                isMoving = true;
            }
            else
            {
                animator.SetBool("isWalking", false);
                isMoving = false;
            }

            // ce smo dosegli tocko in smo obrnjeni, koncaj
            if (dist <= closeEnough && angle <= angleThreshold)
            {
                //agent.updateRotation = true;
                StopWalking(); // To  ustavi agenta in izklopi animacijo
                break;
            }

            // ce nismo agresivni, pocakaj da pride in se obrni
            if (!agresiveMode && dist > closeEnough)
            {
                // V agresiveMode == false hoce gledat kam gre
                Vector3 dir = destination - transform.position;
                dir.y = 0f; // Ignoriramo visino, da ne pride do "pada naprej"

                if (dir.sqrMagnitude > 0.001f) // Preverimo, da ni skoraj ni
                {
                    //Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                    //Debug.Log("prvi rotate");
                    agent.updateRotation = true;
                }
                else
                {
                    agent.updateRotation = false;
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    //Debug.Log("drugi rotate");
                }
            }
            else
            {
                // Ko je blizu cilja ali v agresive modu, rotira proti formacijski smeri
                //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (CompareTag(swordmanAllyTag) && (other.CompareTag(spearmanEnemyTag) || other.CompareTag(bowmanEnemyTag) || other.CompareTag(swordmanEnemyTag) || other.CompareTag(EnemyDoorTag) || other.CompareTag(throneEnemyTag) || other.CompareTag(EnemyBuildingTag)))
        {
            attackable = true;
        }
    }

    public void SetFormationHome(Vector3 localPos, Quaternion localRot, FormationController parentFormation)
    {
        localFormationPosition = localPos;
        localFormationRotation = localRot;
        formation = parentFormation;
    }

    private void DidAttack()
    {
        SpearTipDamage tipdamage = Tip.GetComponent<SpearTipDamage>();

        if (tipdamage != null && !dead && tipdamage.AttackedEnemy && CloseEnemy != null)
        {
            if (CloseEnemy.tag == bowmanEnemyTag)
            {
                BowmanScript bowman = CloseEnemy.GetComponent<BowmanScript>();
                AttackedEnemy = tipdamage.AttackedEnemy;
                if (AttackedEnemy == null) return;
                int DealtDamage = damage + skill * 2;

                if (bowman != null && !dead)
                {
                    DealtDamage = DealtDamage - bowman.armor + skill;
                    if (DealtDamage <= 0)
                    {
                        DealtDamage = 1;
                    }
                    bowman.health = bowman.health - DealtDamage;
                    bowman.AudioSource.PlayOneShot(bowman.armorhitSound);
                    //Debug.Log("damage dealt: " + DealtDamage + "who was hit: " + AttackedEnemy);
                    AttackedEnemy = null;
                }
            }
            else if (CloseEnemy.CompareTag(spearmanEnemyTag))
            {
                AttackedEnemy = tipdamage.AttackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (AttackedEnemy == null) return;
                SpearmanScript spearman = AttackedEnemy.GetComponent<SpearmanScript>();

                if (spearman != null && !dead)
                {
                    DealtDamage = DealtDamage - spearman.armor + skill;
                    if (DealtDamage <= 0)
                    {
                        DealtDamage = 1;
                    }
                    spearman.health = spearman.health - DealtDamage;
                    spearman.AudioSource.PlayOneShot(spearman.armorhitSound);
                    //Debug.Log("damage dealt: " + DealtDamage + "who was hit: " + AttackedEnemy);
                    AttackedEnemy = null;
                }
            }
            else if (CloseEnemy.CompareTag(swordmanEnemyTag))
            {
                AttackedEnemy = tipdamage.AttackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (AttackedEnemy == null) return;
                swordmanScript swordman = AttackedEnemy.GetComponent<swordmanScript>();

                if (swordman != null && !dead)
                {
                    DealtDamage = DealtDamage - swordman.armor + skill;

                    if (DealtDamage <= 0)
                    {
                        DealtDamage = 1;
                    }

                    if(swordman.shieldHealth <= 0)
                    {
                        swordman.health = swordman.health - DealtDamage;
                    }
                    else
                    {
                        swordman.shieldHealth = swordman.shieldHealth - DealtDamage;
                    }
                    swordman.AudioSource.PlayOneShot(swordman.armorhitSound);
                    //Debug.Log("damage dealt: " + DealtDamage + "who was hit: " + AttackedEnemy);
                    AttackedEnemy = null;
                }
            }
            else if (CloseEnemy.CompareTag(EnemyDoorTag))
            {
                AttackedEnemy = tipdamage.AttackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (AttackedEnemy == null) return;
                GameObject parent = AttackedEnemy.transform.parent.gameObject;
                doorScript door = parent.GetComponent<doorScript>();
                //Debug.Log(door + " " + AttackedEnemy + " parent is: " + parent);
                if (door != null && !dead)
                {
                    DealtDamage = DealtDamage - door.armor;
                    if (DealtDamage <= 0)
                    {
                        DealtDamage = 1;
                    }
                    door.health = door.health - DealtDamage;
                    //Debug.Log("damage dealt: " + DealtDamage + "who was hit: " + AttackedEnemy);
                    AttackedEnemy = null;
                }
            }
            else if (CloseEnemy.CompareTag(throneEnemyTag))
            {
                AttackedEnemy = tipdamage.AttackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (AttackedEnemy == null) return;
                throneScript throne = AttackedEnemy.GetComponent<throneScript>();

                if (throne != null && !dead)
                {
                    throne.health = throne.health - DealtDamage;
                    AttackedEnemy = null;
                }
            }
            else if (CloseEnemy.CompareTag(EnemyBuildingTag))
            {
                AttackedEnemy = tipdamage.AttackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (DealtDamage <= 0)
                {
                    DealtDamage = 1;
                }
                if (AttackedEnemy == null) return;
                MoneyBuldingScript building = AttackedEnemy.GetComponent<MoneyBuldingScript>();

                if (building != null && !dead)
                {
                    building.health = building.health - DealtDamage;
                    AttackedEnemy = null;
                }
            }
        }
    }
    private void attack()
    {
        attackable = false;
        if (CloseEnemy == null) return;

        
        HeAttacked = true;
        isShield = false;
        int r = Random.Range(0, 2);
        if (r == 0)
        {
            animator.SetBool("stab", true);
        }
        else
        {
            animator.SetBool("attack", true);
        }
        canAttack = false;
        StopWalking();
        Invoke(nameof(attackCooldown), 1.1f);
    }

    private void attackCooldown()
    {
        DidAttack();
        StopWalking();
        float ActiveAttackSpeed = attackspeed - (skill / 4);
        animator.SetBool("attack", false);
        animator.SetBool("stab", false);
        //Debug.Log(ActiveAttackSpeed);
        HeAttacked = false;
        Invoke(nameof(attackCooldowned), Random.Range(0f, ActiveAttackSpeed));
    }

    private void attackCooldowned()
    {
        attackable = false;
        canAttack = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UIplayerController;

public class BowmanScript : MonoBehaviour
{
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

    //detection enemies
    [Header("detecting enemies")]
    public float redetectTime = 15f;
    public float DetectionRadius = 100f;
    public float targetRadius = 50f;
    private float distanceToTarget = 100f;
    private bool inDistance = false;

    //rotation
    [Header("rotation")]
    public float rotationSpeed = 5f;

    //calcultating offsets
    private Vector3 offsetToTarget;
    private bool hasSetOffset = false;
    private float offset;

    //stats
    [Header("stats and upgrades")]
    public int health = 100;
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
    public bool canAttack = true;
    private bool agresiveMode = false;
    private bool attackWithFormation = false; //ko bodo napadali skupaj, bodo napadali iz ene strani vedno
    public bool HeAttacked = false;
    private bool isMoving = false;

    //navmesh, premikanje AI
    public NavMeshAgent agent;
    public Animator animator;
    [Header("gameobjects and childs")]
    public GameObject CloseEnemy;
    public GameObject target; // child transform kamor agent gre
    public GameObject AttackedEnemy;
    public UIbattleController UIbattle;
    public AIcontrollerBlue AI;
    private Rigidbody rb;

    [Header("arrows")]
    public GameObject arrowPrefab;
    public GameObject arrowPoint;
    public float arrowSpeed = 600;

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
    //private float timer = 0;
    private float timer2 = 0;
    private float timerMain = 0;
    private float footstepTimer = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        AudioSource = GetComponent<AudioSource>();
        if (CompareTag("BowManRed") && UIbattle != null)
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
        offset = Vector3.Distance(transform.position, target.transform.position);

        agent.updateRotation = false;

        agent.speed = forwardMoveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;

        timerMain += Time.deltaTime;
        timer2 += Time.deltaTime;
        footstepTimer += Time.deltaTime;
        if (timerMain < 0.3f) return;

        timerMain = 0;

        GetObjectsWithTagInRadius();

        StartCoroutine(MoveCheckCoroutine());

        bool isCharging = false;
        bool hold = false;
        if (isMoving)
        {
            float forwardAmount = Vector3.Dot(agent.velocity.normalized, transform.forward);

            if (forwardAmount > 0.1f)
            {
                // Gre naprej
                agent.speed = forwardMoveSpeed;
            }
            else if (forwardAmount < -0.1f)
            {
                // Gre nazaj
                agent.speed = backwardMoveSpeed;
            }
            if (footstepTimer >= 0.5f)
            {
                AudioSource.PlayOneShot(footstep);
                footstepTimer = 0;
            }
        }

        if (CompareTag("BowManRed"))
        {
            isCharging = UIbattle != null && UIbattle.Charge;
            hold = UIbattle != null && UIbattle.hold;
            attackWithFormation = UIbattle != null && UIbattle.Engage;
            attackWithFormation = false; //temporaray
        }
        else if (CompareTag("BowManBlue"))
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
        else if(!hold && changeArmor == false)
        {
            armor = armor - 5;
            changeArmor = true;
        }
        //Debug.Log(isCharging);
        if (CloseEnemy != null)
        {
            if (CloseEnemy.tag == bowmanEnemyTag)
            {
                BowmanScript enemyScript = CloseEnemy.GetComponent<BowmanScript>();
                if (enemyScript == null || enemyScript.dead)
                {
                    CloseEnemy = null;
                }
            }
            else if (CloseEnemy.tag == spearmanEnemyTag)
            {
                SpearmanScript enemyScript = CloseEnemy.GetComponent<SpearmanScript>();
                if (enemyScript == null || enemyScript.dead)
                {
                    CloseEnemy = null;
                }
            }
            else if (CloseEnemy.tag == swordmanEnemyTag)
            {
                swordmanScript enemyScript = CloseEnemy.GetComponent<swordmanScript>();
                if (enemyScript == null || enemyScript.dead)
                {
                    CloseEnemy = null;
                }
            }
        }
        if (CloseEnemy != null && target != null && (isCharging || attackWithFormation))
        {
            float distance = Vector3.Distance(CloseEnemy.transform.position, transform.position);
            if (distance < offset && distance > (offset / 2))
            {
                inDistance = true;
            }
            else
            {
                inDistance = false;
            }
            if (!hold && !inDistance && HeAttacked == false)
            {
                //Debug.Log(hold + " " + inDistance + " " + HeAttacked);
                if (!attackWithFormation)
                {
                    //Debug.Log("walk");
                    Vector3 directionFromAgentToEnemy = CloseEnemy.transform.position - transform.position;
                    directionFromAgentToEnemy.y = 0;
                    directionFromAgentToEnemy.Normalize();

                    float offsetDistance;
                    if (distance > offset)
                    {
                        offsetDistance = offset;
                        //Debug.Log("bigger that offset");
                    }
                    else{
                        offsetDistance = offset/2;
                        //Debug.Log("smaller that offset");
                    }

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
            ReturnToFormation();
        }
        if (canAttack && !isMoving && CloseEnemy != null)
        {
            if (Vector3.Distance(CloseEnemy.transform.position, transform.position) < offset)
            {
                StopWalking();
                drawBow();
            }
        }
        else
        {
            animator.SetBool("draw", false);
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

    IEnumerator MoveCheckCoroutine()
    {
        while (true)
        {
            Vector3 p1 = transform.position;
            yield return new WaitForSeconds(1f);
            Vector3 p2 = transform.position;
            isMoving = (p1 != p2);
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

        //if (timer < 1f) return;

        //timer = 0f;
        //float closestDistance = float.MaxValue;
        float Distance = 0f;
        bool DEAD = false;
        int i = 0;

        if (CloseEnemy != null)
        {
            if (CloseEnemy.tag == DeadLayer)
            {
                DEAD = true;
            }
            Distance = Vector3.Distance(transform.position, CloseEnemy.transform.position);
            //Debug.Log("dead " + DEAD + " distance: " + Distance + " < " + DetectionRadius + " time: " + timer2 + " < " + redetectTime + " enemy: " + CloseEnemy);
        }
        if(DEAD == true) CloseEnemy = null;
        
        if ((DEAD == false && timer2 < redetectTime) && CloseEnemy != null) return;
        bool isCharging = false;
        //Debug.Log("detecting");
        if (CompareTag("BowManRed"))
        {
            isCharging = UIbattle != null && UIbattle.Charge;
        }
        else if (CompareTag("BowManBlue"))
        {
            isCharging = AI != null && AI.Charge;
        }
        //Debug.Log(isCharging);
        if (timer2 < redetectTime && (Distance <= DetectionRadius || !isCharging) && CloseEnemy != null) return; //ni zgoraj, saj mora prevejati in is charging je nepotrebna obremenitev.
            //Debug.Log(Distance + " is charging: " + isCharging);
            timer2 = 0f;

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(spearmanEnemyTag);
        GameObject[] taggedSwordmans = GameObject.FindGameObjectsWithTag(swordmanEnemyTag);
        GameObject[] taggedBowmans = GameObject.FindGameObjectsWithTag(bowmanEnemyTag);


        List<GameObject> list = new List<GameObject>(taggedObjects);
        list.AddRange(taggedSwordmans);
        list.AddRange(taggedBowmans);

        taggedObjects = list.ToArray();

        

        List<GameObject> CloseEnemies = new List<GameObject>();
        GameObject closest = null;
        float closestDist = 0;

        foreach (GameObject obj in taggedObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);

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

            if ((distance <= DetectionRadius || isCharging))
            {
                if (DEAD == false)
                {
                    bool inway = false;
                    Vector3 dir = (obj.transform.position - transform.position).normalized;
                    float dist = Vector3.Distance(transform.position, obj.transform.position);
                    RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, dist);

                    foreach (var h in hits)
                    {
                        GameObject col = h.collider.gameObject;
                        if (col.CompareTag(spearmanEnemyTag) || col.CompareTag(bowmanEnemyTag) || col.CompareTag(swordmanEnemyTag) || col.CompareTag(DeadLayer) || col.CompareTag(speardamage) || col.CompareTag("shield")) continue;

                        //Debug.Log("Blocked by: " + h.collider.name);
                        inway = true;
                        break;
                    }

                    if (!inway)
                    {
                        distanceToTarget = distance;
                        CloseEnemies.Add(obj);
                        i++;


                        // Set offset only once when enemy is found
                        if (!hasSetOffset && target != null)
                        {
                            target.transform.localPosition = Vector3.zero;
                            offsetToTarget = transform.position - target.transform.position;
                            hasSetOffset = true;
                        }
                    }

                    if (isCharging)
                    {
                        if (closestDist < distance)
                        {
                            closestDist = distance;
                            closest = obj;
                        }
                    }
                }
            }
        }
        if (CloseEnemies.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, i);
            CloseEnemy = CloseEnemies[r];
            i = 0;
            CloseEnemies = null;
        }
        else if (closest != null)
        {
            CloseEnemy = closest;
        }
        
        agresiveMode = CloseEnemy != null;
    }

    public void SetFormationHome(Vector3 localPos, Quaternion localRot, FormationController parentFormation)
    {
        localFormationPosition = localPos;
        localFormationRotation = localRot;
        formation = parentFormation;
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
            
            //rotate to close enemy if close enough
            float distToTarget = Vector3.Distance(transform.position, CloseEnemy.transform.position);
            if (distToTarget < offset)
            {
                Vector3 dir = CloseEnemy.transform.position - transform.position;
                dir.y = 0f;

                if (dir != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                agent.updateRotation = false;
                transform.rotation = Quaternion.Slerp(transform.rotation, worldRot, rotationSpeed * Time.deltaTime);
            }
        }

        if (HeAttacked == false)
        {
            StartCoroutine(ReturnAndRotate(worldPos, worldRot));
        }
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
                StopWalking(); // To ce ustavi agenta in izklopi animacijo
                break;
            }

            // ce nismo agresivni, pocakaj da pride in se obrni
            if (!agresiveMode && dist > closeEnough)
            {
                // V agresiveMode == false hoce gledat kam gre
                Vector3 dir = destination - transform.position;
                dir.y = 0f; // Ignoriramo visino, da ne pride do "pada naprej"

                if (dir.sqrMagnitude > 0.001f) 
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

    public void drawBow()
    {
        if (CloseEnemy == null) return;

        //Debug.Log(isMoving);
        if (!isMoving)
        {
            HeAttacked = true;
            animator.SetBool("draw", true);
            canAttack = false;
            StopWalking();
            Invoke(nameof(attackCooldown), 3f);
        }
    }
    private void attackCooldown()
    {
        if (CloseEnemy != null)
        {
            StopWalking();
            float ActiveAttackSpeed = attackspeed - (skill / 4);

            Quaternion addRotation = Quaternion.Euler(0f, 90f, 0f);
            float distance = Vector3.Distance(CloseEnemy.transform.position, transform.position);

            float pitchAngle;
            if (distance < 20f) pitchAngle = 2f;
            else if (distance < 25f) pitchAngle = 5f;
            else if (distance < 30f) pitchAngle = 6f;
            else if (distance < 35f) pitchAngle = 7f;
            else if (distance < 45f) pitchAngle = 8f;
            else if (distance < 55f) pitchAngle = 8.5f;
            else pitchAngle = 9f;

            if (!isMoving)
            {
                GameObject arrow = Instantiate(
                    arrowPrefab,
                    arrowPoint.transform.position,
                    arrowPoint.transform.rotation * addRotation
                );

                //play shoot arrow sound
                AudioSource.PlayOneShot(attackSound);

                Vector3 targetDir =
                (CloseEnemy.transform.position - arrowPoint.transform.position);

                Vector3 flatDir = new Vector3(targetDir.x, 0f, targetDir.z).normalized;
                float heightDiff = targetDir.y;

                Vector3 pitchAxis = Vector3.Cross(flatDir, Vector3.up);

                // osnovni kot glede na višino
                float heightAngle = Mathf.Atan2(heightDiff, targetDir.magnitude) * Mathf.Rad2Deg;

                // konèni kot
                float finalPitch = heightAngle + pitchAngle;

                Vector3 shootDirection =
                    Quaternion.AngleAxis(finalPitch, pitchAxis) * flatDir;

                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                rb.AddForce(shootDirection.normalized * arrowSpeed);

                arrow.GetComponent<arrowScript>().parentBowman = gameObject;

                animator.SetBool("draw", false);
                HeAttacked = false;
                Invoke(nameof(attackCooldowned), UnityEngine.Random.Range(0f, ActiveAttackSpeed));
            }

        }
    }

    private void attackCooldowned()
    {
        canAttack = true;
    }

    public void DidAttack(GameObject arrow)
    {
        arrowScript arrowDamage = arrow.GetComponent<arrowScript>();

        if (arrowDamage != null && arrowDamage.alreadyHit == false && arrowDamage.attackedEnemy && CloseEnemy != null)
        {
            if (CloseEnemy.tag == bowmanEnemyTag)
            {
                BowmanScript bowman = CloseEnemy.GetComponent<BowmanScript>();
                AttackedEnemy = arrowDamage.attackedEnemy;
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
            else if (CloseEnemy.tag == spearmanEnemyTag)
            {
                AttackedEnemy = arrowDamage.attackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (DealtDamage <= 0)
                {
                    DealtDamage = 1;
                }
                if (AttackedEnemy == null) return;
                SpearmanScript spearman = AttackedEnemy.GetComponent<SpearmanScript>();

                if (spearman != null && !dead)
                {
                    DealtDamage = DealtDamage - spearman.armor + skill;
                    spearman.health = spearman.health - DealtDamage;
                    spearman.AudioSource.PlayOneShot(spearman.armorhitSound);
                    //Debug.Log("damage dealt: " + DealtDamage + "who was hit: " + AttackedEnemy);
                    AttackedEnemy = null;
                }
            }
            else if (CloseEnemy.tag == swordmanEnemyTag)
            {
                AttackedEnemy = arrowDamage.attackedEnemy;
                int DealtDamage = damage + skill * 2;
                if (DealtDamage <= 0)
                {
                    DealtDamage = 1;
                }
                if (AttackedEnemy == null) return;
                swordmanScript swordman = AttackedEnemy.GetComponent<swordmanScript>();

                if (swordman != null && !dead)
                {
                    DealtDamage = DealtDamage - swordman.armor + skill;
                    if (swordman.shieldHealth <= 0)
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
        }
    }
}

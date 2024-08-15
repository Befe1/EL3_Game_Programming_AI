using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using BigRookGames.Weapons;

/// <summary>
/// Ai Bot Controller
/// </summary>
public class Runner : AiFiniteStates
{
    public bool IsDisguise = false;
    public AiId id;
   
    [SerializeField] Transform point;   
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float shootDelay = .5f;
    [SerializeField] float coverDelay = .5f;
    [SerializeField] float coverStateDelay = .5f;
    [SerializeField] float shootStateDelay = .5f;
    [SerializeField] GunfireController gunFx;
    [SerializeField] GameObject spotLight;

   
    private float nextTurnTime;
    private Transform startTransform;

    public float multiplyBy;
    public float multiplyByMax;

    bool isGoingTowardBonus = false;
    float T_ShootDelay;
    float T_CoverDelay;
    float T_CoverStateDelay;
    float T_ShootStateDelay;
    bool isAiDetect = false;
    bool isNewStatePos = false;
    bool goingToCover = false;
    bool isCoverStand = true;
    Transform otherAi;

    AiHealth health;
    /// <summary>
    /// Init and Ranomize some variable of Ai Bot
    /// </summary>
    void Start()
    {      
        RandomPosition();

        coverDelay = Random.Range(2.5f,5f);
        shootStateDelay = Random.Range(2.5f, 5f);
        coverStateDelay = Random.Range(4.5f, 7.7f);

        health = GetComponent<AiHealth>();

    }
    Vector3 finalPosition;

    /// <summary>
    /// Get the New Random Position
    /// </summary>
    /// <returns></returns>
    Vector3 RandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range( walkRadius,walkRadiusMax);
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        finalPosition = hit.position;

        while (finalPosition.x.ToString().Equals("Infinity"))
        {
            randomDirection = Random.insideUnitSphere * Random.Range(walkRadius, walkRadiusMax);
            randomDirection += transform.position;
            NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
            finalPosition = hit.position;
        }


       // print(hit.position);

      
        point.position = finalPosition;
       // print(finalPosition.x.ToString());
        return finalPosition;
    }


  

    /// <summary>
    /// States Working
    /// </summary>
    void Update()
    {       


        if (T_ShootDelay < shootDelay)
            T_ShootDelay += Time.deltaTime;     



        if (state == AiStates.searching)
        {
            StateSearching();
        }

        else if (state == AiStates.shooting)
        {
            StateShoot();
        }


        else if (state == AiStates.goingToCover)
        {
            StateGoingToCover();
        }

        else if (state == AiStates.onCover)
        {
            StateOnCover();
        }

        else if (state == AiStates.goingToNewShootingPos)
        {
            StateNewShootingPos();
        }


    }

   /// <summary>
   /// New State Position of Standing Shooting
   /// </summary>
    void StateNewShootingPos()
    {
        print("State New Shoot Pos ...");
       
        agent.isStopped = false;

        if (!isNewStatePos)
        {
            var d = RandomPosition();

            while (Vector3.Distance(d, transform.position) < 15)
            {
                d = RandomPosition();
            }

          

            isNewStatePos = true;

            agent.SetDestination(finalPosition);
        }       

       else if (NavMeshGetPathRemainingDistance(agent) < .9f)
        {
            print("End !! State New Shoot Pos ...");

            isNewStatePos = false;

            if (isRandomOdd()) {
                
                agent.isStopped = true;
                state = AiStates.shooting;
            }
            else
            {
                SetGoingToCoverState();
            }
        }
    }

    /// <summary>
    /// Set State of Standing Run Pos
    /// </summary>
    void SetStateNewShootingPos()
    {
        GetComponents<Collider>()[0].enabled = true;
        agent.isStopped = true;
        SetAnimCover(false);
        state = AiStates.goingToNewShootingPos;
    }

    /// <summary>
    /// State Searching of other Ai
    /// </summary>
    void StateSearching()
    {
        if (agent.velocity.magnitude == 0)
        {
            var d = RandomPosition();

            while (Vector3.Distance(d, transform.position) < 15)
            {
                d = RandomPosition();
            }
        }


        agent.SetDestination(finalPosition);

        if (NavMeshGetPathRemainingDistance(agent) < .3f)
        {
            var d = RandomPosition();

            while (Vector3.Distance(d, transform.position) < 15)
            {
                d = RandomPosition();
            }

        }       

            RaycastHit hit;
          
            if (Physics.Raycast(transform.position + new Vector3(0, 2, 0), transform.TransformDirection(Vector3.forward), out hit, rayDistance))
            {
                Debug.DrawRay(transform.position + new Vector3(0, 2, 0), transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);

                if (hit.collider.gameObject.name.StartsWith("Runner"))
                {
                    Debug.Log("Found !!");
                    spotLight.SetActive(false);
                    otherAi = hit.collider.transform;

                if(isRandomOdd())
                
                    state = AiStates.shooting;                
                else
                    state = AiStates.goingToCover;

                }


                Debug.Log("Did Hit");
            }
            else
            {
                Debug.DrawRay(transform.position + new Vector3(0, 2, 0), transform.TransformDirection(Vector3.forward) * rayDistance, Color.blue);
                Debug.Log("Did not Hit");
            }
        
    }

    /// <summary>
    /// Shoot the Rifle
    /// </summary>
    void Shoot()
    {
        transform.LookAt(otherAi);

        if (T_ShootDelay >= shootDelay)
        {
            T_ShootDelay = 0;

            var b = Instantiate(bullet, bulletSpawnPoint.position, transform.rotation);
            b.GetComponent<Bullet>().speed = 97;

            gunFx.FireWeapon();
        }
    }
    /// <summary>
    /// Stae of Stand Shoot
    /// </summary>
    void StateShoot()
    {       
        agent.isStopped = true;
        Shoot();

        if (T_ShootStateDelay < shootStateDelay)
            T_ShootStateDelay += Time.deltaTime;

        if (T_ShootStateDelay >= shootStateDelay)
        {
            T_ShootStateDelay = 0;

            if (isRandomOdd())
            {
                state = AiStates.goingToCover;
                agent.isStopped = false;
            }
            else
            {
                SetStateNewShootingPos();
            }
        }


    }


    /// <summary>
    /// State Going to Cover
    /// </summary>
    
    void StateGoingToCover()
    {
        if (!goingToCover) {
            
            var t = CoverWayPointManager.Instance.GetPos(otherAi.GetComponent<Runner>().id);
            agent.SetDestination(t.position);
            goingToCover = true;
            agent.isStopped = false;
        }
        else
        {
            if (NavMeshGetPathRemainingDistance(agent) < .9f)
            {
                state = AiStates.onCover;
                goingToCover = false;
                agent.isStopped = true;
            }
        }
    }

    /// <summary>
    /// State when Ai in Cover
    /// </summary>
    void StateOnCover()
    {
        ShootOnCover();
    }


   /// <summary>
   /// Shooting and Hiding from Cover
   /// </summary>
    void ShootOnCover()
    {
        if (isCoverStand) {
            if (T_CoverDelay < coverDelay)
                T_CoverDelay += Time.deltaTime;
        }
        else
        {
            if (T_CoverDelay > 0 )
                T_CoverDelay -= Time.deltaTime;
        }

        var d = Vector3.Distance(transform.position, otherAi.position) * 2;

        if (T_CoverDelay >= coverDelay && isCoverStand)
        { 
            isCoverStand = false;

            if (d > 70f) {

                if (isRandomOdd())
                {
                    SetGoingToCoverState();
                }
                else
                {
                    SetStateNewShootingPos();
                }
                return;
            }
          
        }

        if (!isCoverStand && T_CoverDelay <= 0f)
        {
            isCoverStand = true;

            if (d > 70f)
            {
                if (isRandomOdd())
                {
                    SetGoingToCoverState();
                }
                else
                {
                    SetStateNewShootingPos();
                }
                return;
            }
        }
       

        if (isCoverStand)
        {
            GetComponents<Collider>()[0].enabled = true;
            Shoot();
            SetAnimCover(false);
        }
        else
        {
            GetComponents<Collider>()[0].enabled = false;
            SetAnimCover(true);
        }



        if (T_CoverStateDelay < coverStateDelay)
            T_CoverStateDelay += Time.deltaTime;

        if (T_CoverStateDelay >= coverStateDelay)
        {
            T_CoverStateDelay = 0f;

            if (isRandomOdd())
            {
                SetGoingToCoverState();
            }
            else
            {
                SetStateNewShootingPos();
            }

        }



        CheckEnemySeen();      

    }

    /// <summary>
    /// Check if the Enemy is direct Seen or Not
    /// </summary>
    void CheckEnemySeen()
    {
        if (CoverWayPointManager.Instance.IsEnemySeen(otherAi.GetComponent<Runner>().id, transform))
        {
            if (isRandomOdd())
            {
                SetStateNewShootingPos();
            }
            else
            {
                SetGoingToCoverState();
              
            }
        }
    }
    /// <summary>
    /// Cover State Setting
    /// </summary>
    void SetGoingToCoverState()
    {
        SetAnimCover(false);
        state = AiStates.goingToCover;
        agent.isStopped = false;
        GetComponents<Collider>()[0].enabled = true;
    }

    private void LateUpdate()
    {
        base.Updates();
    }

 
   

    void ActiveDeactiveAll(GameObject[] objs,bool val)
    {
        foreach (GameObject obj in objs) {
        
            obj.SetActive(val);
        }
    }
 
    /// <summary>
    /// Collision Handling
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.StartsWith("Bullet"))
        {
            Destroy(other.gameObject);
            health.GetShoot();
        }
    }

    
}
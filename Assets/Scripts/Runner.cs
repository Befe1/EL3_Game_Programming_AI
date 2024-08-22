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
    [SerializeField] private List<Vector3> pathHistory = new List<Vector3>();
    [SerializeField] private List<List<Vector3>> historicalPaths = new List<List<Vector3>>();
    [SerializeField] private float similarityThreshold = 0.5f;  // Adjust as needed for path similarity
    [SerializeField] private float recordInterval = 1.0f;
    private float recordTimer;

    
    public bool IsDisguise = false;
    public AiId id;
   
    [SerializeField] Transform point;   
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float shootDelay = .5f;
    [SerializeField] float coverDelay = .5f;
    [SerializeField] float coverStateDelay = .5f;
    [SerializeField] float shootStateDelay = .5f;
    [SerializeField] private float rotationSpeed = 5; 
    [SerializeField] GunfireController gunFx;
    [SerializeField] GameObject spotLight;
    
    
    
    private Vector3 lastHitPosition;

   
    private float nextTurnTime;
    private Transform startTransform;

    public float multiplyBy;
    public float multiplyByMax;

    bool isGoingTowardBonus = false;
    float T_ShootDelay;
    float T_CoverDelay;
    float T_CoverStateDelay;
    float T_ShootStateDelay;
    private float alertedTime = 5; 
    private float currentAlertedTime;
    bool isAiDetect = false;
    bool isNewStatePos = false;
    bool goingToCover = false;
    bool isCoverStand = true;
    Transform otherAi;

    AiHealth health;
    /// <summary>
    /// Init and Ranomize some variable of Ai Bot
    /// </summary>
    /// 
    void Start()
    {      
        RandomPosition();

        coverDelay = Random.Range(2.5f,5f);
        shootStateDelay = Random.Range(2.5f, 5f);
        coverStateDelay = Random.Range(4.5f, 7.7f);

        health = GetComponent<AiHealth>();
        AiHealth.OnGetShootAi += HandleGetShot;

    }

     void OnDestroy()
    {
        AiHealth.OnGetShootAi -= HandleGetShot;
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
    
    private void SmoothRotateTowards(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// States Working and Distance Calcualtor
    /// </summary>
    void Update()
    { 
        GameManager.Instance.UpdateAiState(id, state);
        AiStates otherState = GameManager.Instance.GetAiState(id == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a);
        Debug.Log($"Current state of {id}: {state}, Other AI's state: {otherState}");
        float distance = GameManager.Instance.CalculateDistanceBetweenAIs();
        Debug.Log($"Distance between AI_a and AI_b: {distance}"); 


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
        else if (state == AiStates.alerted)
        {
            HandleAlertState();
        }
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            RecordCurrentPosition();
            recordTimer = 0;
        }
        if (distance < 10.0f && state != AiStates.searching && state != AiStates.alerted)
        {
            AdjustPosition();
        }
        DebugClosestCoverPoint();
    }
    private void RecordCurrentPosition()
    {
        if (pathHistory.Count == 0 || Vector3.Distance(pathHistory[pathHistory.Count - 1], transform.position) > 1.0f)
        {
            pathHistory.Add(transform.position);
            Debug.Log($"Recording position: {transform.position}");
        }
    }

     public void SavePath()
    {
        historicalPaths.Add(new List<Vector3>(pathHistory));
        pathHistory.Clear();
    }
    public bool IsFollowingSamePath()
    {
        foreach (var oldPath in historicalPaths)
        {
            if (ComparePaths(oldPath, pathHistory))
            {
                Debug.Log("AI is following a previously recorded path.");
                return true;
            }
        }
        Debug.Log("AI is not following any previously recorded paths.");
        return false;
    }

    private bool ComparePaths(List<Vector3> path1, List<Vector3> path2)
    {
        if (path1.Count != path2.Count)
            return false;

        for (int i = 0; i < path1.Count; i++)
        {
            if (Vector3.Distance(path1[i], path2[i]) > similarityThreshold)
                return false;
        }
        return true;
    }
    void AdjustPosition()
    {
        // Logic to move AI away from each other
        // This could involve setting a new destination point that increases their separation
        Vector3 awayDirection = transform.position - GameManager.Instance.GetAi(id == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a).position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position + awayDirection.normalized * 5.0f, out hit, 10.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        Debug.Log("Adjusting position to maintain distance.");
    }
    private void DebugClosestCoverPoint()
    {
        Transform closestWayPoint = CoverWayPointManager.Instance.GetClosestWayPoint(transform.position);
        if (closestWayPoint != null)
        {
            Debug.DrawLine(transform.position, closestWayPoint.position, Color.red);
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
        
        
        if (NavMeshGetPathRemainingDistance(agent) < 0.3f || agent.velocity.magnitude == 0)
        {
            Vector3 newPosition = RandomPosition();

            
            while (Vector3.Distance(newPosition, transform.position) < 15)
            {
                newPosition = RandomPosition();
            }

            agent.SetDestination(newPosition); 
            agent.isStopped = false; 
        }

        
        if (agent.velocity.sqrMagnitude > Mathf.Epsilon) 
        {
            SmoothRotateTowards(agent.destination); 
        }
        else
        {
            
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        Testing_T();   

           
        
    }
     void Testing_T()
    {
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
     private void HandleGetShot(AiId shooterId, int shooterHealth)
    {
        if (state == AiStates.searching)
        {
            Debug.Log("Alerted");
            state = AiStates.alerted;
            currentAlertedTime = alertedTime; // Reset the alert timer
        }
    }
    
    private void HandleAlertState()
    {
        // Increase ray distance when alerted
        if (currentAlertedTime == alertedTime) // Check if it's the initial call for alert state
        {
            SetRayDistance(25, 5); // Increase ray distance for 5 seconds
        }

        SmoothRotateTowards(lastHitPosition);

        // Alert state behavior
        Testing_T();

        // Decrement the alert timer
        currentAlertedTime -= Time.deltaTime;
        if (currentAlertedTime <= 0)
        {
            state = AiStates.searching;  // Transition back to searching after alert duration
            ResetRayDistance();  // Ensure ray distance is reset when exiting alert state
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
            lastHitPosition = other.transform.position;  // Store the hit position
            Destroy(other.gameObject);
            health.GetShoot();
        }
        
    }
    void OnDrawGizmos()
    {
        // Draw path history
        Gizmos.color = Color.blue;
        for (int i = 1; i < pathHistory.Count; i++)
        {
            Gizmos.DrawLine(pathHistory[i - 1], pathHistory[i]);
        }

        
        Gizmos.color = Color.green;
        foreach (var path in historicalPaths)
        {
            for (int i = 1; i < path.Count; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }

    
}

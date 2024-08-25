using System.Collections;
using System.Collections.Generic;
using BigRookGames.Weapons;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Ai Bot Controller
/// </summary>
public class Runner : AiFiniteStates
{
    private readonly float alertedTime = 5;
    [SerializeField] private float increasedRayDistance = 25f; // Increased ray distance during alert
    [SerializeField] private float defaultRayDistance = 18f; // Default ray distance
    private float currentRayDistance;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float coverDelay = .5f;
    [SerializeField] private float coverStateDelay = .5f;
   [SerializeField]private float minDistanceBetweenAIs = 20f;
    private float currentAlertedTime;
    private AiDecisionMaker decisionMaker;

    private Vector3 finalPosition;
    private bool goingToCover;

    [SerializeField] private GunfireController gunFx;

    private AiHealth health;
    [SerializeField] private readonly List<List<Vector3>> historicalPaths = new();
    public AiId id;
    private bool isCoverStand = true;


    public bool IsDisguise = false;
    private bool isNewStatePos;


    private Vector3 lastHitPosition;
    private Transform otherAi;
    [SerializeField] private readonly float pathFollowDelay = 5f;

    private float pathFollowTimer;
    [SerializeField] private readonly List<Vector3> pathHistory = new();

    [SerializeField] private Transform point;
    [SerializeField] private readonly float recordInterval = 1.0f;
    private float recordTimer;
    [SerializeField] private readonly float rotationSpeed = 5;
    [SerializeField] private readonly float shootDelay = .5f;
    [SerializeField] private float shootStateDelay = .5f;
    [SerializeField] private readonly float similarityThreshold = 1.5f;
    [SerializeField] private GameObject spotLight;
    private float T_CoverDelay;
    private float T_CoverStateDelay;

    //public float multiplyBy;
    //public float multiplyByMax;

    //bool isGoingTowardBonus = false;
    private float T_ShootDelay;
    private float T_ShootStateDelay;
    private float searchingTimer; // Timer for the searching state of this AI
    private bool isSpeedBoostActive = false; // Flag to track if speed boost is active
    private float originalSpeed; // To store the original NavMeshAgent speed


    /// <summary>
    ///     Init and Ranomize some variable of Ai Bot
    /// </summary>
    private void Start()
    {
        RandomPosition();
        currentRayDistance = defaultRayDistance; // Initialize with default ray distance

        coverDelay = Random.Range(2.5f, 5f);
        shootStateDelay = Random.Range(2.5f, 5f);
        coverStateDelay = Random.Range(4.5f, 7.7f);

        health = GetComponent<AiHealth>();
        AiHealth.OnGetShootAi += HandleGetShot;
        decisionMaker = GetComponent<AiDecisionMaker>();
        originalSpeed = agent.speed; 
        
    }

    private void OnDestroy()
    {
        AiHealth.OnGetShootAi -= HandleGetShot;
    }

    /// <summary>
    ///     Get the New Random Position
    /// </summary>
    /// <returns></returns>
    private Vector3 RandomPosition()
    {
        int maxAttempts = 10; // Maximum number of attempts to find a valid position
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomDirection = Random.insideUnitCircle * Random.Range(walkRadius, walkRadiusMax);
            randomDirection += new Vector2(transform.position.x, transform.position.z);

            var candidatePosition = new Vector3(randomDirection.x, transform.position.y, randomDirection.y);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(candidatePosition, out hit, walkRadiusMax, NavMesh.AllAreas))
            {
                Vector3 originalPosition = transform.position; // Store the original position
                transform.position = hit.position; // Temporarily move AI to the new position for distance calculation
                float distanceBetweenAIs = GameManager.Instance.CalculateDistanceBetweenAIs();
                transform.position = originalPosition; // Reset the AI's position

                if (distanceBetweenAIs >= minDistanceBetweenAIs)
                {
                    return hit.position; // Return this position only if it maintains the minimum distance
                }
            }
        }
    
    return transform.position;
    }

    private void SmoothRotateTowards(Vector3 targetPosition)
    {
        var directionToTarget = (targetPosition - transform.position).normalized;
        var desiredRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    ///     States Working and Distance Calcualtor
    /// </summary>
    private void Update()
    {
        GameManager.Instance.UpdateAiState(id, state);
        var otherState = GameManager.Instance.GetAiState(id == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a);
        //Debug.Log($"Current state of {id}: {state}, Other AI's state: {otherState}");
        var distance = GameManager.Instance.CalculateDistanceBetweenAIs();
        Debug.Log($"Distance between AI_a and AI_b: {distance}"); 

        if (T_ShootDelay < shootDelay)
            T_ShootDelay += Time.deltaTime;


        if (state == AiStates.searching)
            StateSearching();


        else if (state == AiStates.shooting)
            StateShoot(otherAi != null ? otherAi.GetComponent<Runner>().id : AiId.None);



        else if (state == AiStates.goingToCover)
            StateGoingToCover();

        else if (state == AiStates.onCover)
            StateOnCover();

        else if (state == AiStates.goingToNewShootingPos)
            StateNewShootingPos();
        else if (state == AiStates.alerted) HandleAlertState();
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            RecordCurrentPosition();
            recordTimer = 0;
        }

        if (distance < 10.0f && state != AiStates.searching && state != AiStates.alerted) AdjustPosition();
        DebugClosestCoverPoint();
    }

    private void RecordCurrentPosition()
    {
        if (pathHistory.Count == 0 || Vector3.Distance(pathHistory[pathHistory.Count - 1], transform.position) > 1.0f)
            pathHistory.Add(transform.position);
        //Debug.Log($"Recording position: {transform.position}");
    }

    public void SavePath()
    {
        historicalPaths.Add(new List<Vector3>(pathHistory));
        pathHistory.Clear();
    }

    public bool IsFollowingSamePath()
    {
        foreach (var oldPath in historicalPaths)
            if (ComparePaths(oldPath, pathHistory))
                //Debug.Log("AI is following a previously recorded path.");
                return true;
        //Debug.Log("AI is not following any previously recorded paths.");
        return false;
    }

    private bool ComparePaths(List<Vector3> path1, List<Vector3> path2)
    {
        if (Mathf.Abs(path1.Count - path2.Count) >
            similarityThreshold * 10) // Allow for some difference in path lengths
            return false;

        var cumulativeDifference = 0f;
        var compareLength = Mathf.Min(path1.Count, path2.Count);

        for (var i = 0; i < compareLength; i++) cumulativeDifference += Vector3.Distance(path1[i], path2[i]);

        var averageDifference = cumulativeDifference / compareLength;

        // Check if the average difference per point is within a defined similarity threshold
        return averageDifference < similarityThreshold;
    }

    private void AdjustPosition()
    {

        if (transform != null)
        {
            if (GameManager.Instance != null)
            {
                var awayDirection = transform.position -
                                    GameManager.Instance.GetAi(id == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a).position;
                NavMeshHit hit;
                if (transform != null && NavMesh.SamplePosition(transform.position + awayDirection.normalized * 5.0f, out hit, 10.0f,
                        NavMesh.AllAreas)) agent.SetDestination(hit.position);
            }
        }

        //Debug.Log("Adjusting position to maintain distance.");
    }


    /// <summary>
    ///     New State Position of Standing Shooting
    /// </summary>
    private void StateNewShootingPos()
    {
        print("State New Shoot Pos ...");

        agent.isStopped = false;

        if (!isNewStatePos)
        {
            var d = RandomPosition();

            while (Vector3.Distance(d, transform.position) < 15) d = RandomPosition();

            isNewStatePos = true;
            agent.SetDestination(finalPosition);
        }
        else if (NavMeshGetPathRemainingDistance(agent) < .9f)
        {
            print("End !! State New Shoot Pos ...");

            isNewStatePos = false;

            float distanceToOpponent = Vector3.Distance(transform.position, otherAi.position);

            if (distanceToOpponent < 10.0f && Random.value < 0.8f)
            {
                // Increase the likelihood of changing position when the opponent is nearby
                SetStateNewShootingPos();
            }
            else if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health))
            {
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
    ///     Set State of Standing Run Pos
    /// </summary>
    private void SetStateNewShootingPos()
    {
        GetComponents<Collider>()[0].enabled = true;
        agent.isStopped = true;
        SetAnimCover(false);
        state = AiStates.goingToNewShootingPos;
    }

    /// <summary>
    ///     State Searching of other Ai
    /// </summary>
    private void StateSearching()
    {
        searchingTimer += Time.deltaTime;
        Debug.Log($"AI {id} has spent {searchingTimer:F2} seconds in the searching state");
        if (searchingTimer > 10f && !isSpeedBoostActive)
        {
            StartCoroutine(BoostSpeedForLimitedTime(8f, 8f));
        }

        if (state != AiStates.searching)
        {
            pathFollowTimer = 0;
            SavePath();
        }


        pathFollowTimer += Time.deltaTime;
        recordTimer += Time.deltaTime;


        if (recordTimer >= recordInterval)
        {
            RecordCurrentPosition();
            recordTimer = 0;
        }


        if (pathFollowTimer < pathFollowDelay)
            if (IsFollowingSamePath())
            {
                Debug.Log("Attempting to follow a too similar path, recalculating...");
                var newPosition = RandomPosition(); // Force a new random position to avoid repetition
                agent.SetDestination(newPosition);
            }


        if (NavMeshGetPathRemainingDistance(agent) < 0.3f || agent.velocity.magnitude == 0)
        {
            var newPosition = RandomPosition();
            while (Vector3.Distance(newPosition, transform.position) < 15) newPosition = RandomPosition();
            agent.SetDestination(newPosition);
            agent.isStopped = false;
        }


        if (agent.velocity.sqrMagnitude > Mathf.Epsilon)
            SmoothRotateTowards(agent.destination);
        else
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        Testing_T();
    }
     private IEnumerator BoostSpeedForLimitedTime(float newSpeed, float duration)
    {
        isSpeedBoostActive = true;
        agent.speed = newSpeed;
        Debug.Log($"AI {id} speed boosted to {newSpeed} for {duration} seconds.");

        yield return new WaitForSeconds(duration);

        agent.speed = 8;
        isSpeedBoostActive = false;
        Debug.Log($"AI {id} speed reset to {originalSpeed}.");
    }

    private void Testing_T()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + new Vector3(0, 2, 0), transform.TransformDirection(Vector3.forward),
                out hit, currentRayDistance))
        {
            Debug.DrawRay(transform.position + new Vector3(0, 2, 0),
                transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);

            if (hit.collider.gameObject.name.StartsWith("Runner"))
            {
                Debug.Log("Found !!");
                spotLight.SetActive(false);
                otherAi = hit.collider.transform;

                if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health))
                    state = AiStates.shooting;
                else
                    state = AiStates.goingToCover;
            }
        }
        else
        {
            Debug.DrawRay(transform.position + new Vector3(0, 2, 0),
                transform.TransformDirection(Vector3.forward) * currentRayDistance, Color.blue);
        }
    }
    /// <summary>
    ///     Shoot the Rifle
    /// </summary>
    private void Shoot()
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
        if (currentAlertedTime == alertedTime)
        s_manager.Instance.PlaySound("Alert"); // Check if it's the initial call for alert state
            currentRayDistance = increasedRayDistance; // Increase ray distance
             

        SmoothRotateTowards(lastHitPosition);

        Testing_T();

        currentAlertedTime -= Time.deltaTime;
        if (currentAlertedTime <= 0)
        {
            state = AiStates.searching; // Transition back to searching after alert duration
            ResetRayDistance(); // Ensure ray distance is reset when exiting alert state
        }
    }

    /// <summary>
    ///     Stae of Stand Shoot
    /// </summary>
    private void StateShoot(AiId otherAiId)
    {
        agent.isStopped = true;
        Shoot();

        if (T_ShootStateDelay < shootStateDelay)
            T_ShootStateDelay += Time.deltaTime;

        if (T_ShootStateDelay >= shootStateDelay)
        {
            T_ShootStateDelay = 0;

            // Use IsEnemyInCover method with AiId directly
            bool opponentInCover = CoverWayPointManager.Instance.IsEnemyInCover(otherAiId);

            if (opponentInCover)
            {
                // Increase the likelihood of continuing to shoot
                if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health) || Random.value < 0.7f)
                {
                    state = AiStates.goingToCover;
                    agent.isStopped = false;
                }
                else
                {
                    SetStateNewShootingPos();
                }
            }
            else
            {
                if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health))
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
    }


    /// <summary>
    ///     State Going to Cover
    /// </summary>
    private void StateGoingToCover()
    {
        if (!goingToCover)
        {
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
    ///     State when Ai in Cover
    /// </summary>
    private void StateOnCover()
    {
        ShootOnCover();
    }


    /// <summary>
    ///     Shooting and Hiding from Cover
    /// </summary>
    private void ShootOnCover()
    {
        if (isCoverStand)
        {
            if (T_CoverDelay < coverDelay)
                T_CoverDelay += Time.deltaTime;
        }
        else
        {
            if (T_CoverDelay > 0)
                T_CoverDelay -= Time.deltaTime;
        }

        if (transform != null)
        {
            if (otherAi != null)
            {
                var d = Vector3.Distance(transform.position, otherAi.position) * 2;

                if (T_CoverDelay >= coverDelay && isCoverStand)
                {
                    isCoverStand = false;

                    if (d > 70f)
                    {
                        if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health) && !IsUnderFire())
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
                        if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health) && !IsUnderFire())
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
            }
        }

        if (isCoverStand)
        {
            GetComponents<Collider>()[0].enabled = true;
            Shoot();
            SetAnimCover(false);
        }
        else //StayOncOVER
        {
            GetComponents<Collider>()[0].enabled = false;
            SetAnimCover(true);
        }

        if (T_CoverStateDelay < coverStateDelay)
            T_CoverStateDelay += Time.deltaTime;

        if (T_CoverStateDelay >= coverStateDelay)
        {
            T_CoverStateDelay = 0f;

            if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health) && !IsUnderFire())
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

    private bool IsUnderFire()
    {
        // Implement a method to check if the AI is currently under fire
        // This could involve checking if the AI has been hit recently or detecting bullets flying near
        return false;
    }


    /// <summary>
    ///     Check if the Enemy is direct Seen or Not
    /// </summary>
    private void CheckEnemySeen()
    {
        if (transform != null && transform != null && CoverWayPointManager.Instance.IsEnemySeen(otherAi.GetComponent<Runner>().id, transform))
        {
            if (decisionMaker.MakeDecisionBasedOnState(state, id, health.health))
                SetStateNewShootingPos();
            else
                SetGoingToCoverState();
        }
    }

    /// <summary>
    ///     Cover State Setting
    /// </summary>
    private void SetGoingToCoverState()
    {
        SetAnimCover(false);
        state = AiStates.goingToCover;
        agent.isStopped = false;
        GetComponents<Collider>()[0].enabled = true;
    }

    private void LateUpdate()
    {
        Updates();
    }

    private void ActiveDeactiveAll(GameObject[] objs, bool val)
    {
        foreach (var obj in objs) obj.SetActive(val);
    }

    /// <summary>
    ///     Collision Handling
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.StartsWith("Bullet")|| other.GetComponent<Bullet>() != null)
        {
            lastHitPosition = other.transform.position; // Store the hit position
            Destroy(other.gameObject);
            health.GetShoot();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw path history
        Gizmos.color = Color.blue;
        for (var i = 1; i < pathHistory.Count; i++) Gizmos.DrawLine(pathHistory[i - 1], pathHistory[i]);


        Gizmos.color = Color.green;
        foreach (var path in historicalPaths)
            for (var i = 1; i < path.Count; i++)
                Gizmos.DrawLine(path[i - 1], path[i]);
    }

    private void DebugClosestCoverPoint()
    {
        var closestWayPoint = CoverWayPointManager.Instance.GetClosestWayPoint(transform.position);
        if (closestWayPoint != null) Debug.DrawLine(transform.position, closestWayPoint.position, Color.red);
    }
}

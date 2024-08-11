using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Runner : AiFiniteStates
{
    public AiId id;
    [SerializeField] Transform point;   
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float shootDelay = .5f;
    [SerializeField] float coverDelay = .5f;
    [SerializeField] float coverStateDelay = .5f;
    [SerializeField] float shootStateDelay = .5f;
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


    void Start()
    {
        
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
    void Update()
    {       


        if (T_ShootDelay < shootDelay)
            T_ShootDelay += Time.deltaTime;     



        if (state == AiStates.searching)
        {
            //StateSearching();
        }

        else if (state == AiStates.shooting)
        {
            //StateShoot();
        }


        else if (state == AiStates.goingToCover)
        {
            //StateGoingToCover();
        }

        else if (state == AiStates.onCover)
        {
            //StateOnCover();
        }

        else if (state == AiStates.goingToNewShootingPos)
        {
            //StateNewShootingPos();
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
                //SetGoingToCoverState();
            }
        }
    }
}

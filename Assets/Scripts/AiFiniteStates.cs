using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public enum AiStates
{
    searching,
    shooting,
    goingToCover,
    onCover,
    goingToNewShootingPos,

}
public class AiFiniteStates : MonoBehaviour
{
    public AiStates state=AiStates.searching;
    [SerializeField] protected int walkRadius;
    [SerializeField] protected int walkRadiusMax;
    protected NavMeshAgent agent;
    [SerializeField] protected int rayDistance;
    Animator anim;

     void Awake() 
     {
        agent=GetComponent<NavMeshAgent>();
        anim=GetComponent<Animator>();
        
    }
    /// <summary>
    /// Animation and Rotation of Agent
    /// </summary>
    protected void Updates()
    {

    }
    public void SetAnimCover(bool isCover)
    {
        //Code will be come 

    }
     public float NavMeshGetPathRemainingDistance(NavMeshAgent navMeshAgent)
    {
        if (navMeshAgent.pathPending ||
            navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
            navMeshAgent.path.corners.Length == 0)
            return 0f;

        float distance = 0.0f;
        for (int i = 0; i < navMeshAgent.path.corners.Length - 1; ++i)
        {
            distance += Vector3.Distance(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1]);
        }

        return distance;
    } 
    public bool isRandomOdd()
    {
        var r = UnityEngine.Random.Range(1, 99);
        return r % 2 != 0;
    }

}

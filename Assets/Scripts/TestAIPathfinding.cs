using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TestAIPathfinding : MonoBehaviour
{
    public Transform target;
    private Seeker seeker;
    private CharacterController controller;
    private Path path;
    private int currentWaypoint = 0;
    public float speed = 3;
    public float nextWaypointDistance = 1;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        seeker.StartPath(transform.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void Update()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            Debug.Log("End of path reached.");
            return;
        }

        Vector3 direction = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector3 move = direction * speed * Time.deltaTime;
        controller.SimpleMove(move);

        if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}

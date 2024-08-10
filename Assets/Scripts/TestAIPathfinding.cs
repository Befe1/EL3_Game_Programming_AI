using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TestAIPathfinding : MonoBehaviour
{
     public Transform targetObject; // The object to search for
    public float viewDistance = 10f; // Distance within which the character can see the object
    public float searchSpeed = 2f; // Speed at which the character moves while searching
    public float nextWaypointDistance = 1f; // Distance to the next waypoint before moving to the next one
    public float wallDetectionDistance = 2f; // Distance within which the character detects walls

    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool isSearching = false;
    private bool targetReached = false; // Indicates if the target object has been reached
    private Rigidbody rb; // Rigidbody component for movement

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody>();
        StartCoroutine(SearchRoutine()); // Start the search routine
    }

    IEnumerator SearchRoutine()
    {
        while (!targetReached) // Continue searching until the target is reached
        {
            UpdatePath();
            yield return new WaitForSeconds(0.5f); // Adjust the frequency of path updates as needed
        }
    }

    void UpdatePath()
    {
        if (isSearching || targetReached) return;

        // Check if the object is within line of sight
        RaycastHit hit;
        if (Vector3.Distance(transform.position, targetObject.position) <= viewDistance &&
            Physics.Raycast(transform.position, (targetObject.position - transform.position).normalized, out hit, viewDistance))
        {
            if (hit.transform == targetObject)
            {
                seeker.StartPath(transform.position, targetObject.position, OnPathComplete);
                return;
            }
        }

        // Continue searching
        Vector3 randomPoint = transform.position + new Vector3(Random.Range(-viewDistance, viewDistance), 0, Random.Range(-viewDistance, viewDistance));
        seeker.StartPath(transform.position, randomPoint, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            isSearching = true; // Mark that the character is now following a path
        }
    }

    void FixedUpdate()
    {
        if (path == null || targetReached)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            isSearching = false; // Allow new path to be generated
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector3 direction = ((Vector3)path.vectorPath[currentWaypoint] - transform.position).normalized;
        direction *= searchSpeed * Time.fixedDeltaTime;

        // Move the character using Rigidbody
        rb.MovePosition(transform.position + direction);

        float distance = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Check if the character has reached the target object
        if (Vector3.Distance(transform.position, targetObject.position) < nextWaypointDistance)
        {
            targetReached = true; // Stop the search and movement
        }

        DetectWalls();
    }

    void DetectWalls()
    {
        // Directions to cast rays
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        foreach (Vector3 dir in directions)
        {
            RaycastHit hit;
            // Cast a ray in the specified direction
            if (Physics.Raycast(transform.position, dir, out hit, wallDetectionDistance))
            {
                // Check if the hit object has a specific tag (e.g., "Wall")
                if (hit.collider.CompareTag("Wall"))
                {
                    Debug.Log($"Wall detected at distance: {hit.distance} in direction {dir}");
                }
            }
        }
    }
}

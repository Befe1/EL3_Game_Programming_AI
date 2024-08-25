using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverWayPointManager : MonoBehaviour
{
    public static CoverWayPointManager Instance { get; private set; }

    [SerializeField] Transform[] wayPoints;
    [SerializeField] private float coverDistanceThreshold = 10f; // Define a reasonable distance

    private void Awake()
    {
        if (Instance != null) Destroy(Instance);

        Instance = this;
    }

    // Method to get the closest waypoint
    public Transform GetClosestWayPoint(Vector3 position)
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Transform waypoint in wayPoints)
        {
            float distance = Vector3.Distance(position, waypoint.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = waypoint;
            }
        }
        return closest;
    }

    // Randomize the Positions of Cover
    void Shuffle(Transform[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            var r = UnityEngine.Random.Range(0, n);
            var t = array[r];
            array[r] = array[n];
            array[n] = t;
        }

        wayPoints = array;
    }

    // Get the Right Cover Position Transform
    public Transform GetPos(AiId ai)
    {
        Shuffle(wayPoints);

        Transform targetTransform = GameManager.Instance.GetAi(ai);
        if (targetTransform == null)
        {
            return null; // AI Transform not found
        }

        foreach (Transform t in wayPoints)
        {
            RaycastHit hit;
            Vector3 raycastDir = (targetTransform.position + new Vector3(0, .3f, 0)) - t.position;

            if (Physics.Raycast(t.position + new Vector3(0, .3f, 0), raycastDir, out hit, 1000))
            {
                var heading = (hit.point - t.position + new Vector3(0, .3f, 0));
                var distance = heading.magnitude;
                var direction = heading / distance;

                Color color = Color.red;

                if (hit.collider.gameObject.name.StartsWith("Runner"))
                {
                    color = Color.green;
                }
                else
                {
                    return t;
                }

                Debug.DrawRay(t.position + new Vector3(0, .3f, 0), direction * distance, color);
                Debug.Log("Did Hit => " + hit.collider.gameObject.name);
            }
            else
            {
                Debug.DrawRay(t.position + new Vector3(0, 0, 0), raycastDir * 1000, Color.black);
            }
        }

        return null;
    }

    // Detect if Enemy is Directly Seen from sight
    public bool IsEnemySeen(AiId enemyId, Transform ai)
    {
        Transform targetTransform = GameManager.Instance.GetAi(enemyId);
        if (targetTransform == null)
        {
            return false; // AI Transform not found
        }

        RaycastHit hit;
        Vector3 raycastDir = (targetTransform.position + new Vector3(0, .3f, 0)) - ai.position;

        if (Physics.Raycast(ai.position + new Vector3(0, .3f, 0), raycastDir, out hit, 1000))
        {
            var heading = (hit.point - ai.position + new Vector3(0, .3f, 0));
            var distance = heading.magnitude;
            var direction = heading / distance;

            Color color = Color.red;

            Debug.DrawRay(ai.position + new Vector3(0, .3f, 0), direction * distance, color);

            if (hit.collider.gameObject.name.StartsWith("Runner"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    // Check if the enemy is in cover
    public bool IsEnemyInCover(AiId enemyId)
    {
        Transform enemyTransform = GameManager.Instance.GetAi(enemyId);
        if (enemyTransform == null)
        {
            return false; // AI Transform not found
        }

        Transform coverPosition = GetPos(enemyId);
        if (coverPosition == null)
        {
            return false; // No cover position found
        }

        // Use the defined cover distance threshold
        return Vector3.Distance(enemyTransform.position, coverPosition.position) < coverDistanceThreshold;
    }

}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cover Random Location Provider
/// </summary>
public class CoverWayPointManager : MonoBehaviour
{
    public static CoverWayPointManager Instance { get; private set; }

    [SerializeField] Transform[] wayPoints;


    private void Awake()
    {
        if (Instance != null) Destroy(Instance);

        Instance = this;
    }

    void Start()
    {
        
    }

    /// <summary>
    /// Random the Positions of Cover
    /// </summary>
    /// <param name="array"></param>
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

    /// <summary>
    /// Get the Right Cover Position Transform
    /// </summary>
    /// <param name="ai"></param>
    /// <returns></returns>
    public Transform GetPos(AiId ai)
    {
        Shuffle(wayPoints);

        var targetAi = GameManager.Instance.GetAi(ai);

        foreach (Transform t in wayPoints)
        {


            RaycastHit hit;
            Vector3 raycastDir = (targetAi.position + new Vector3(0, .3f, 0)) - t.position;
           
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
               // Debug.Log("Did not Hit");
            }

        }


        return null;
    }

    /// <summary>
    /// Detect if Enemy is Direct Seen from sight
    /// </summary>
    /// <param name="enemyId"></param>
    /// <param name="ai"></param>
    /// <returns></returns>
    public bool IsEnemySeen(AiId enemyId,Transform ai)
    {
        var targetAi = GameManager.Instance.GetAi(enemyId);

        RaycastHit hit;
        Vector3 raycastDir = (targetAi.position + new Vector3(0, .3f, 0)) - ai.position;

        if (Physics.Raycast(ai.position + new Vector3(0, .3f, 0), raycastDir, out hit, 1000))
        {

            var heading = (hit.point - ai.position + new Vector3(0, .3f, 0));
            var distance = heading.magnitude;
            var direction = heading / distance;

            Color color = Color.red;


            Debug.DrawRay(ai.position + new Vector3(0, .3f, 0), direction * distance, color);

            Debug.Log("Did Hit => " + hit.collider.gameObject.name);

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

}

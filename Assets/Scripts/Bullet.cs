using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float trailDuration = 0.1f;
    [SerializeField] private LineRenderer lineRenderer;
    private Vector3 lastPosition;
    public GameObject hitParticlePrefab;
    public float speed = 1000f;  // Speed of the bullet
    public float maxDistance = 1000f;  // Max distance the bullet can travel before being destroyed

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;  // Width of the line renderer
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.red;
        
        lastPosition = transform.position;
        lineRenderer.SetPosition(0, lastPosition);
        lineRenderer.SetPosition(1, transform.position);

        // Auto-destroy the bullet after 5 seconds to prevent memory leaks
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = transform.forward;
        float step = speed * Time.deltaTime;

        // Update the bullet's position
        transform.Translate(Vector3.forward * step);

        // Perform a raycast from the last position to the current position
        RaycastHit hit;
        if (Physics.Raycast(lastPosition, direction, out hit, step))
        {
            if (hit.collider.CompareTag("Player"))  // Make sure to set your AI GameObjects with tag "AI"
            {
                // Instantiate hit particles if prefab is set
                if (hitParticlePrefab != null)
                {
                    Instantiate(hitParticlePrefab, hit.point, Quaternion.identity);
                }

                // Notify the GameManager that the AI was hit
                AiId hitAiId = hit.collider.GetComponent<Runner>().id;  // Assuming the Runner script contains the AI ID
                GameManager.Instance.UpdateAiState(hitAiId, AiStates.alerted);  // For example, set the AI to alerted state

                Destroy(gameObject);  // Destroy the bullet
                return;
            }
        }

        // Update positions for line renderer
        lineRenderer.SetPosition(0, lastPosition);
        lineRenderer.SetPosition(1, transform.position);
        lastPosition = currentPosition;
    }
        
    }
    
    


   


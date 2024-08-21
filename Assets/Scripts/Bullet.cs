using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public float trailDuration = 0.1f;  
    [SerializeField]private LineRenderer lineRenderer;
    private Vector3 lastPosition;
    public GameObject hitParticlePrefab;
    
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;  // Adjust the width of the trail
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Default material
        lineRenderer.startColor = Color.yellow;  // Adjust the trail color
        lineRenderer.endColor = Color.red;

        // Set the initial positions for the line renderer
        lastPosition = transform.position;
        lineRenderer.SetPosition(0, lastPosition);
        lineRenderer.SetPosition(1, transform.position);
        
        /// Auto Destroy
        Destroy(gameObject,5f);
    }

    public float speed = 1000;

    /// <summary>
    /// Bullet Movement
    /// </summary>
    private void Update()
    {       
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
        
        lineRenderer.SetPosition(0, lastPosition);
        lineRenderer.SetPosition(1, transform.position);
        lastPosition = transform.position;
        
    }
    
    


   
}

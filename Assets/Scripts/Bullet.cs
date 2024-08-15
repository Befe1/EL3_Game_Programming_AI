using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
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
    }


   
}

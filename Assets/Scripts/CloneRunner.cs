using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneRunner : MonoBehaviour
{
    [SerializeField]
    private float speed = 177;
    public GameObject explosionEffect;
    void Start()
    {
        Invoke(nameof(ActiveCollider),.3f);
        Destroy(gameObject, 5f);
    }

    void ActiveCollider()
    {
        GetComponent<Collider>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;

        // Update the Clone 
        transform.Translate(Vector3.forward * step);
    }

   


    void Explode()
    {
        s_manager.Instance.PlaySound("Explosion");
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

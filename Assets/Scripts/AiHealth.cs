using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiHealth : MonoBehaviour
{

   public delegate void GetShootAi(AiId id, int health);
    public static event GetShootAi OnGetShootAi;

    public int health = 100;

    [SerializeField] private Runner runner;
    private AiId id;

    void Start()
    {
        id = runner.id;
    }

    public void GetShoot()
    {
        health -= 2;
        OnGetShootAi?.Invoke(id, health);
        if (health <= 0) Destroy(gameObject);
    }
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiHealth : MonoBehaviour
{
    public delegate void GetShootAi(AiId id,int health);
    public static event GetShootAi OnGetShootAi;

    public int health = 100;

    [SerializeField] Runner runner;

    AiId id;
    void Start()
    {
        id = runner.id;        
    }
    /// <summary>
    /// Handling Bullet Hit
    /// </summary>
    public void GetShoot()
    {
        health -=2;
        OnGetShootAi(id,health);
    }
}

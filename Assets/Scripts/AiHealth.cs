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

    public void GetShoot(BulletType bulletType)
    {
        if (bulletType == BulletType.stun) return; 

        if(bulletType == BulletType.bullet)
        health -= 2;
        else if (bulletType == BulletType.rapidBullet)
        health -= 1;
        else if (bulletType == BulletType.sniper)
            health -= 5;
        else if (bulletType == BulletType.clone)
            health -= 10;

        OnGetShootAi?.Invoke(id, health);
        if (health <= 0) Destroy(gameObject);
    }
   
}

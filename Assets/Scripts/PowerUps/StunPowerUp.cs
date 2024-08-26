using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunPowerUp : MonoBehaviour, IPowerUp
{
    [SerializeField]
    float coolDownTime;
    [SerializeField]
    Runner ai;

    [SerializeField] private readonly float shootDelay = .27f;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletSpawnPoint;

    public bool IsActive { get => isActive; set => isActive = value; }
    public bool IsReady { get => isReady; set => isReady = value; }


    private bool isActive = false;
    private bool isReady = false;
    private float T_ShootDelay;
    private int totalBulletShoots = 0;
    private float T_CoolDownTime = 0f;


    public void Activate()
    {
        if (IsReady)
        {
            Debug.Log("Start Stun ...");
            IsActive = true;
            totalBulletShoots = 0;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        IsReady = false;
        T_CoolDownTime = coolDownTime;
        ai.PowerupDeactive();

        Debug.Log("Finish Stun ...");
    }


    void Stun()
    {
        T_ShootDelay += Time.deltaTime;

        ai.transform.LookAt(ai.GetOtherAi);

        if (T_ShootDelay >= shootDelay)
        {
            T_ShootDelay = 0;
            var b = Instantiate(bullet, bulletSpawnPoint.position, ai.transform.rotation);
            b.GetComponent<Bullet>().SetStunBullet();
            totalBulletShoots++;
            print("Power Up Stun Shooting ...");

            if (totalBulletShoots > 3)
            {
                Deactivate();
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive)
        {
            Stun();
        }
        else
        {
            if (T_CoolDownTime > 0f)
            {
                T_CoolDownTime -= Time.deltaTime;
            }
            else
            {
                IsReady = true;
            }

        }
    }
}

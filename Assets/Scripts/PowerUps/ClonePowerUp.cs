using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClonePowerUp : MonoBehaviour, IPowerUp
{
    [SerializeField]
    float coolDownTime;
    [SerializeField]
    Runner ai;
    [SerializeField] private readonly float shootDelay = .27f;
    [SerializeField] private GameObject clone;
    


    public bool IsActive { get => isActive; set => isActive = value; }
    public bool IsReady { get => isReady; set => isReady = value; }

    private float T_ShootDelay;
    private int totalBulletShoots = 0;
    private float T_CoolDownTime = 0f;
    private bool isActive = false;
    private bool isReady = false;

    public void Activate()
    {
        if (IsReady)
        {
            Debug.Log("Start Clone ...");
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

        Debug.Log("Finish Clone Powerup ...");
    }


    void CloneShoot()
    {
        T_ShootDelay += Time.deltaTime;

        ai.transform.LookAt(ai.GetOtherAi);

        if (T_ShootDelay >= shootDelay)
        {
            T_ShootDelay = 0;
            var b = Instantiate(clone, ai.transform.position, ai.transform.rotation);
            
            totalBulletShoots++;
            print("Power Up Rapid Shooting ...");

            if (totalBulletShoots > 0)
            {
                Deactivate();
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    void Update()
    {
        if (IsActive)
        {
            CloneShoot();
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


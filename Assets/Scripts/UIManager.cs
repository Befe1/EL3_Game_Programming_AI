using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Image imgAi_a;
    [SerializeField] Image imgAi_b;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] TextMeshProUGUI txtWonAi;

    private void Awake()
    {
        if (Instance != null) Destroy(Instance);

        Instance = this;
    }
    void Start()
    {
        
    }
    /// <summary>
    /// Get the Bullet Hit event from Ai Bot when Bullet Hit
    /// </summary>
    /// <param name="id"></param>
    /// <param name="health"></param>
    private void AiHealth_OnGetShootAi(AiId id, int health )
    {
        if (id == AiId.Ai_a)
        {
            imgAi_a.fillAmount = ((float)health / 100f);
        }
        else
        {
            imgAi_b.fillAmount = ((float)health / 100f);
        }

        if(health <= 0)
        {
            if (id == AiId.Ai_a) txtWonAi.text = "Ai Blue Won !";
            else txtWonAi.text = "Ai Red Won !";

            gameOverUI.SetActive(true);
        }
    }

    private void OnEnable()
    {
        AiHealth.OnGetShootAi += AiHealth_OnGetShootAi;
    }

  

    private void OnDisable()
    {
        AiHealth.OnGetShootAi -= AiHealth_OnGetShootAi;
    }


}

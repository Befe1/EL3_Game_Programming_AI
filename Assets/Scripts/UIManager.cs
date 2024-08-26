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
    [SerializeField] private GameObject exclamationAi_a; // Exclamation mark for Ai_a
    [SerializeField] private GameObject exclamationAi_b; // Exclamation mark for Ai_b
    [SerializeField] Runner Ai_a;
    [SerializeField] Runner Ai_b;


    private void Awake()
    {
        if (Instance != null) Destroy(Instance);

        Instance = this;
    }
    void Start()
    {
        exclamationAi_a.SetActive(false);
        exclamationAi_b.SetActive(false);
    }
    private void Update()
    {
        // Check current states of AIs and update UI accordingly
        UpdateExclamation(AiId.Ai_a, exclamationAi_a);
        UpdateExclamation(AiId.Ai_b, exclamationAi_b);
    }

    private void UpdateExclamation(AiId aiId, GameObject exclamationImage)
    {
        AiStates currentState = GameManager.Instance.GetAiState(aiId);
        if (currentState == AiStates.alerted)
        {
            if (!exclamationImage.activeSelf)
            {
                exclamationImage.SetActive(true);
            }
        }
        else
        {
            if (exclamationImage.activeSelf)
            {
                exclamationImage.SetActive(false);
            }
        }
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
     

    public void DebugStartFight()
    {
        Ai_a.DebugStartFight(Ai_b.transform);
        Ai_b.DebugStartFight(Ai_a.transform);
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

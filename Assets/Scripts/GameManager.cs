using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ai Ids Enum
/// </summary>
public enum AiId
{
    Ai_a,
    Ai_b
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Transform Ai_a;
    [SerializeField] Transform Ai_b;
   
    private void Awake()
    {
        
           if(Instance != null) Destroy(Instance);      
       
            Instance = this;
        
    }   

    /// <summary>
    /// Get the Specific Ai from Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Transform GetAi(AiId id)
    {
        if (id == AiId.Ai_a) return Ai_a;
        else return Ai_b;
    }
    /// <summary>
    /// Calculate the distance between two specified AIs
    /// </summary>
    /// <returns>Distance between Ai_a and Ai_b</returns>
    public float CalculateDistanceBetweenAIs()
    {
        if (Ai_a != null && Ai_b != null)
        {
            return Vector3.Distance(Ai_a.position, Ai_b.position);
        }
        else
        {
            Debug.LogError("One or both AI Transforms are not set.");
            return -1; // Indicates an error
        }
    }

    /// <summary>
    /// Restart the Game
    /// </summary>
    public void GameRestart()
    {
        SceneManager.LoadScene(0);
    }
    
}

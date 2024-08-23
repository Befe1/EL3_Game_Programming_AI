using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionMaker : MonoBehaviour
{
    public bool MakeDecisionBasedOnState(AiStates currentState, AiId currentAiId ,int currentHealth)
    {
         AiStates otherState = GameManager.Instance.GetAiState(currentAiId == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a);
        float probability = 0.5f;

        // Adjusting decision sensitivity and adding strategic depth
        switch (currentState)
        {
            case AiStates.shooting:
                if (otherState == AiStates.goingToCover || otherState == AiStates.onCover)
                {
                    // Only change position if really needed (less sensitive)
                    probability = currentHealth < 50 ? 0.8f : 0.6f;
                }
                break;
            case AiStates.onCover:
                // Stay in cover unless there's a clear advantage to move
                probability = otherState == AiStates.searching ? 0.3f : 0.1f;
                break;
        }


        return UnityEngine.Random.value < probability;
    }
    
}

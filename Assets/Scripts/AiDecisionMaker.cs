using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDecisionMaker : MonoBehaviour
{
   public bool MakeDecisionBasedOnState(AiStates currentState, AiId currentAiId, int currentHealth)
    {
        AiStates otherState = GameManager.Instance.GetAiState(currentAiId == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a);
        float probability = 0.5f; // Default decision probability

        // Adjusting decision sensitivity and adding strategic depth
        switch (currentState)
        {
            case AiStates.shooting:
                if (otherState == AiStates.goingToCover || otherState == AiStates.onCover)
                {
                    // More aggressive if the AI is healthier; more cautious if health is low
                    probability = currentHealth < 50 ? 0.8f : 0.6f;
                }
                break;

            case AiStates.onCover:
                // More likely to stay in cover if health is low or the opponent is not actively searching
                probability = currentHealth < 50 ? 0.2f : (otherState == AiStates.searching ? 0.3f : 0.1f);
                break;

            case AiStates.goingToCover:
                // If already moving to cover, continue with high probability if health is low
                probability = currentHealth < 50 ? 0.9f : 0.7f;
                break;

            default:
                // Default probability adjustment based on health
                probability = currentHealth < 50 ? 0.7f : 0.5f;
                break;
        }

        // Random decision based on the calculated probability
        return UnityEngine.Random.value < probability;
    }

}


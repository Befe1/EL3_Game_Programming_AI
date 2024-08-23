using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionMaker : MonoBehaviour
{
    public bool MakeDecisionBasedOnState(AiStates currentState, AiId currentAiId)
    {
        AiStates otherState = GameManager.Instance.GetAiState(currentAiId == AiId.Ai_a ? AiId.Ai_b : AiId.Ai_a);
        float probability = 0.5f; // Default probability for making a decision

        switch (currentState)
        {
            case AiStates.shooting:
                if (otherState == AiStates.goingToCover || otherState == AiStates.onCover)
                    probability = 0.8f; // More likely to continue shooting if opponent is in cover
                break;
            case AiStates.searching:
                if (otherState == AiStates.shooting)
                    probability = 0.3f; // Less likely to leave cover if opponent is shooting
                break;
            case AiStates.onCover:
                if (otherState == AiStates.searching)
                    probability = 0.7f; // More likely to leave cover if opponent is searching
                break;
        }

        return UnityEngine.Random.value < probability;
    }
}

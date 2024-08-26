using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSound : MonoBehaviour
{

   private bool isSuspensionPlaying = false;
    private bool isHeartbeatPlaying = false;

    void Update()
    {
       AiStates stateAI_A = GameManager.Instance.GetAiState(AiId.Ai_a);
        AiStates stateAI_B = GameManager.Instance.GetAiState(AiId.Ai_b);

        // Check if both AIs are in the searching state
        if (stateAI_A == AiStates.searching && stateAI_B == AiStates.searching)
        {
            if (!isSuspensionPlaying)
            {
                s_manager.Instance.PlaySound("Night");
                s_manager.Instance.PlaySound("Wind");
                isSuspensionPlaying = true;
                // Ensure Heartbeat is stopped if it was playing
                if (isHeartbeatPlaying)
                {
                    s_manager.Instance.StopSound("Heartbeat");
                    isHeartbeatPlaying = false;
                }
            }
        }
        else if ((stateAI_A == AiStates.searching && stateAI_B != AiStates.searching) ||
                 (stateAI_A != AiStates.searching && stateAI_B == AiStates.searching))
        {
            // If only one AI is searching, play Heartbeat
            if (!isHeartbeatPlaying)
            {
                s_manager.Instance.PlaySound("Heartbeat");
                isHeartbeatPlaying = true;
                // Stop Suspension and Wind if they were playing
                if (isSuspensionPlaying)
                {
                    s_manager.Instance.StopSound("Night");
                    s_manager.Instance.StopSound("Wind");
                    isSuspensionPlaying = false;
                }
            }
        }
        else
        {
            // Stop all sounds if none of the above conditions are met
            if (isSuspensionPlaying)
            {
                s_manager.Instance.StopSound("Night");
                s_manager.Instance.StopSound("Wind");
                isSuspensionPlaying = false;
            }
            if (isHeartbeatPlaying)
            {
                s_manager.Instance.StopSound("Heartbeat");
                isHeartbeatPlaying = false;
            }
        }
    }

    }


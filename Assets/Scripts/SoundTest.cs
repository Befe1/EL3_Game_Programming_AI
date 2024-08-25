using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    private void Start()
    {
        if (s_manager.Instance != null)
        {
            s_manager.Instance.PlaySound("Alert");
        }
        else
        {
            Debug.LogError("SoundManager Instance is null!");
        }
    }
}

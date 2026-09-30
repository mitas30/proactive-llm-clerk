using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIControllerForSpeechAI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI aiResponseTextUI;

    public void UpdateDisplayText(string text)
    {
        if (aiResponseTextUI != null)
        {
            aiResponseTextUI.text = text;
        }
    }
}

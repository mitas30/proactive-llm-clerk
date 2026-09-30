using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDes : MonoBehaviour
{
    [SerializeField] float delayTime = 5f;
    [SerializeField] GameObject startPanel;
    private void Start()
    {
        StartCoroutine(Delay(delayTime));
    }

    IEnumerator Delay(float time)
    {
        yield return new WaitForSeconds(delayTime);
        startPanel.SetActive(true);
    }
}

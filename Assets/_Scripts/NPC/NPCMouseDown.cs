using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class NPCMouseDown : MonoBehaviour
{

    [SerializeField] GameObject questPanel;
    public GameObject[] artistQuestPanel;
    public AudioClip questAC;
    public AudioSource au;
    private void Start()
    {

    }
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        questPanel.SetActive(true);
        AudioMG.st.Play(questAC);
    }

    public void OnButtonDown(int index)
    {

        questPanel.SetActive(false);
        artistQuestPanel[index].SetActive(true);
    }

    public void Return1()
    {
        foreach (var item in artistQuestPanel)
        {
            item.SetActive(false);
        }
        questPanel.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawCoin : MonoBehaviour,IPointerClickHandler
{
    public int drawCion;
    public int drawId;
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
       //ShopPanel.instance.curDrawCoin = drawCion;
       // ShopPanel.instance.curDrawIndex = drawId;
       // ShopPanel.instance.ShowDraw(drawId);
    }

}

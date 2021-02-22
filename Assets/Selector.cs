using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Selector : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Image selectionEffect;

    public Image standardSprite;
    

    public void OnSelect(BaseEventData eventData){
        if(standardSprite != null){ standardSprite.enabled = false; }
        selectionEffect.enabled = true;
    }

    public void OnDeselect(BaseEventData eventData){
        if(standardSprite != null){ standardSprite.enabled = true; }
        selectionEffect.enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorDevice : CircuitDevice
{
    public Color offColor;
    public Color onColor;
    public Image iconImage;

    void Awake(){
        iconImage.color = offColor;
    }

    public void SetStatus(bool status){
        iconImage.color = status ? onColor : offColor;
    }
}

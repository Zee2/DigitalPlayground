using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverDevice : CircuitDevice
{
    public Lever leverGate;
    public void ToggleLever(){
        leverGate.ToggleLever();
    }
}

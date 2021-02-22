using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

public class ShiftRegister : Primitive
{
    bool lastClock;
    private int length = 4;
    public ShiftRegister(string name, int length){
        this.length = length;
        logicType = LogicType.ShiftRegister4;
        inputs = new List<Net> { null, null };

        outputs = new List<Net>();
        stagedOutputs = new List<bool>();
        for(int i = 0; i < length; i++){
            outputs.Add(null);
            stagedOutputs.Add(false);
        }
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {

            // Rising edge detection
            if(inputs[0].value && !lastClock){
                // Shift and latch in new data
                Shift();
                stagedOutputs[0] = inputs[1].value;
            }
            lastClock = inputs[0].value;
        }
    }

    private void Shift(){
        for(int i = length-1; i > 0; i--){
            stagedOutputs[i] = stagedOutputs[i-1];
        }
    }
}
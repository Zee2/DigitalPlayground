using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

public class DFF : Primitive
{
    [SerializeField]
    bool lastClock;
    public DFF(string name){

        logicType = LogicType.DFF;
        inputs = new List<Net> { null, null };

        outputs = new List<Net> { null, null };
        stagedOutputs = new List<bool>{ false, true };
        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        if(inputs[0] != null && inputs[1] != null) {

            // Rising edge detection
            if(inputs[0].value && !lastClock){
                // Latch new data
                stagedOutputs[0] = inputs[1].value;
                stagedOutputs[1] = !inputs[1].value;
            }
            lastClock = inputs[0].value;
        }
    }
}
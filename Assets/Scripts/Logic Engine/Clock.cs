using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

public class Clock : Primitive, ITickable {

    public ulong period = 20000;

    private ulong lastTick = 0;

    private bool desiredValue = false;

    public Clock(string name){
        logicType = Utils.LogicType.Clock;
        inputs = new List<Net> { };
        outputs = new List<Net> { null };
        stagedOutputs = new List<bool> { false };
        this.name = name;
        guid = Guid.NewGuid();
    }

    public void Tick(Queue<LogicEvent> parentEventQueue){
        if(LogicServer.ticks - lastTick > period / 2) {
            desiredValue = !desiredValue;
            hostCircuit.NotifyGateChange(guid);
            lastTick = LogicServer.ticks;
        }
    }

    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        
        stagedOutputs[0] = desiredValue;
        //Debug.Log("Computing clock, staged output value = " + stagedOutputs[0]);
    }
}
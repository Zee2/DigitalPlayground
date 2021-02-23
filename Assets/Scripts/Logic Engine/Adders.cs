using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

public class Adder : Primitive
{
    [SerializeField]
    private int length = 1;
    public Adder(string name, int length){

        // TODO: Variable width components
        Debug.Assert(length == 1);

        this.length = length;
        logicType = LogicType.FA1;
        inputs = new List<Net> { null, null, null };
        outputs = new List<Net>();
        stagedOutputs = new List<bool>();
        for(int i = 0; i < length + 1; i++){
            outputs.Add(null);
            stagedOutputs.Add(false);
        }

        Debug.Assert(outputs.Count == length + 1);

        this.name = name;
        guid = Guid.NewGuid();
    }
    public override void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue){
        
        bool a =  (inputs[0]?.value ?? false);
        bool b = (inputs[1]?.value ?? false);
        bool carryIn = inputs[2]?.value ?? false;
        bool inputXor = a ^ b;

        stagedOutputs[0] = inputXor ^ carryIn;
        stagedOutputs[1] = (a && b) || (inputXor && carryIn);
    }
}
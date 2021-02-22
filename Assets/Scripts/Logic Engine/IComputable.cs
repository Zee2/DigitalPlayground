using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IComputable
{
    Vector2 position {get; set;}
    Guid guid {get; set;}
    void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue);
}

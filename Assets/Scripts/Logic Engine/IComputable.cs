using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IComputable
{
    void Compute(Queue<Guid> parentGateQueue, Queue<LogicEvent> parentEventQueue);
}

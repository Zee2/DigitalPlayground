using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ITickable
{
    void Tick(Queue<LogicEvent> parentEventQueue);
}
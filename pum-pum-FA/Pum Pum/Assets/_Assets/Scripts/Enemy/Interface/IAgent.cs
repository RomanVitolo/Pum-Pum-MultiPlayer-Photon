using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IAgent
{
    //int maxHealth { get; }
    UnityEvent OnDie { get; set; }
    UnityEvent OnGetHit { get; set; }
}
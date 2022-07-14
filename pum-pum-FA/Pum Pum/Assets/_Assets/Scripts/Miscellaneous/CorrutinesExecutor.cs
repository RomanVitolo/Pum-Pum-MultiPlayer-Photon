using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrutinesExecutor : MonoBehaviour
{
    public static CorrutinesExecutor Instance;

    void Start()
    {
        Instance = this;
    }
}

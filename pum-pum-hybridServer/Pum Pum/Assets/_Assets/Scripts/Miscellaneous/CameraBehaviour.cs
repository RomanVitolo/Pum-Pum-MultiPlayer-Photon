using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    private Quaternion _myRotation;

    private void Start()
    {
        _myRotation = Quaternion.Euler(0, 0, 0);
    }

    private void Update()
    {
        transform.rotation = _myRotation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Camera _myCamera = null;
    private Quaternion _myRotation;
    private Transform _target;

    public Camera MyCamera { get => _myCamera; set => _myCamera = value; }

    private void Start()
    {
        _myRotation = Quaternion.Euler(0, 0, 0);
    }

    private void Update()
    {
        if (_target == null) return;
        var position = _target.transform.position;
        position.z = _myCamera.transform.position.z;

        transform.position = position;
        //transform.rotation = _myRotation;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        //this.transform.parent = target;
        //this.transform.localPosition = Vector3.zero;
    }
}

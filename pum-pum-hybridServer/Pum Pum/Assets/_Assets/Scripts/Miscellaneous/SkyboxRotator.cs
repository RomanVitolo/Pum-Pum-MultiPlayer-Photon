using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 2f;

    private void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * _rotationSpeed);
    }
}

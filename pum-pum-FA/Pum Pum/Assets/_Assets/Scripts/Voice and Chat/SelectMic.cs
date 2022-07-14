using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Voice.Unity;
using Photon.Voice.PUN;

public class SelectMic : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private Recorder _recorder;
    

    private void Start()
    {
        string[] devices = Microphone.devices;
        if (devices.Length != 0)
        {
            List<string> mic = new List<string>();
            for (int i = 0; i < devices.Length; i++)
            {
                mic.Add(devices[i]);
            }
            _dropdown.AddOptions(mic);
            SetMic(0);
        }
    }

    public void SetMic(int i)
    {
        string[] devices = Microphone.devices;
        if (devices.Length > i)
        {
            _recorder.UnityMicrophoneDevice = devices[i];
        }
    }
}

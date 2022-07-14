using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
public class UIMic : MonoBehaviour
{
    [SerializeField] private Sprite micOn;
    [SerializeField] private Sprite micOff;
    [SerializeField] private Image micImage;

    private Recorder _recorder;

    private void Start()
    {
        _recorder = PhotonVoiceNetwork.Instance.PrimaryRecorder;
    }

    private void Update()
    {
        OnMicChange();
    }

    public void OnMicChange()
    {
        if(_recorder.TransmitEnabled)
        {
            micImage.sprite = micOn;
        }
        else
        {
            micImage.sprite = micOff;
        }
    }
}

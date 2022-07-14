using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInGameHudsData : MonoBehaviourPun
{
    [Header("---- References ----")]
    [SerializeField] private PlayerModel _playerModelRef = null;
    [SerializeField] private ClockTimer _playerLocalClockTimer = null;
    //[SerializeField] private int timerValue = 180;
    //private WaveSpawner _waveSpawner = null;
    private int _playerBullets = 0;
    
    [Header("---- Huds References ----")]
    [SerializeField] private GameObject _canvasStruct = null;
    [SerializeField] private TextMeshProUGUI _lifeText = null;
    [SerializeField] private TextMeshProUGUI _roomName = null;

    public ClockTimer PlayerLocalClockTimer { get => _playerLocalClockTimer; set => _playerLocalClockTimer = value; }

    private void Start()
    {
        _roomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
    }
    
    private void FixedUpdate()
    {
        UpdateLifeHud();
        //UpdateWaveCount();
    }

    public void ShowUi()
    {
        _canvasStruct.SetActive(true);
    }

    public void UpdateAllGUI(float life, float time, float startCountdownTime, float targetTime)
    {
        //print($"Updating all gui ::: life = {life} , bullets = {bullets} , waveCount = {waveCount} , time = {time} , startCountdownTime = {startCountdownTime} , targetTime = {targetTime}");

        _playerModelRef.CurrentLife = life;
        UpdateLifeHud();

        _playerLocalClockTimer.ClockTime = time;
        _playerLocalClockTimer.SetTimerCountdownParameters(startCountdownTime, targetTime);
    }

    private void UpdateLifeHud()
    {
        _lifeText.text = _playerModelRef.CurrentLife.ToString();
    }

    //private void UpdateWaveCount()
    //{
    //    _waveCountText.text = "Wave: " + _waveSpawner.waveCount.ToString();
    //}

    //private void CountDownTimer()
    //{
    //    if (timerValue > 0)
    //    {
    //        TimeSpan spanTime = TimeSpan.FromSeconds(timerValue);
    //        _showTimer.text = "Time Left : " + spanTime.Minutes + " : " + spanTime.Seconds;
    //        timerValue--;
    //        Invoke(nameof(CountDownTimer), 1f);
    //    }
    //    else
    //    {
    //        _showTimer.text = "Game Over";
    //        //Ir al GameplayManager y cargar la condicion de derrota.
    //    }
    //}
}

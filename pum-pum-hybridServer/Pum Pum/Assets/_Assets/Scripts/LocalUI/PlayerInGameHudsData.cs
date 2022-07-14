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
    private int _playerBullets = 0;

    [Header("---- Huds References ----")]
    [SerializeField] private GameObject _canvasStruct = null;
    [SerializeField] private TextMeshProUGUI _lifeText = null;
    [SerializeField] private TextMeshProUGUI _bulletsText = null;
    [SerializeField] private TextMeshProUGUI _roomName = null;

    private void Start()
    {
        if (!photonView.IsMine) Destroy(_canvasStruct);//Make sure there is only my local canvas on screen
        _roomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
    }

    private void Update()
    {
        UpdateLifeHud();
        UpdateBulletHud();
    }

    private void UpdateLifeHud()
    {
        _lifeText.text = _playerModelRef.CurrentLife.ToString();
    }

    private void UpdateBulletHud()
    {
        _playerBullets = (int)_playerModelRef.CurrentWeapon.CurrentAvailableBullets;
        _bulletsText.text = _playerBullets.ToString();
    }
}

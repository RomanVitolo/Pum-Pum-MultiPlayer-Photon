using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private float _secondsToStart = 2;
    [SerializeField] private int _numberOfPlayers;

    private ServerManager _server = null;
    private Player _localPlayer = null;

    private PlayerModel _playerModel = null;
    #region Private Methods

    private void Start()
    {
        _server = GameObject.FindObjectOfType<ServerManager>();
        _localPlayer = PhotonNetwork.LocalPlayer;

        var foundPlayerModel = FindObjectsOfType<PlayerModel>();
        _playerModel = foundPlayerModel[0];
        if (_playerModel != null)
        {
            _playerModel.OnPlayerDie += OnPlayerDieHandler;
        }

        _numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    private void CheckPlayersToStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount >= _numberOfPlayers)
            {
                StartCoroutine(WaitToStart());
            }
        }
    }

    private void OnPlayerDieHandler()
    {
        print("Se ejecuta OnPlayerDieHandler del gameplayManager");
        _server.photonView.RPC("OnPlayerDieNotification", RpcTarget.All, _localPlayer);
        _playerModel.MakePlayerGhost();
    }

    #endregion
    #region Public Methods

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckPlayersToStart();
    }

    #endregion
    #region Coroutines
    IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(_secondsToStart);

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }
    #endregion
}


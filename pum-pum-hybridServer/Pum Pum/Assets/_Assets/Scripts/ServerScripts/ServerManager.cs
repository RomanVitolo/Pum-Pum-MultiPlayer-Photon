using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerManager : MonoBehaviourPun
{
    [Header("---- Server Parameters ----")]
    [SerializeField] private string _folderPrefabsPath = "";
    [SerializeField] private GameObject _playerPrefab = null;
    [SerializeField] private SpawnPointsManager _spawnPointsManager = null;
    [SerializeField] private int _ghostModeLayerNumber = 10;
    private int _curretPlayersAlive = 0;
    private Player _server;
    private Dictionary<Player, PlayerModel> _charactersDic = new Dictionary<Player, PlayerModel>(); //Dictionary used to save the player Model of each player
    private List<int> _deadPlayersIds = new List<int>();

    public Player GetPlayerServer => _server;

    private void Start()
    {
        _server = PhotonNetwork.MasterClient;
        _curretPlayersAlive = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    [PunRPC]
    public void InitializedPlayer(Player player) //RPC Called by PlayerController
    {
        var spawnPos = _spawnPointsManager.GetSpawnPosition();
        GameObject obj = PhotonNetwork.Instantiate(_folderPrefabsPath + "/" + _playerPrefab.name, spawnPos, Quaternion.Euler(0, 0, 0));

        PlayerModel playerModel = obj.GetComponentInChildren<PlayerModel>();
        _charactersDic[player] = playerModel;
        int ID = playerModel.photonView.ViewID;

        photonView.RPC("RequestRegisterPlayer", RpcTarget.Others, player, ID);
    }

    [PunRPC]
    public void RequestRegisterPlayer(Player client, int ID)
    {
        PhotonView pv = PhotonView.Find(ID);
        if (pv == null) return;
        var character = pv.GetComponent<PlayerModel>();
        if (character == null) return;
        _charactersDic[client] = character;
    }

    [PunRPC]
    public void RequestGetPlayer(Player client) //RPC Called by PlayerController
    {
        if (_charactersDic.ContainsKey(client)) //If my dictionary has that player, return that player
        {
            var character = _charactersDic[client];
            int characterID = character.photonView.ViewID;
            photonView.RPC("SetPlayer", client, characterID);
        }
    }

    [PunRPC]
    public void SetPlayer(int playerID) //RPC Called by Server Manager
    {
        PhotonView photonView = PhotonView.Find(playerID);
        if (photonView == null) return;

        var model = photonView.GetComponent<PlayerModel>();
        if (model == null) return;

        var controller = GameObject.FindObjectOfType<PlayerController>();
        //controller.SetPlayerController = character;
        controller.SetPlayerModel = model;
    }

    [PunRPC]
    public void RequestShoot(Player client) //RPC Called by PlayerController
    {
        if (_charactersDic.ContainsKey(client))
        {
            _charactersDic[client].Shoot();
            _charactersDic[client].CurrentWeapon.ExecuteCoolDown = true;
        }
    }

    [PunRPC]
    public void RequestBullets(Player client, int bullets) //RPC Called by player controller
    {
        if (_charactersDic.ContainsKey(client))
        {
            _charactersDic[client].CurrentWeapon.CurrentAvailableBullets = bullets;
        }
    }

    [PunRPC]
    public void OnPlayerDieNotification(Player client)
    {
        if (_charactersDic.ContainsKey(client))
        {
            var incomingDeadPlayerID = _charactersDic[client].photonView.ViewID;
            if (_deadPlayersIds.Contains(incomingDeadPlayerID)) return;

            _deadPlayersIds.Add(incomingDeadPlayerID);
            ManageGameplayChange(client);
        }
    }

    [PunRPC]
    public void ExecuteVictoryCelebration()
    {
        foreach (var player in _charactersDic)
        {
            player.Value.MakePlayerWinner();
        }
    }

    private void ManageGameplayChange(Player playerKilled)
    {
        _charactersDic[playerKilled].CurrentWeapon.CurrentAvailableBullets = 0;
        _charactersDic[playerKilled].MyPlayerSR.color = new Color(1, 1, 1, 0.5f);
        _curretPlayersAlive--;
        Debug.LogWarning($"_curretPlayersAlive = {_curretPlayersAlive}");
        if (_curretPlayersAlive == 1)
        {
            Debug.LogWarning($"Executing Victory because _curretPlayersAlive= {_curretPlayersAlive}");

            photonView.RPC("ExecuteVictoryCelebration", RpcTarget.Others);
            StartCoroutine(DelayToDisconnect());
        }
    }

    IEnumerator DelayToDisconnect()
    {
        yield return new WaitForSeconds(5f);
        foreach (var player in _charactersDic)
        {
            player.Value.photonView.RPC("DisconnectPlayer", RpcTarget.All, "Menu");
        }

        PhotonNetwork.Disconnect();
    }
}

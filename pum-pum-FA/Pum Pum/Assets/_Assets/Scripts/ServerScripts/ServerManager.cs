using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Linq;
using Photon.Voice.Unity;

public class ServerManager : MonoBehaviourPun
{
    [Header("---- Server Parameters ----")]
    [SerializeField] private string _folderPrefabsPath = "";
    [SerializeField] private string _menuSceneName = "Menu_PumVsDead";
    [SerializeField] private GameObject _playerPrefab = null;
    [SerializeField] private SpawnPointsManager _spawnPointsManager = null;

    [Header("---- Timer Parameters ----")]
    [SerializeField] private ClockTimer _serverTimer = null;
    [SerializeField] private float _startTimeInSeconds = 120f;

    [Header("---- Camera Parameters ----")]
    [SerializeField] private CameraBehaviour _camera = null;

    [Header("---- Gameplay Parameters ----")]
    [SerializeField] private float _speedIncrease = 4;
    [SerializeField] private float _lifeIncreasePercentage = 0.2f;

    [Header("---- Spawner Parameters ----")]
    [SerializeField] private PowerUpSpawner _powerUpSpawner = null;
    [SerializeField] private float _boostedPowerUpSpawnInterval = 2f;

    private bool _shouldUpdateGUI = true ;
    private bool _shouldCheckVictoryCondition = true;
    private int _curretPlayersAlive = 0;
    private Player _server;
    private Photon.Voice.Unity.Recorder _recorder;
    private Dictionary<Player, PlayerModel> _charactersDic = new Dictionary<Player, PlayerModel>(); //Dictionary used to save the player Model of each player
    private Dictionary<PlayerModel, Player> _playersModelsDic = new Dictionary<PlayerModel, Player>(); 
    private List<PlayerModel> _deadPlayersModels = new List<PlayerModel>();
    private List<PlayerModel> _alivePlayersModels = new List<PlayerModel>();
    //---- Cheat Codes ----
    private Dictionary<string, bool> _cheatCodes = new Dictionary<string, bool>();

    public Player GetPlayerServer => _server;
    public Dictionary<PlayerModel, Player> PlayersModelsDic { get => _playersModelsDic; }
    public Recorder Recorder { get => _recorder; set => _recorder = value; }

    private void Start()
    {
        _server = PhotonNetwork.MasterClient;
        _curretPlayersAlive = PhotonNetwork.CurrentRoom.PlayerCount;
        _curretPlayersAlive--; //Remove server from count
        _serverTimer.SetTimerCountdownParameters(_startTimeInSeconds, 0);
        _recorder = GameObject.FindObjectOfType<Photon.Voice.Unity.Recorder>();

        FillCheatDictionary();
    }

    private void Update()
    {
        if (_shouldCheckVictoryCondition) StartCoroutine(CheckVictoryConditionWDelay());
    }

    [PunRPC]
    public void InitializedPlayer(Player player) //RPC Called by PlayerController
    {
        var spawnPos = _spawnPointsManager.GetSpawnPosition();
        GameObject obj = PhotonNetwork.Instantiate(_folderPrefabsPath + "/" + _playerPrefab.name, spawnPos, Quaternion.Euler(0, 0, 0));

        PlayerModel playerModel = obj.GetComponentInChildren<PlayerModel>();
        _charactersDic[player] = playerModel;
        _playersModelsDic[playerModel] = player;
        _alivePlayersModels.Add(playerModel);

        int ID = playerModel.photonView.ViewID;

        photonView.RPC("RequestRegisterPlayer", RpcTarget.Others, player, ID);
        photonView.RPC("SetCamera", player, ID);
        playerModel.photonView.RPC("SetCanvas", player);
        if(_recorder != null) playerModel.photonView.RPC("InitClientMic", player);
    }

    [PunRPC]
    public void RequestRegisterPlayer(Player client, int ID)
    {
        PhotonView pv = PhotonView.Find(ID);
        if (pv == null) return;
        var character = pv.GetComponent<PlayerModel>();
        if (character == null) return;
        _charactersDic[client] = character;
        _playersModelsDic[character] = client;
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
        controller.SetPlayerModel = model;
    }

    [PunRPC]
    void SetCamera(int id) //RPC Called by serverManager
    {
        var photonView = PhotonView.Find(id);
        if (photonView == null) return;
        Transform target = photonView.gameObject.transform;
        _camera.SetTarget(target);
    }

    [PunRPC]
    void RequestMove(Player client, Vector3 dir) //RPC Called by PlayerController
    {
        if (_charactersDic.ContainsKey(client))
        {
            _charactersDic[client].Move(dir);
        }
    }

    [PunRPC]
    public void RequestLife(Player client) //Called by player model
    {
        if (_charactersDic.ContainsKey(client))
        {
            var newIncreaseLifeValue = _charactersDic[client].CurrentLife * _lifeIncreasePercentage;
            var newLifeAmount = _charactersDic[client].CurrentLife + (int)newIncreaseLifeValue;
            _charactersDic[client].CurrentLife = (int)newLifeAmount;

            var newScale = _charactersDic[client].transform.localScale + new Vector3(0.5f, 0.5f, 0);
            _charactersDic[client].ScalePlayer(newScale);
            _charactersDic[client].photonView.RPC("ChangePlayerMagnitude", client, newLifeAmount, newScale);
            _charactersDic[client].photonView.RPC("ChangePlayerInWorldLifeText", client, newLifeAmount);// --> testear
        }
    }

    [PunRPC]
    public void RequestSpeed(Player client, bool lifeTimeChange = false) //Called by player model
    {
        if (_charactersDic.ContainsKey(client))
        {
            StartCoroutine(ChangePlayerSpeed(client, lifeTimeChange));
        }
    }

    [PunRPC]
    public void RequestEatPlayer(Player client, Player collidedClient) //Called by player model
    {
        if (!_charactersDic.ContainsKey(client)) return;
        var clientModel = _charactersDic[client];
        var collidedClientModel = _charactersDic[collidedClient];

        if (clientModel.CurrentLife <= collidedClientModel.CurrentLife || _deadPlayersModels.Contains(collidedClientModel) || _deadPlayersModels.Contains(clientModel)) return;

        collidedClientModel.KillPlayer();
        collidedClientModel.photonView.RPC("KillPlayer", collidedClient);

        var newIncreaseLifeValue = clientModel.CurrentLife + collidedClientModel.CurrentLife;
        clientModel.CurrentLife = (int)newIncreaseLifeValue;

        var scalesDifferences = clientModel.transform.localScale + (clientModel.transform.localScale - collidedClientModel.transform.localScale);
        var newScale = clientModel.transform.localScale + scalesDifferences;
        clientModel.ScalePlayer(newScale);
        clientModel.photonView.RPC("ChangePlayerMagnitude", client, newIncreaseLifeValue, newScale);
    }

    [PunRPC]
    public void SyncGUIRequest(Player client) //Called by player controller
    {
        if (_charactersDic.ContainsKey(client))
        {
            photonView.RPC("UpdateGUIRequest", client, client);
            _charactersDic[client].photonView.RPC("ChangePlayerInWorldLifeText", RpcTarget.All, _charactersDic[client].CurrentLife); // --> testear
        }
    }

    [PunRPC]
    public void UpdateGUIRequest(Player client) //RPC Called by serverManager
    {
        var life = _charactersDic[client].CurrentLife;
        var currTime = _serverTimer.ClockTime;
        var startCountdownTime = _startTimeInSeconds;

        _charactersDic[client].photonView.RPC("UpdateGUIData", client, life, currTime, startCountdownTime, 0f);
    }

    [PunRPC]
    public void OnPlayerDieNotification(PlayerModel clientModel) //RPC Called by player Model
    {
        if (_playersModelsDic.ContainsKey(clientModel))
        {
            if (_deadPlayersModels.Contains(clientModel)) return;

            _deadPlayersModels.Add(clientModel);
            ManageGameplayChange(_playersModelsDic[clientModel]);
        }
    }

    [PunRPC]
    public void RequestCheat(string cheatCode) //RPC Called by Chat Manager
    {
        if (!_cheatCodes.ContainsKey(cheatCode)) return;

        if (cheatCode == _cheatCodes.ElementAt(0).Key)  //Health cheat
        {
            print("LIFEUP Cheat executing...");
            foreach (var alivePlayer in _alivePlayersModels)
            {
                var newScale = alivePlayer.transform.localScale + new Vector3(2, 2, 0);
                alivePlayer.photonView.RPC("ChangePlayerMagnitude", RpcTarget.All, alivePlayer.CurrentLife + 200, newScale);
            }
        }
        else if (cheatCode == _cheatCodes.ElementAt(1).Key) //Speed cheat
        {
            print("SPEEDUP Cheat executing...");
            foreach (var alivePlayer in _alivePlayersModels) StartCoroutine(ChangePlayerSpeed(_playersModelsDic[alivePlayer], false));
        }
        else if (cheatCode == _cheatCodes.ElementAt(2).Key)
        {
            print("SPAWNUP Cheat executing...");
            _powerUpSpawner.BoostPowerUpInterval(_boostedPowerUpSpawnInterval);
        }
    }

    private void ManageGameplayChange(Player playerKilled)
    {
        _charactersDic[playerKilled].photonView.RPC("MakePlayerGhost", RpcTarget.All);
        _curretPlayersAlive--;
        _alivePlayersModels.Remove(_charactersDic[playerKilled]);

        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (_serverTimer.ClockTime <= 0.01f || _curretPlayersAlive == 1)
        {
            foreach (var deadPlayer in _deadPlayersModels)
            {
                deadPlayer.photonView.RPC("MakePlayerLooser", RpcTarget.All);
            }

            foreach (var alivePlayer in _alivePlayersModels)
            {
                alivePlayer.photonView.RPC("MakePlayerWinner", RpcTarget.All);
            }

            StartCoroutine(DelayToDisconnect());
        }
    }

    [PunRPC]
    private void Disconnect() //RPC Called by serverManager (DelayToDisconnect())
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(_menuSceneName);
    }

    private void FillCheatDictionary()
    {
        _cheatCodes.Add("LIFEUP", false); //+200 life cheatcode
        _cheatCodes.Add("SPEEDUP", false); //X2 speed cheatcode
        _cheatCodes.Add("SPAWNUP", false); //Boost power up spawn rate cheatcode
    }

    IEnumerator CheckVictoryConditionWDelay()
    {
        _shouldCheckVictoryCondition = false;
        yield return new WaitForSeconds(1f);
        CheckVictoryCondition();
        _shouldCheckVictoryCondition = true;
    }

    IEnumerator DelayToDisconnect()
    {
        yield return new WaitForSeconds(5f);
        photonView.RPC("Disconnect", RpcTarget.Others);

        yield return new WaitForSeconds(0.2f);
        photonView.RPC("Disconnect", _server);
    }

    IEnumerator ChangePlayerSpeed(Player client, bool lifetimeChange = false)
    {
        var newSpeed = _charactersDic[client].MovementSpeed + _speedIncrease;
        _charactersDic[client].MovementSpeed = newSpeed;
        _charactersDic[client].photonView.RPC("ChangePlayerSpeed", client, newSpeed);

        if (!lifetimeChange)
        {
            yield return new WaitForSeconds(2f);

            newSpeed = _charactersDic[client].MovementSpeed - _speedIncrease;
            _charactersDic[client].MovementSpeed = newSpeed;
            _charactersDic[client].photonView.RPC("ChangePlayerSpeed", client, newSpeed);
        }
    }
}

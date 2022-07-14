using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPun
{
    [Header("Server")]
    [SerializeField] private ServerManager _server = null;
    private Player _localPlayer = null;
    private Recorder _recorder;
    private bool _isLocked;

    private PlayerModel _myPlayerModel = null;
    public Player LocalPlayer { get => _localPlayer; set => _localPlayer = value; }

    private void Start()
    {

        //if (!PhotonNetwork.IsMasterClient)
        //{
        //    _server = GameObject.FindObjectOfType<ServerManager>();
        //    _localPlayer = PhotonNetwork.LocalPlayer;

        //    _server.photonView.RPC("InitializedPlayer", _localPlayer, _localPlayer); //The player itself executes the ConnectPlayerToServer that instantiates the player  
        //    _server.photonView.RPC("RequestGetPlayer", _localPlayer, _localPlayer); //The player itself executes RequestGetPlayer that assing a value to local _playerController variable. Because serve instantiate the prefab without PlayerController class and then creates it and assign it

        //    var chatManager = FindObjectOfType<ChatManager>();
        //    if (chatManager)
        //    {
        //        chatManager.OnSelect += Lock;
        //        chatManager.OnDeselect += UnLock;
        //    }

        //    _recorder = PhotonVoiceNetwork.Instance.PrimaryRecorder;
        //    _myPlayerModel.OnPickedPowerUp += OnPickedPowerUpHandler;
        //}
        //else
        //{
        //    Destroy(this);
        //}

        _server = GameObject.FindObjectOfType<ServerManager>();
        _localPlayer = PhotonNetwork.LocalPlayer;

        _server.photonView.RPC("InitializedPlayer", _localPlayer, _localPlayer); //The player itself executes the ConnectPlayerToServer that instantiates the player  
        _server.photonView.RPC("RequestGetPlayer", _localPlayer, _localPlayer); //The player itself executes RequestGetPlayer that assing a value to local _playerController variable. Because serve instantiate the prefab without PlayerController class and then creates it and assign it

        var chatManager = FindObjectOfType<ChatManager>();
        if (chatManager)
        {
            chatManager.OnSelect += Lock;
            chatManager.OnDeselect += UnLock;
        }

        _recorder = PhotonVoiceNetwork.Instance.PrimaryRecorder;
        _myPlayerModel.OnPickedPowerUp += OnPickedPowerUpHandler;
        _myPlayerModel.OnPlayerDie += OnPlayerDieHandler;
    }

    void Lock()
    {
        _isLocked = true;
    }

    void UnLock()
    {
        _isLocked = false;
    }

    private void Update()
    {
        if (Input.GetButton("Fire1")) //--> REMOTO - SERVER
        {
            _server.photonView.RPC("RequestShoot", _server.GetPlayerServer, _localPlayer);
        }

        if (Input.GetButtonDown("Dash")) // --> LOCAL
        {
            _myPlayerModel.Dash();
        }

        if (_recorder != null)
        {
            if (Input.GetKey(KeyCode.V))
            {
                _recorder.TransmitEnabled = true;
            }
            else
            {
                _recorder.TransmitEnabled = false;
            }
        }

    }

    private void FixedUpdate()
    {
        _myPlayerModel.LookAtMouse(); //Input == MousePosition
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) _myPlayerModel.Move(); //Input == W-A-S-D --> LOCAL
    }

    public PlayerModel SetPlayerModel
    {
        set
        {
            _myPlayerModel = value;
        }
    }

    private void OnPickedPowerUpHandler(int bullets)
    {
        _server.photonView.RPC("RequestBullets", _server.GetPlayerServer, _localPlayer, bullets);
    }

    private void OnPlayerDieHandler()
    {
        _server.photonView.RPC("OnPlayerDieNotification", _server.GetPlayerServer, _localPlayer);
    }
}

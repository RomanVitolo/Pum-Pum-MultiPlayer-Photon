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
    [Header("-- Server --")]
    [SerializeField] private ServerManager _server = null;
    private Player _localPlayer = null;
    private Recorder _recorder;
    private Camera _camera;
    private bool _isLocked;
    private bool _shouldSyncServerData = true;
    private PlayerModel _myPlayerModel = null;
    public Player LocalPlayer { get => _localPlayer; set => _localPlayer = value; }

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient) Destroy(this);
    }

    private void Start()
    {
        _server = GameObject.FindObjectOfType<ServerManager>();
        _myPlayerModel = GameObject.FindObjectOfType<PlayerModel>();
        _localPlayer = PhotonNetwork.LocalPlayer;
        _camera = Camera.main;

        _server.photonView.RPC("InitializedPlayer", _server.GetPlayerServer, _localPlayer); //The player itself executes the ConnectPlayerToServer that instantiates the player  
        _server.photonView.RPC("RequestGetPlayer", _server.GetPlayerServer, _localPlayer); //The player itself executes RequestGetPlayer that assing a value to local _playerController variable. Because serve instantiate the prefab without PlayerController class and then creates it and assign it

        var chatManager = FindObjectOfType<ChatManager>();
        if (chatManager)
        {
            chatManager.OnSelect += Lock;
            chatManager.OnDeselect += UnLock;
        }

        _recorder = PhotonVoiceNetwork.Instance.PrimaryRecorder;
        _server.photonView.RPC("SyncGUIRequest", _server.GetPlayerServer, _localPlayer);
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

        if (_shouldSyncServerData)
        {
            StartCoroutine(SyncServerData());
        }
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    public PlayerModel SetPlayerModel
    {
        set
        {
            _myPlayerModel = value;
        }
    }

    private void OnPlayerDieHandler()
    {
        _server.photonView.RPC("OnPlayerDieNotification", _server.GetPlayerServer, _localPlayer);
    }

    private void PlayerMovement()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            float xMovement = Input.GetAxisRaw("Horizontal");
            float yMovement = Input.GetAxisRaw("Vertical");
            var newDir = new Vector3(xMovement, yMovement, 0).normalized;
            _server.photonView.RPC("RequestMove", _server.GetPlayerServer, _localPlayer, newDir);
        }
    }

    IEnumerator SyncServerData()
    {
        _shouldSyncServerData = false;
        yield return new WaitForSeconds(10f);
        _server.photonView.RPC("SyncGUIRequest", _server.GetPlayerServer, _localPlayer);
        _shouldSyncServerData = true;
    }
}
    
    


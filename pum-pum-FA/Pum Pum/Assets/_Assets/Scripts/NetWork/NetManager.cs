using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Voice.PUN;
using Photon.Realtime;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using TMPro;

public class NetManager : MonoBehaviourPunCallbacks
{
    [Header("Players")]
    [SerializeField] private int _playersToStart = 4;
    [SerializeField] private string _sceneName = "Gameplay";

    [Header("GUI Parameters")]
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _refreshConnectButton;
    [SerializeField] private GameObject backgroundMenuImage;
    [SerializeField] private TMP_InputField _inputFieldRoom;
    [SerializeField] private TextMeshProUGUI _waitingForPlayersTxt;
    [SerializeField] private RoomList _roomItemPrefab;
    [SerializeField] private Transform _roomContent;

    [Header("Voice Parameters")]
    [SerializeField] private GameObject _playersToStartGameSlider;
    //[SerializeField] private Slider _playersToStartGameSlider;
    //[SerializeField] private TextMeshProUGUI _currentSelectedPlayers; 

    List<RoomList> _listRoomItems = new List<RoomList>();

    private int _playersCount;
    private bool _loadingLevel = false;
    
    void Start()
    {
        _refreshConnectButton?.onClick.AddListener(TryConnect);
        TryConnect();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        //print("Connected to lobby");
        _connectButton.interactable = true;
    }
    
    public void Connect()
    {
        _connectButton.interactable = false;
        if (string.IsNullOrWhiteSpace(_inputFieldRoom.text) || string.IsNullOrEmpty(_inputFieldRoom.text)) return;

        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.IsVisible = true;
        options.MaxPlayers = (byte)_playersToStart;
        PhotonNetwork.JoinOrCreateRoom(_inputFieldRoom.text, options, TypedLobby.Default);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _connectButton.interactable = true;
        backgroundMenuImage.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        _connectButton.interactable = true;
        backgroundMenuImage.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        _connectButton.interactable = false;
        backgroundMenuImage.SetActive(true);

        if (_playersCount != _playersToStart)
        {
            _waitingForPlayersTxt.text = "WAITING FOR PLAYERS...";
        }

    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            _playersCount = PhotonNetwork.CurrentRoom.PlayerCount;
        }

        if (_playersCount >= _playersToStart && !_loadingLevel)
        {
            _loadingLevel = true;
            StartCoroutine(LoadLevelWithDelay(1f));
        }
    }

    IEnumerator LoadLevelWithDelay(float delay)
    {
        _waitingForPlayersTxt.text = "MATCH!";
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.CurrentRoom.IsVisible)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        SceneManager.LoadScene(_sceneName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        try
        {
            UpdateRoomList(roomList);
        }
        catch
        {
            Debug.LogWarning("Error updating list");
        }
        
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        for (int i = _listRoomItems.Count - 1; i >= 0; i++)
        {
            Destroy(_listRoomItems[i].gameObject);
        }

        _listRoomItems.Clear();

        for (int i = 0; i < roomList.Count; i++)
        {
            var currentRoom = roomList[i];
            var roomItem = Instantiate(_roomItemPrefab, _roomContent);
            if (roomItem == null) return;

            roomItem.roomList.text = "Room : " + currentRoom.Name + " | Players:" + currentRoom.PlayerCount + "/" + currentRoom.MaxPlayers;
            roomItem._button.onClick.AddListener(() => ConnectToRoom(currentRoom.Name));
            _listRoomItems.Add(roomItem);
            Debug.Log("" + currentRoom.MaxPlayers);
        }
    }

    private void ConnectToRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    private void TryConnect()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
        _connectButton.interactable = false;
        backgroundMenuImage.SetActive(false);
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using TMPro;

public class NetManager : MonoBehaviourPunCallbacks
{
    [Header("Players")]
    [SerializeField] private int _playersToStart = 4;

    [Header("GUI Parameters")]
    [SerializeField] private Button button;
    [SerializeField] private GameObject backgroundMenuImage;
    [SerializeField] private TMP_InputField _inputFieldRoom;
    [SerializeField] private TextMeshProUGUI _waitingForPlayersTxt;
    [SerializeField] private RoomList _roomItemPrefab;
    [SerializeField] private Transform _roomContent;
    //[SerializeField] private Slider _playersToStartGameSlider;
    //[SerializeField] private TextMeshProUGUI _currentSelectedPlayers; 

    List<RoomList> _listRoomItems = new List<RoomList>();

    private int _playersCount;
    private bool _loadingLevel = false;
    
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        button.interactable = false;
        backgroundMenuImage.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("Connected to lobby");
        button.interactable = true;
    }
    
    public void Connect()
    {
        button.interactable = false;
        if (string.IsNullOrWhiteSpace(_inputFieldRoom.text) || string.IsNullOrEmpty(_inputFieldRoom.text)) return;

        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.IsVisible = true;
        options.MaxPlayers = (byte)_playersToStart;
        PhotonNetwork.JoinOrCreateRoom(_inputFieldRoom.text, options, TypedLobby.Default);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        button.interactable = true;
        backgroundMenuImage.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        button.interactable = true;
        backgroundMenuImage.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        button.interactable = false;
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

        SceneManager.LoadScene("Gameplay");
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
            print("el objeto instanciado es = " + roomItem);
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
}


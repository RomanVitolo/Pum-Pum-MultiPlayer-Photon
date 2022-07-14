using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;
using TMPro;
using System;

public class ChatManager : MonoBehaviour, IChatClientListener
{
   public TextMeshProUGUI content;
    public TMP_InputField inputField;
    public ScrollRect scroll;
    public Action OnSelect = delegate { };
    public Action OnDeselect = delegate { };
    public int maxLines;
    ChatClient _chatClient;
    string[] _channels;
    string[] _chats;
    Dictionary<string, int> _chatDic = new Dictionary<string, int>();
    int _currentChat;
    float _limitScrollAutomation = 0.2f;
    
    void Start()
    {
        if (!PhotonNetwork.IsConnected) return;
        _channels = new string[] { "World", PhotonNetwork.CurrentRoom.Name };
        _chats = new string[2];
        _chatDic["World"] = 0;
        _chatDic[PhotonNetwork.CurrentRoom.Name] = 1;
        _chatClient = new ChatClient(this);
        _chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));
    }
    void Update()
    {
        _chatClient.Service();
    }
    void UpdateChatUI()
    {
        content.text = _chats[_currentChat];
        if (content.textInfo.lineCount >= maxLines)
        {
            StartCoroutine(WaitToDeleteLine());
        }
        if (scroll.verticalNormalizedPosition < _limitScrollAutomation)
        {
            StartCoroutine(WaitToScroll());
        }
    }
    IEnumerator WaitToScroll()
    {
        yield return new WaitForEndOfFrame();
        scroll.verticalNormalizedPosition = 0;
    }
    IEnumerator WaitToDeleteLine()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < content.textInfo.lineCount - maxLines; i++)
        {
            var index = _chats[_currentChat].IndexOf("\n");
            _chats[_currentChat] = _chats[_currentChat].Substring(index + 1);
        }
        content.text = _chats[_currentChat];
    }
    public void SendChat()
    {
        if (string.IsNullOrEmpty(inputField.text) || string.IsNullOrWhiteSpace(inputField.text)) return;
        print("SendChat");

        string[] words = inputField.text.Split(' ');

        if (words[0] == "/w" && words.Length > 2)
        {
            _chatClient.SendPrivateMessage(words[1], string.Join(" ", words, 2, words.Length - 2));
        }
        else
        {
            _chatClient.PublishMessage(_channels[_currentChat], inputField.text);
        }
        inputField.text = "";
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
    }
    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnChatStateChange(ChatState state)
    {
    }

    public void OnConnected()
    {
        _chatClient.Subscribe(_channels);
    }

    public void OnDisconnected()
    {
        print("Chat/Disconnected");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        print("OnGetMessages");
        for (int i = 0; i < senders.Length; i++)
        {
            int indexChat = _chatDic[channelName];
            _chats[indexChat] += messages[i] + "\n";
        }
        UpdateChatUI();
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        for (int i = 0; i < _chats.Length; i++)
        {
            _chats[i] += "<color=purple>" + sender + ": " + "</color>" + message + "\n";
        }
        UpdateChatUI();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            _chats[0] += "<color=green>" + ": " + channels[i] + "</color>" + "\n";
        }
        UpdateChatUI();
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }
    public void SelectChat()
    {
        OnSelect();
    }
    public void DeselectChat()
    {
        OnDeselect();
    }
}
    

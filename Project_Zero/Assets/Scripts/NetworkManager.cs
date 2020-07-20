using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers = 10;

    // instance
    public static NetworkManager instance;

    void Awake ()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start ()
    {
        // connect to the master server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster ()
    {
        PhotonNetwork.JoinLobby();
    }

    // creates a new room of the requested room name
    public void CreateRoom (string roomName)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;

        PhotonNetwork.CreateRoom(roomName, options);
    }
    
    // joins a room of the requested room name
    public void JoinRoom (string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // changes the scene through Photon's system
    [PunRPC]
    public void ChangeScene (string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    // called when we disconnect from the Photon server
    public override void OnDisconnected (DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Menu");
    }

    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();

        if(PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }
    }
}
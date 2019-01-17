// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon;

public class GameMenu : MonoBehaviour
{
    public GUISkin Skin;
    public Vector2 WidthAndHeight = new Vector2(1200, 800);
    private string roomName = "myLab";
    private string password = "";
    private string roomAndPass = "";
    private bool spectator = false;

    private Vector2 scrollPos = Vector2.zero;

    private bool connectFailed = false;

    public static readonly string SceneNameMenu = "Launcher";

    public static readonly string SceneNameGame = "vrjeans_scene1";

    public static readonly string SpectatorScene = "spectator_scene";

    private string errorDialog;
    private double timeToClearDialog;

    public string ErrorDialog
    {
        get { return this.errorDialog; }
        private set
        {
            this.errorDialog = value;
            if (!string.IsNullOrEmpty(value))
            {
                this.timeToClearDialog = Time.time + 4.0f;
            }
        }
    }

    public void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings("0.9");
        }

        // generate a name for this player, if none is assigned yet
        if (String.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
        }

        // if you wanted more debug out, turn this on:
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;
    }

    public void OnGUI()
    {
        if (this.Skin != null)
        {
            GUI.skin = this.Skin;
        }

        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connecting)
            {
                GUILayout.Label("Connecting to: " + PhotonNetwork.ServerAddress);
            }
            else
            {
                GUILayout.Label("Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress);
            }

            if (this.connectFailed)
            {
                GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
                GUILayout.Label(String.Format("Server: {0}", new object[] {PhotonNetwork.ServerAddress}));
                GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID.Substring(0, 8) + "****"); // only show/log first 8 characters. never log the full AppId.

                if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                {
                    this.connectFailed = false;
                    PhotonNetwork.ConnectUsingSettings("0.9");
                }
            }

            return;
        }

        Rect content = new Rect((Screen.width - this.WidthAndHeight.x)/2, (Screen.height - this.WidthAndHeight.y)/2, this.WidthAndHeight.x, this.WidthAndHeight.y);
        GUI.Box(content, "Join or Create Room");
        GUILayout.BeginArea(content);

        GUILayout.Space(40);

        // Player name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player name:", GUILayout.Width(200));
        PhotonNetwork.playerName = GUILayout.TextField(PhotonNetwork.playerName);
        GUILayout.Space(158);
        if (GUI.changed)
        {
            // Save name
            if (spectator && !PhotonNetwork.playerName.Contains("Spectator"))
            {
                
                PlayerPrefs.SetString("playerName", "Spectator_" + PhotonNetwork.playerName);
            }
            else
            {
                PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Roomname:", GUILayout.Width(200));
        this.roomName = GUILayout.TextField(this.roomName);
        GUILayout.Space(158);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Password(4 digit code):", GUILayout.Width(200));

        this.password = GUILayout.PasswordField(this.password, "*"[0], 4, GUILayout.Width(100));

        this.roomAndPass = this.roomName + this.password;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        //this.roomName = GUILayout.TextField(this.roomName);
        // Join room by title
        // Create a room (fails if exist or if password does not comply with rules!)
        if (GUILayout.Button("Create Room", GUILayout.Width(150)))
        {
            int n;
            if (spectator)
            {
                PhotonNetwork.playerName = "Spectator_" + PhotonNetwork.playerName;
            }
            if (this.password.Length == 4 && int.TryParse(this.password, out n))
            {
                print("Create room: " + this.roomAndPass);
                PhotonNetwork.CreateRoom(this.roomAndPass.ToLower(), new RoomOptions() { MaxPlayers = 10 }, null);
            }
            if (!(this.password.Length == 4) || !int.TryParse(this.password, out n))
            {
                ErrorDialog = "Password has to be of length 4 and contain only digits.";

            }
        }
        if (GUILayout.Button("Join Room", GUILayout.Width(150)))
        {
            PhotonNetwork.JoinRoom(this.roomAndPass.ToLower());
        }

        spectator = GUILayout.Toggle(spectator, "Spectator View");

        GUILayout.EndHorizontal();


        if (!string.IsNullOrEmpty(ErrorDialog))
        {
            GUILayout.Label(ErrorDialog);

            if (this.timeToClearDialog < Time.time)
            {
                this.timeToClearDialog = 0;
                ErrorDialog = "";
            }
        }

        GUILayout.Space(15);

        // Join random room
        //GUILayout.BeginHorizontal();

        //GUILayout.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
        //GUILayout.FlexibleSpace();
        //if (GUILayout.Button("Join Random", GUILayout.Width(150)))
        //{
        //    PhotonNetwork.JoinRandomRoom();
        //}


        //GUILayout.EndHorizontal();

        //GUILayout.Space(15);
        //if (PhotonNetwork.GetRoomList().Length == 0)
        //{
        //    GUILayout.Label("Currently no games are available.");
        //    GUILayout.Label("Rooms will be listed here, when they become available.");
        //}
        //else
        //{
        //    GUILayout.Label(PhotonNetwork.GetRoomList().Length + " rooms available:");

        //    // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
        //    this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
        //    foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
        //    {
        //        GUILayout.BeginHorizontal();
        //        GUILayout.Label(roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers);
        //        if (GUILayout.Button("Join", GUILayout.Width(150)))
        //        {
        //            PhotonNetwork.JoinRoom(roomInfo.Name);
        //        }

        //        GUILayout.EndHorizontal();
        //    }

        //    GUILayout.EndScrollView();
        //}

        GUILayout.EndArea();
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }

    public void OnPhotonCreateRoomFailed()
    {
        ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause)
    {
        ErrorDialog = "Error: Can't join room (room name or password is incorrect). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        ErrorDialog = "Error: Can't join room (room name or password is incorrect).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        PhotonNetwork.LoadLevel(SceneNameGame);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
        PhotonNetwork.JoinLobby();
    }
}

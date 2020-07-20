using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tim.Scripts.Login_System
{
    public class Launcher: MonoBehaviourPunCallbacks
    {

        // Instance
        public static Launcher Instance;
        
        [Tooltip("The level index to load in build settings, 1,2,3...")]
        [SerializeField]
        public int LevelToLoad;

        #region Private Serializable Fields

        /// <summary>
        /// The maximum number of player per room. When a room is full, it can't be joined by new players,
        /// so a new room will be created.
        /// </summary>
        [Tooltip("The maximum of player per room. If a room is full, " +
                 "it can't be joined by new players, so a new room will be created.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        #endregion

        #region Private Fields
        
        /// <summary>
        /// This clients version number. Users are separated from each other by gameVersion.
        /// Allows making game breaking changes. 
        /// </summary>
        private string gameVersion = "1";
        
        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during init phase.
        /// </summary>
        private void Awake()
        {

            Instance = this;
            
            //Allows breaking changes
            PhotonNetwork.GameVersion = gameVersion;
            
            // #CRITICAL
            // This makes sure we can use PhotonNetwork.LoadLevel() on the master client,
            // and all clients in the same room sync their level automatically. 
            //PhotonNetwork.AutomaticallySyncScene = true;
            DontDestroyOnLoad(gameObject);
            
        }

        #endregion

        #region Public Methods

        public static void ConnectToPhoton()
        {
            // we check if are connected or not, we join if we are, else we init connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #CRITICAL
                // We need this at this point to attempt joining a Random Room. 
                // If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical: We must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                
            }
        }

        #endregion

        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basic Launcher: OnConnectedToMaster() was called by PUN");
            
            // #Critical: The first try to do is to join a potential existing room.
            // If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat($"PUN Basic Launcher: OnDisconnected() was called by PUN {cause}");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basic Launcher: OnJoinRandomFailed() was called by PUN." +
                      " No random room available, so we create one.\n Calling: PhotonNetwork.CreateRoom");
            
            // #Critical: we failed to join a random room, maybe none exists or they are all full.
            // NO worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions{MaxPlayers = maxPlayersPerRoom});
        }
    
        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basic Launcher: OnJoinedRoom() called by PUN. " +
                      "Now this client is in a room.");

            if (PhotonNetwork.IsMasterClient)
            {
                SceneManager.LoadScene(LevelToLoad);

                //PhotonNetwork.LoadLevel(LevelToLoad);

            }

        }

        #endregion
    }
}
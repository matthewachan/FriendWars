using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
    public GameObject playerPrefab;
    public Transform spawnLocation;

    string gameName = "MC_FriendWars_v0";

    float display_x;
    float display_y;
    float btn_height;
    float btn_width;

    Rect rect1;
    Rect rect2;

    bool refreshing = false;

    HostData[] hostData;

	// Use this for initialization
	void Start () {
        
        display_x = Screen.width * 0.05f;
        display_y = Screen.width * 0.05f;
        btn_width = Screen.width * 0.1f;
        btn_height = Screen.height * 0.1f;
        rect1 = new Rect(display_x, display_y, btn_width, btn_height);
        rect2 = new Rect(display_x, display_y + btn_height + 4, btn_width, btn_height);
	}

    void Update() {
        if (refreshing) {
            if(MasterServer.PollHostList().Length > 0) {
                refreshing = false;
                Debug.Log(MasterServer.PollHostList().Length);
                hostData = MasterServer.PollHostList();
            }
        }
    }

    void startServer() {
        // max 12 players, port 5000
        Network.InitializeServer(12, 5000, !Network.HavePublicAddress());
        MasterServer.RegisterHost(gameName, "FriendWars", "2D platformer with friends!");
        Network.sendRate = 15   ;
    }

    void refreshHosts() {
        MasterServer.RequestHostList(gameName);
        refreshing = true;
    }

    void spawnPlayer() {
        Network.Instantiate(playerPrefab, spawnLocation.position, Quaternion.identity, 0);
    }
    
    void OnServerInitialized() {
        Debug.Log("Server initialized.");
        spawnPlayer();
    }


    // Called when client connects
    void OnConnectedToServer() {
        spawnPlayer();
    }

    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.RegistrationSucceeded) {
            Debug.Log("Server registered!");
        }
    }

    // Update called several times per frame (overhead warning)
    void OnGUI() {
        if (!Network.isServer && !Network.isClient) {
            if (GUI.Button(rect1, "Start Server")) {
                Debug.Log("Starting server...");
                startServer();
            }
            if (GUI.Button(rect2, "Refresh Hosts")) {
                Debug.Log("Refreshing Hosts...");
                refreshHosts();
            }
            if (!Object.Equals(hostData, default(HostData))) {
                for (int i = 0; i < hostData.Length; ++i) {
                    if (GUI.Button(new Rect(display_x + btn_width + 4, display_y + (btn_height + 4) * (i + 1), btn_width, btn_height), hostData[i].gameName)) {
                        Network.Connect(hostData[i]);
                    }
                }
            }
        }
       
    }
}

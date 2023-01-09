using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager1 : MonoBehaviourPunCallbacks
{
    //public InputField NickNameInput;
    public GameObject EntrancePannel;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public GameObject Player;
    public Transform Playertrans;
    public PhotonView PV;

    public Text WelcomeText;
    public Text ListText;
    public Text RoomInfoText;
    public Text MasterText;
    public GameManager gamemanager;

    public GameObject[] Pannels;
    public Text[] Players;
    public GameObject[] PlayerSprite;
    public GameObject[] PlayerGun;

   




    void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        PhotonNetwork.AutomaticallySyncScene = true;//방장이 신 로드하면 나머지도 바꿈
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        //PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }

    private void Start()
    {
        gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Spawn();
        WelcomeText.text = "<color=yellow>" + PhotonNetwork.LocalPlayer.NickName + "님이 입장하셨습니다</color>";
        if (PhotonNetwork.IsMasterClient)//분필
        {
            PhotonNetwork.InstantiateSceneObject("Pencile", new Vector3(Random.Range(-7, 7), Random.Range(2, 4), 0), Quaternion.identity);
            PhotonNetwork.InstantiateSceneObject("Pen", new Vector3(Random.Range(-7, 7), Random.Range(2, 4), 0), Quaternion.identity);
            PhotonNetwork.InstantiateSceneObject("Sharp", new Vector3(Random.Range(-7, 7), Random.Range(2, 4), 0), Quaternion.identity);
        }
    }


    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    public void Spawn()
    {
        Player = PhotonNetwork.Instantiate("Player", new Vector3(0, 0, 0), Quaternion.identity);


        gamemanager.isMaster = PhotonNetwork.IsMasterClient ? true : false;
        if (gamemanager.isMaster)
        {
            Pannels[0].SetActive(true);
        }
        else
        {
            Pannels[0].SetActive(false);
        }

        EntrancePannel.SetActive(false);
        DisconnectPanel.SetActive(true);
        StartCoroutine("DestroyBullet");
        RoomRenewal();

    }

    void FixedUpdate() {
        if(gamemanager ==null)
            gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        for (int i = 0; i < gamemanager.Players.Count; i++)
        {
            if (gamemanager.Players[i].PlayerSkin != PlayerSprite[i].GetComponent<Animator>().GetFloat("Blend") || gamemanager.Players[i].GunSkin != PlayerGun[i].GetComponent<Animator>().GetFloat("Blend"))
                RoomRenewal();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Main");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Playertrans = Player.transform;
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");


    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }



    void RoomRenewal()
    {
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name;
        string a = "방장 : " + PhotonNetwork.MasterClient;
        MasterText.text = a.Substring(0,a.Length-2) + "님";


        if (gamemanager.Players.Count != PhotonNetwork.PlayerList.Length)
        {
            print("실패");
            Invoke("RoomRenewal", 1);
        }
        else
        {
            for (int i = 0; i < gamemanager.Players.Count; i++)
            {
                PlayerSprite[i].SetActive(true);
                PlayerGun[i].SetActive(true);

                Players[i].text = gamemanager.Players[i].nick;
                PlayerSprite[i].GetComponent<Animator>().SetFloat("Blend", gamemanager.Players[i].PlayerSkin);
                PlayerGun[i].GetComponent<Animator>().SetFloat("Blend", gamemanager.Players[i].GunSkin);
            }
            for (int i = PhotonNetwork.PlayerList.Length; i < 4; i++)
            {
                PlayerSprite[i].SetActive(false);
                PlayerGun[i].SetActive(false);
                Players[i].text = "";
            }
        }
    }

    public void FindMap()
    {
        Pannels[1].SetActive(true);
    }
    public void NotFindMap()
    {
        Pannels[1].SetActive(false);
    }

    public void Respawn()
    {
        GameObject.Find("Canvas2").transform.Find("RespawnPanel").gameObject.SetActive(false);
        Player.GetComponent<PhotonView>().RPC("Respawn", RpcTarget.All);
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        WelcomeText.text = msg;
    }

     public  void GoScene(string Name)
    {
        PhotonNetwork.LoadLevel(Name);
    }

    [ContextMenu("정보")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        }
        else
        {
            print("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("로비에 있는지? : " + PhotonNetwork.InLobby);
            print("연결됐는지? : " + PhotonNetwork.IsConnected);
        }
    }


}

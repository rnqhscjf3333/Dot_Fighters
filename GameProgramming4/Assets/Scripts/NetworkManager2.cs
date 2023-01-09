using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager2 : MonoBehaviourPunCallbacks
{
    public GameObject RespawnPanel;
    public GameObject Player;
    public Transform Playertrans;
    public PhotonView PV;

    public GameManager gamemanager;
    public Text Survived;
    public bool isReady;//방에 모두 들어왔는지
    public bool isStart;//게임시작

    public Vector2[] StartPosition;
    public float StartTimer;
    public GameObject StartTimerObject;

    public int PlayerCount;
    public GameObject Win;
    public GameObject RankBoard;
    public GameObject ReturnButton;
    public Text[] rank;

    public float BossCul;//공격쿨타임
    public float ThunderCul;//천둥쿨타임
    public Animator Thunder;
    public Animator Event;










    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;//방장이 신 로드하면 나머지도 바꿈
        PlayerCount = PhotonNetwork.PlayerList.Length;
        Survived.text = "생존 : " + PlayerCount + "명";
    }


    private void Start()
    {
        gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Spawn();

    }


    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    public void Spawn()
    {
        Player = PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-6, 2), 10, 0), Quaternion.identity);
        Player.GetComponent<PlayerScript>().ismove = false;
        Player.GetComponent<PlayerScript>().isGame = true;
        Player.GetComponent<PlayerScript>().NT = this;

        StartCoroutine("DestroyBullet");
        RoomRenewal();
    }

    void FixedUpdate()
    {
        if (gamemanager == null)
            gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (StartTimer > 0)
        {
            StartTimerObject.SetActive(true); StartTimer -= Time.deltaTime;
        }
        else
            StartTimerObject.SetActive(false);

        if (StartTimer > 4)
            StartTimerObject.transform.Find("Text").GetComponent<Text>().text = "3";
        else if (StartTimer > 3)
            StartTimerObject.transform.Find("Text").GetComponent<Text>().text = "2";
        else if (StartTimer > 2)
            StartTimerObject.transform.Find("Text").GetComponent<Text>().text = "1";
        else if (StartTimer > 1)
            StartTimerObject.transform.Find("Text").GetComponent<Text>().text = "Start!!";

        if (isReady && StartTimer <= 2 && !isStart)
        {
            isStart = true;
            Player.GetComponent<PlayerScript>().ismove = true;
            if (GameObject.Find("배경음악") != null)
                GameObject.Find("배경음악").GetComponent<AudioSource>().Play();

            if (Thunder != null)
            {
                Thunder.SetTrigger("isThunder");
                Event.SetTrigger("Start");


                if (PhotonNetwork.IsMasterClient)
                {
                    BossCul = 20;
                    ThunderCul = 5;
                }
            }
        }
        if (isStart && PhotonNetwork.IsMasterClient && Thunder != null)
        {
            if (BossCul > 0)
                BossCul -= Time.deltaTime;
            else
            {
                int ran = Random.Range(0, 3);
                if (ran == 0)
                {
                    Player.GetComponent<PlayerScript>().PV.RPC("EventRPC", RpcTarget.All, 0);
                    BossCul = Random.Range(10, 20);
                }
                if (ran == 1)
                {
                    Player.GetComponent<PlayerScript>().PV.RPC("EventRPC", RpcTarget.All, 1);
                    BossCul = Random.Range(10, 20);
                }
                if (ran == 2)
                {
                    Player.GetComponent<PlayerScript>().PV.RPC("EventRPC", RpcTarget.All, 2);
                    BossCul = Random.Range(20, 30);
                }
            }

            if (ThunderCul > 0)
                ThunderCul -= Time.deltaTime;
            else
            {
                ThunderCul = Random.Range(5, 10);
                Player.GetComponent<PlayerScript>().PV.RPC("ThunderRPC", RpcTarget.All);
            }
        }
    }

    void Update()
    {

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Main");
    }

    [PunRPC]
    public void ReCount()
    {
        print("recount");
        Survived.text = "현재인원 : " + PlayerCount + "명";

        if (PlayerCount <= 1)
        {
            if (Player.GetComponent<PlayerScript>().HealthImage.fillAmount > 0)
            {
                Player.GetComponent<PlayerScript>().Rank = 1;
                Win.SetActive(true);
            }

            bool issucess = true;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (gamemanager.Players[i].Rank == 0)
                {
                    print("실패");
                    Invoke("ReCount", 1);
                    issucess = false;
                    break;
                }
                rank[gamemanager.Players[i].Rank - 1].text = (gamemanager.Players[i].Rank) + "등 : " + gamemanager.Players[i].nick;
            }
            for (int i = PhotonNetwork.PlayerList.Length; i < 4; i++)
            {
                rank[i].text = "";
            }
            if (issucess)
                RankBoard.SetActive(true);
            if (PhotonNetwork.IsMasterClient)
                ReturnButton.SetActive(true);

        }
    }



    [PunRPC]
    void RoomRenewal()
    {
        if (!isReady && gamemanager.Players.Count != PhotonNetwork.PlayerList.Length)
        {
            print("실패");
            Invoke("RoomRenewal", 1);
        }
        else
        {
            for (int i = 0; i < gamemanager.Players.Count; i++)
            {
                StartTimer = 5f;
                isReady = true;
                gamemanager.Players[i].transform.position = StartPosition[i];
            }
        }
    }





    public void GoScene(string Name)
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

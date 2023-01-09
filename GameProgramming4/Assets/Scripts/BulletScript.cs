using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Animator AN;
    public int killed = 0;
    public Transform bulletImage;
    int dir;    //방향
    int BulletUp = 0; //각도
    int fire = 0;
    int BSpeed = 10;
    int skin;
    PlayerScript playerscript;
    Rigidbody2D rigidbody;
    public ParticleSystem parti;


    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(dir, 1, 1);
        transform.eulerAngles = new Vector3(0, 0, BulletUp);
        Destroy(gameObject, 3.5f);
    }

    void Update()
    {

        if (fire == 0 && skin != 3)
        {
            transform.Translate(Vector3.right * BSpeed * Time.deltaTime * dir);//속도
        }
        if (PV.IsMine && killed == 1)
        {
            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            //manager.kill += 1;
            killed = 0;
        }
        RaycastHit2D[] rayHits = Physics2D.CircleCastAll(transform.position, 15, Vector2.up, 0f, LayerMask.GetMask("Ground"));

    }

    void FixedUpdate()
    {
        float angle = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x) * Mathf.Rad2Deg;
        bulletImage.eulerAngles = new Vector3(0, 0, angle);
    }


    void OnTriggerEnter2D(Collider2D col) // col을 RPC의 매개변수로 넘겨줄 수 없다
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Ground") || col.gameObject.layer == LayerMask.NameToLayer("Grass"))
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
            if (skin == 3)
            {
                RaycastHit2D[] rayHits = Physics2D.CircleCastAll(transform.position, 5, Vector2.up, 0f, LayerMask.GetMask("Ground"));
                foreach (RaycastHit2D hitObj in rayHits)
                {
                    if (hitObj.transform.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic)
                        print(Vector2.Distance(hitObj.transform.position, transform.position) + " " + hitObj.transform.name + ", " + ((hitObj.transform.position - transform.position).normalized * 200 / (hitObj.transform.position - transform.position).magnitude).magnitude);
                    hitObj.transform.GetComponent<Rigidbody2D>().AddForce((hitObj.transform.position - transform.position).normalized * 300 / (hitObj.transform.position - transform.position).magnitude);
                }
                rayHits = Physics2D.CircleCastAll(transform.position, 5, Vector2.up, 0f, LayerMask.GetMask("Player"));
                foreach (RaycastHit2D hitObj in rayHits)
                {
                    hitObj.transform.GetComponent<PlayerScript>().RB.AddForce((hitObj.transform.position - transform.position).normalized * 2000 / (hitObj.transform.position - transform.position).magnitude);
                    float forc = 20 / (hitObj.transform.position.x - transform.position.x);
                    if (forc > 30)
                        forc = 30;
                    if (forc < -30)
                        forc = -30;
                    hitObj.transform.GetComponent<PlayerScript>().force += forc;
                    print(20 / (hitObj.transform.position.x - transform.position.x));
                    hitObj.transform.GetComponent<PhotonView>().RPC("Hit2", RpcTarget.All, Mathf.Abs(forc / 100));
                }
            }
        }

        if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {


            col.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)) * 500);
        }

        if (PV.IsMine && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine == false && col.GetComponent<PlayerScript>().HealthImage.fillAmount > 0 && skin != 3)
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
            col.GetComponent<PhotonView>().RPC("Hit", RpcTarget.All);
            if (col.GetComponent<PlayerScript>().HealthImage.fillAmount == 0)
            {
                playerscript.kill += 1;
                GameObject.Find("Canvas2").transform.Find("KillPanel").transform.Find("Text").GetComponent<Text>().text = "Kill : " + playerscript.kill;

                GameObject.Find("Canvas2").transform.Find("Notice").transform.Find("Text").GetComponent<Text>().text = playerscript.nick + " 님이 " + col.GetComponent<PlayerScript>().nick + " 님을 처치하였습니다.";
                GameObject.Find("Canvas2").transform.Find("Notice").gameObject.SetActive(true);
            }
        }
    }


    [PunRPC]
    void DirRPC(int dir, int skin, int Up, int Speed, int actor1)
    {
        this.dir = dir; //총알 방향 플레이어에서 받음
        GameManager gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        int i;
        for (i = 0; i < 4; i++)
        {
            if (gamemanager.Players[i].actor == actor1)
                break;
        }

        rigidbody = GetComponent<Rigidbody2D>();
        if (skin == 3)
            rigidbody.AddForce(new Vector3(Mathf.Cos(new Vector3(0, 0, Up).z * Mathf.Deg2Rad), Mathf.Sin(new Vector3(0, 0, Up).z * Mathf.Deg2Rad), 0) * 500);

        playerscript = gamemanager.Players[i].GetComponent<PlayerScript>();
        this.skin = skin;
        AN.SetFloat("Blend", skin);
        this.BulletUp = Up;
        BSpeed = Speed;
    }

    [PunRPC]
    void DestroyRPC()
    {
        parti.Stop();

        fire = 1;
        AN.SetTrigger("shoot");
        rigidbody.velocity = Vector3.zero;
        Destroy(gameObject, 5f);//파괴
    }
}

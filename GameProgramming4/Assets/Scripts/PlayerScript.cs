using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering.Universal;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameManager gamemanager;
    public Rigidbody2D RB;
    public Animator AN;
    public Animator GN; //총 애니메이션
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;
    public Image BulletImage;

    public float Bullettime = 0.01f;

    public float axis;
    public bool isGround;//점프상태
    public bool isGrass;//풀
    public bool isLeftConv;
    public bool isRightConv;

    float Bullet = 0.2f;// 1/총알 개수
    public Transform Gun;//총
    public Transform GunLight;//총빛
    public Transform ShotPoint;

    public AudioClip[] audios;
    public AudioSource audioSource;
    public AudioSource GaudioSource;//총 오디오

    public int GunNum;//현재 총 이름
    int[] GunTime = new int[] { 1, 2, 3, 4 };//총 쿨타임
    public ParticleSystem[] GunParticle; //파티클시스템

    bool isReroad = false;//장전중
    bool isCanShoot = true;//쏠수있나
    bool isreloadmotion = false;//장전파티클시간
    public bool ismove= true;//살아있는지
    public bool isShoot = true;//총쏠수있는지-메뉴판

    Transform listenertrans;

    public int PlayerSkin;
    public int GunSkin;

    public float Liflecul;
    public float maxSpeed_left = 6;
    public float maxSpeed_right = 6;

    int Liflenum = 0;
    public Sprite[] bulletImage;

    public string nick;
    public int actor;
    public int kill;
    float hitcul;
    public int Rank;
    public bool isGame;
    public NetworkManager2 NT;

    public float force;
    public float Convey;
    public float Convey2;
    bool isStop;


    Vector3 curPos;



    void Awake()
    {
        gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();

        actor = PV.OwnerActorNr;
        nick = PV.Owner.NickName;

        gamemanager.Players.Add(this);
        gamemanager.SortPlayers();

        // 닉네임
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : new Color(0, 0, 0, 0);



        //if (!PV.IsMine) GunLight.GetComponent<Light2D>().enabled = false;

        HealthImage.color = PV.IsMine ? Color.red  : new Color(0, 0, 0, 0);
        BulletImage.color = PV.IsMine ? Color.yellow : new Color(0, 0, 0, 0);




        if (PV.IsMine)
        {
            // 2D 카메라
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;

            PV.RPC("SkinRPC", RpcTarget.AllBuffered);
            listenertrans = GameObject.Find("SoundListner").GetComponent<Transform>();

            if (GameObject.Find("메뉴판") != null)
                GameObject.Find("메뉴판").GetComponent<Menu>().player = gameObject;
        }

    }
    void OnDestroy()
    {
        gamemanager.Players.Remove(this);
        gamemanager.SortPlayers();
    }


    void Update()
    {
        if (hitcul > 0)//맞으면 닉네임이랑 체력 잠깐 보이기
            hitcul -= Time.deltaTime;
        if (hitcul <= 0 && HealthImage.fillAmount > 0)
        {
            if (PV.IsMine)
                NickNameText.color = Color.green;
            else
            {
                NickNameText.color = new Color(0, 0, 0, 0);
                HealthImage.color = new Color(0, 0, 0, 0);
            }
        }


        if (PlayerSkin != (int)AN.GetFloat("Blend") || GunSkin != (int)GN.GetFloat("Blend"))
        {
            PlayerSkin = (int)AN.GetFloat("Blend");
            GunSkin = (int)GN.GetFloat("Blend");
        }
        if (PV.IsMine)
        {
            listenertrans.position = GetComponent<Transform>().position;
            if (ismove )
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);// 마우스 따라 총 회전
                float maxis = 0;
                Vector2 direction = mousePos - (Vector2)Gun.position;
                Gun.transform.right = direction;
                if (isShoot)
                {
                    if (Gun.transform.rotation.eulerAngles.z > 90 && Gun.transform.rotation.eulerAngles.z < 270)
                    {
                        PV.RPC("FlipGunRPC", RpcTarget.AllBuffered, -1);
                        Gun.transform.localPosition = new Vector3(0.2f, -0.2f, 1);
                        maxis = -1;
                    }
                    else
                    {
                        PV.RPC("FlipGunRPC", RpcTarget.AllBuffered, 1);
                        Gun.transform.localPosition = new Vector3(-0.2f, -0.2f, 1);
                        GunLight.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        maxis = 1;
                    }


                    PV.RPC("FlipXRPC", RpcTarget.AllBuffered, maxis); // 재접속시 filpX를 동기화해주기 위해서 AllBuffered

                }




                // ↑ 점프, 바닥체크
                isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
                bool isPlayer= Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Player"));
                isGrass = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Grass"));
                if (isGrass) isGround = true;
                AN.SetBool("jump", !isGround);
                if (Input.GetKeyDown(KeyCode.Space) && (isGround || isPlayer))
                {
                    PV.RPC("JumpRPC", RpcTarget.All);
                    audioSource.clip = audios[0];
                    audioSource.Play();
                }



                // ← → 이동
                axis = Input.GetAxisRaw("Horizontal");


                RaycastHit2D[] rayHits = Physics2D.CircleCastAll((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, Vector2.up, 0f, LayerMask.GetMask("Ground"));
                foreach (RaycastHit2D hitObj in rayHits)
                {
                    if (Convey2 == 0)
                        Convey2 = hitObj.transform.GetComponent<Rigidbody2D>().velocity.x;
                }
                if (rayHits.Length == 0)
                    Convey2 = 0;


                RB.velocity = new Vector2(Convey+ Convey2 + 4 * axis, RB.velocity.y);


                if (Mathf.Abs(force) < 1f) force = 0;
                else
                    RB.velocity = new Vector2(force, RB.velocity.y);
                if (force > 0)
                    force -= 1;
                if (force < 0)
                    force += 1;

                if (axis != 0)//보는방향
                {
                    if (isGrass) PV.RPC("ParticleRPC", RpcTarget.All, 7);
                    AN.SetBool("walk", true);
                    if (axis != maxis)
                        AN.SetBool("isback", true);
                }
                else { AN.SetBool("walk", false); AN.SetBool("isback", false); }

                //총알없음
                if (Input.GetMouseButtonDown(0) && (BulletImage.fillAmount < Bullet) && GunNum == 0 && isCanShoot && isShoot)
                {
                    audioSource.clip = audios[3];
                    audioSource.Play();
                }

                // 총알 발사
                if (Input.GetMouseButtonDown(0) && (BulletImage.fillAmount >= Bullet) && isCanShoot && isShoot && Liflenum == 0 && Liflecul <= 0)
                {
                    if (GN.GetFloat("Blend") == 0)
                        if (maxis < 0) PV.RPC("ParticleRPC", RpcTarget.All, 0); else PV.RPC("ParticleRPC", RpcTarget.All, 1);
                    BulletImage.fillAmount -= Bullet;
                    int gunskin = (int)GN.GetFloat("Blend");
                    if (gunskin >= 3) gunskin += 1;
                    PhotonNetwork.Instantiate("Bullet", ShotPoint.position, Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 1, gunskin, (int)Gun.transform.rotation.eulerAngles.z, 10, actor);
                    AN.SetTrigger("shot"); //총알 발사 
                    GN.SetTrigger("isShot");
                    if (gunskin == 0)
                        PV.RPC("SoundROC", RpcTarget.All, 1);
                    if (gunskin == 1)
                        PV.RPC("SoundROC", RpcTarget.All, 4);
                    if (gunskin == 2)
                        PV.RPC("SoundROC", RpcTarget.All, 5);
                    if (gunskin == 4)
                        PV.RPC("SoundROC", RpcTarget.All, 9);
                    if (gunskin == 5)
                        PV.RPC("SoundROC", RpcTarget.All, 8);
                    if (BulletImage.fillAmount < Bullet)
                        GN.SetTrigger("NoBullet");
                    Liflecul = 0.2f;
                }

                // 기관총 발사
                if (Input.GetMouseButton(0) && (BulletImage.fillAmount > 0) && isCanShoot && isShoot && Liflenum == 1 && Liflecul <= 0)
                {
                    if (maxis < 0)
                    {
                        PV.RPC("ParticleRPC", RpcTarget.All, 0);
                        force += 5;
                    }
                    else
                    {
                        PV.RPC("ParticleRPC", RpcTarget.All, 1);
                        force -= 5;
                    }
                    BulletImage.fillAmount -= 0.03f;
                    PhotonNetwork.Instantiate("Bullet", ShotPoint.position, Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 1, 0, (int)Gun.transform.rotation.eulerAngles.z + Random.Range(-5, 6), 15, actor);
                    AN.SetTrigger("shot"); //총알 발사 
                    GN.SetTrigger("isLifleShot");
                    PV.RPC("SoundROC", RpcTarget.All, 7);
                    Liflecul = 0.1f;
                }

                if (Liflecul > 0)
                    Liflecul -= Time.deltaTime;

                if (Input.GetMouseButton(0) && (BulletImage.fillAmount > 0) && isCanShoot && isShoot && Liflenum == 2 && Liflecul <= 0)//바주카
                {
                    if (maxis < 0) { PV.RPC("ParticleRPC", RpcTarget.All, 0); force += 10; }
                    else { PV.RPC("ParticleRPC", RpcTarget.All, 1); force -= 10; }
                    BulletImage.fillAmount -= 0.2f;
                    PhotonNetwork.Instantiate("Bullet", ShotPoint.position, Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 1, 3, (int)Gun.transform.rotation.eulerAngles.z + Random.Range(-5, 6), 10, actor);
                    AN.SetTrigger("shot"); //총알 발사 
                    if (BulletImage.fillAmount > 0.3f)
                        GN.SetTrigger("isBazukaShot");
                    PV.RPC("SoundROC", RpcTarget.All, 7);
                    Liflecul = 2f;
                }


                if (Liflenum == 1 && BulletImage.fillAmount <= 0)//라이플끝
                {
                    PV.RPC("ParticleRPC", RpcTarget.All, 6);
                    GN.SetTrigger("isLifeEnd");
                    BulletImage.fillAmount = 1;
                    Liflenum = 0;
                    Liflecul = 0f;
                    BulletImage.sprite = bulletImage[0];
                }
                if (Liflenum == 2 && BulletImage.fillAmount <= 0.1f)//바주카끝
                {
                    PV.RPC("ParticleRPC", RpcTarget.All, 8);
                    GN.SetTrigger("isLifeEnd");
                    BulletImage.fillAmount = 1;
                    Liflenum = 0;
                    Liflecul = 0f;
                    BulletImage.sprite = bulletImage[0];
                }

                //장전
                if (Input.GetKeyDown(KeyCode.R) && (BulletImage.fillAmount < 1) && GunNum == 0 && isCanShoot && Liflenum == 0)
                {
                    isReroad = true; isCanShoot = false; isreloadmotion = true;
                    BulletImage.fillAmount = 0f;
                    GN.SetTrigger("isReload");
                }
                if (isReroad)
                {
                    BulletImage.fillAmount += 0.5f * Time.deltaTime;
                }
                if (isReroad && BulletImage.fillAmount >= 0.25f && isreloadmotion)
                {
                    if (GN.GetFloat("Blend") == 0)
                        PV.RPC("ParticleRPC", RpcTarget.All, 2);
                    if (GN.GetFloat("Blend") == 1)
                    {
                        if (maxis < 1)
                            PV.RPC("ParticleRPC", RpcTarget.All, 3);
                        else
                            PV.RPC("ParticleRPC", RpcTarget.All, 4);
                    }
                    if (GN.GetFloat("Blend") == 2)
                        PV.RPC("ParticleRPC", RpcTarget.All, 5);
                    isreloadmotion = false;
                }
                if (isReroad && BulletImage.fillAmount >= 0.95f)
                {
                    BulletImage.fillAmount = 1f;
                    isReroad = false; isCanShoot = true;
                }

                if (Input.GetMouseButtonDown(1))//빛끄고켜기
                {
                    if (GunLight.GetComponent<Light2D>().intensity == 0f)
                        PV.RPC("LightRPC", RpcTarget.All, 6.5f);
                    else
                        PV.RPC("LightRPC", RpcTarget.All, 0f);
                }

                if (HealthImage.fillAmount <= 0)
                {
                    GameObject.Find("Canvas2").transform.Find("RespawnPanel").gameObject.SetActive(true);
                    AN.SetTrigger("isDie");
                    PV.RPC("LightRPC", RpcTarget.All, 0f);
                    ismove = false;
                }
            }
        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }


    [PunRPC]
    void FlipXRPC(float axis) => SR.flipX = axis == -1;

    [PunRPC]
    void FlipGunRPC(int a)
    {
        Gun.transform.localScale = new Vector3(1, a, 1);
        GunLight.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, a * -90));
    }

    [PunRPC]
    void JumpRPC()
    {
        RB.velocity = Vector2.zero;
        RB.AddForce(Vector2.up * 700);
    }

    [PunRPC]
    void BulletRPC()
    {
        BulletImage.fillAmount -= Bullet;
    }

    [PunRPC]
    void ParticleRPC(int i)
    {
        GunParticle[i].Play();
    }

    [PunRPC]
    void StopParticleRPC(int i)
    {
        GunParticle[i].Stop();
    }

    [PunRPC]
    void SoundROC(int i)
    {
        GaudioSource.clip = audios[i];
        GaudioSource.Play();
    }

    [PunRPC]
    void LightRPC(float li)
    {
        GunLight.GetComponent<Light2D>().intensity = li;
    }


    [PunRPC]
    void SkinRPC()
    {
        AN.SetFloat("Blend", gamemanager.skin);
        GN.SetFloat("Blend", gamemanager.gskin);
    }

    [PunRPC]
    public void Hit()//맞으면
    {
        HealthImage.fillAmount -= 0.1f;
        if (HealthImage.fillAmount > 0)
        {
            AN.SetTrigger("x");

            NickNameText.color = Color.red;
            HealthImage.color = Color.red;
            hitcul = 5;
        }
        if (HealthImage.fillAmount <= 0)
            if (isGame)
            {
                if (PV.IsMine)
                {
                    GameObject.Find("Canvas2").transform.Find("RespawnPanel").transform.Find("Text").GetComponent<Text>().text = NT.PlayerCount + "등";
                    Rank = NT.PlayerCount;
                }
                NT = GameObject.Find("NetworkManager").GetComponent<NetworkManager2>();
                NT.PlayerCount -= 1;
                NT.ReCount();
            }
    }
    [PunRPC]
    public void Hit2(float dam)//맞으면
    {
        HealthImage.fillAmount -= dam;
        if (HealthImage.fillAmount > 0)
        {
            AN.SetTrigger("x");

            NickNameText.color = Color.red;
            HealthImage.color = Color.red;
            hitcul = 5;
        }
        if (HealthImage.fillAmount <= 0)
            if (isGame)
            {
                if (PV.IsMine)
                {
                    GameObject.Find("Canvas2").transform.Find("RespawnPanel").transform.Find("Text").GetComponent<Text>().text = NT.PlayerCount + "등";
                    Rank = NT.PlayerCount;
                }
                NT = GameObject.Find("NetworkManager").GetComponent<NetworkManager2>();
                NT.PlayerCount -= 1;
                NT.ReCount();
            }
    }

    [PunRPC]
    public void Respawn()//부활
    {
        HealthImage.fillAmount = 1f;
        PV.RPC("LightRPC", RpcTarget.All, 6.5f);
        AN.SetTrigger("isSpawn");
        ismove = true;

    }

    public void Respawn1(bool isdie)
    {
        if (HealthImage.fillAmount <= 0)
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(isdie);
    }

    [PunRPC]
    void getLifle(int i)
    {
        Liflenum = i;
        if (Liflenum == 1)
        {
            GN.SetTrigger("isLifle");
            BulletImage.fillAmount = 1f;
            BulletImage.sprite = bulletImage[1];
        }
        else if (Liflenum == 2)
        {
            GN.SetTrigger("isBazuka");
            BulletImage.fillAmount = 1f;
            BulletImage.sprite = bulletImage[0];
        }
    }


    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);


    [PunRPC]
    void ThunderRPC()
    {
        print("번개");
        GameObject.Find("번개").GetComponent<Animator>().SetTrigger("isThunder");
    }

    [PunRPC]
    void EventRPC(int i)
    {
        print("이벤트 " + i);
        if (i == 0)
            GameObject.Find("이벤트").GetComponent<Animator>().SetTrigger("Right");
        if (i == 1)
            GameObject.Find("이벤트").GetComponent<Animator>().SetTrigger("Left");
        if (i == 2)
        {
            GameObject.Find("이벤트").GetComponent<Animator>().SetTrigger("isDown");
            Invoke("isUp", 1.5f);
            Invoke("isDown", 3);
            Invoke("isFin", 5);
        }
    }
    void isUp()
    {
        foreach (PlayerScript player in gamemanager.Players)
        {
            player.transform.GetComponent<Rigidbody2D>().gravityScale = 8;
        }
    }
    void isDown()
    {
        foreach (PlayerScript player in gamemanager.Players)
        {
            player.transform.GetComponent<Rigidbody2D>().gravityScale = 1;
        }
    }
    void isFin()
    {
        foreach (PlayerScript player in gamemanager.Players)
        {
            player.transform.GetComponent<Rigidbody2D>().gravityScale = 3;
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)   //위치, 체력 변수 동기화
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
            stream.SendNext(BulletImage.fillAmount);
            stream.SendNext(isGame);
            stream.SendNext(Rank);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
            BulletImage.fillAmount = (float)stream.ReceiveNext();
            isGame = (bool)stream.ReceiveNext();
            Rank = (int)stream.ReceiveNext();
        }
    }
}

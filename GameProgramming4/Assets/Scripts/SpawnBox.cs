using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering.Universal;

public class SpawnBox : MonoBehaviourPunCallbacks
{
    public GameObject Distructable_box;

    int spawn_delay = 5;            //오브젝트 생성 딜레이
    public float spawn_x = 0f;
    public float spawn_y = 0f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine("Spawn", spawn_delay);
        }
    }

    //게임 오브젝트를 복제하여 scene에 추가
    private IEnumerator Spawn(float delayTime)
    {
        Vector2 spawnPos = new Vector2(spawn_x, spawn_y); //생성위치
        PhotonNetwork.Instantiate("Distructable box", spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(delayTime);   //주기
        StartCoroutine("Spawn", spawn_delay);    //박스 다시 스폰
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

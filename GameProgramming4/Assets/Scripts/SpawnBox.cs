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

    int spawn_delay = 5;            //������Ʈ ���� ������
    public float spawn_x = 0f;
    public float spawn_y = 0f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine("Spawn", spawn_delay);
        }
    }

    //���� ������Ʈ�� �����Ͽ� scene�� �߰�
    private IEnumerator Spawn(float delayTime)
    {
        Vector2 spawnPos = new Vector2(spawn_x, spawn_y); //������ġ
        PhotonNetwork.Instantiate("Distructable box", spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(delayTime);   //�ֱ�
        StartCoroutine("Spawn", spawn_delay);    //�ڽ� �ٽ� ����
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

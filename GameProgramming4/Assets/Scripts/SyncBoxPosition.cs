using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering.Universal;

public class SyncBoxPosition : MonoBehaviourPunCallbacks, IPunObservable
{
    private Vector3 remotePos;
    private Quaternion remoteRot;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        if (stream.IsWriting)
        {
            // 이 방안에 있는 모든 사용자에게 브로드캐스트 
            // - 내 포지션 값을 보내보자
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        // 내가 데이터를 받는 중이라면 
        else
        {
            // 순서대로 보내면 순서대로 들어옴. 근데 타입캐스팅 해주어야 함
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        void Update()
        {
            if (false == photonView.IsMine)
            {
                transform.position = Vector3.Lerp(transform.position, remotePos, 10 * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, remoteRot, 10 * Time.deltaTime);

                return;
            }
        }
    }
}

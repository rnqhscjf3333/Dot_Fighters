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
            // �� ��ȿ� �ִ� ��� ����ڿ��� ��ε�ĳ��Ʈ 
            // - �� ������ ���� ��������
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        // ���� �����͸� �޴� ���̶�� 
        else
        {
            // ������� ������ ������� ����. �ٵ� Ÿ��ĳ���� ���־�� ��
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

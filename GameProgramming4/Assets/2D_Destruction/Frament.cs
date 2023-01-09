using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frament : MonoBehaviour
{
    public int hp = 3;
    public PhotonView PV;
    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<BulletScript>() != null)
        {
            hp--;
            if(hp <= 0)
            {
                PV.RPC("FragDestroy",RpcTarget.All);
                Destroy(gameObject, 0f);
            }
        }
    }
    [PunRPC]
    public void FragDestroy()
    {
        Destroy(gameObject, 0f);
    }
}


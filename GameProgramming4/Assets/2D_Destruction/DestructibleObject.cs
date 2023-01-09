using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Explodable))]
public class DestructibleObject : MonoBehaviour
{
    private Explodable _explodable;
    public PhotonView PV;
    public int hp = 3;

    void Start()
    {
        _explodable = GetComponent<Explodable>();
        PV = GetComponent<PhotonView>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<BulletScript>() != null)
        {
            hp--;
            if(hp <= 0)
            {
                PV.RPC("Explode",RpcTarget.All,collision.transform.position); // 총알이 맞은 위치 전달
            }
        }
    }
    [PunRPC]
    public void Explode(Vector3 position)
    {
        _explodable.explode();
        ExplosionForce ef = GameObject.FindObjectOfType<ExplosionForce>();
        ef.doExplosion(position);
    }
}

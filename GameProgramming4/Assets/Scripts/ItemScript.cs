using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemScript : MonoBehaviour
{
    public PhotonView PV;
    public CircleCollider2D collid;
    public Animator IN;
    public int Num;
    public float Itemcul;

    public Sprite[] sprite;
    public SpriteRenderer Gun;
    // Start is called before the first frame update


    // Update is called once per frame
    void FixedUpdate()
    {
        if (Itemcul > 0)
            Itemcul -= Time.deltaTime;

        if (Itemcul < 0)
        {
            Itemcul = 0;
            IN.SetTrigger("isSet");
        }
    }

    void OnTriggerEnter2D(Collider2D col) // col을 RPC의 매개변수로 넘겨줄 수 없다
    {
        if (col.tag == "Player")
        {
            print("아이템 획득");
            col.GetComponent<PhotonView>().RPC("getLifle", RpcTarget.All, Num);

            IN.SetTrigger("isEat");
            Itemcul = 20;

            if (Num == 1)
            {
                Num = 2;
                Gun.sprite = sprite[1];
            }
            else if (Num == 2)
            {
                Num = 1;
                Gun.sprite = sprite[0];
            }
        }

    }

}

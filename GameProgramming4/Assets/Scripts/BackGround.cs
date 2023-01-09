using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGround : MonoBehaviour
{
    public float speed;

    public int Startositionx;
    public int EndPositionx;
    public int[] EndPositiony;


    void Awake()
    {
    }

    void Update()
    {
        transform.position = new Vector2(transform.position.x-speed, transform.position.y);

        if (transform.position.x < EndPositionx)
            transform.position = new Vector2(Startositionx, Random.Range(EndPositiony[0], EndPositiony[1]));
    }
        

}

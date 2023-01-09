using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbleUnit : MonoBehaviour
{
    public float Lifecul;//0되면 비활성화
    public float maxLife;//활성화 시간
    // Start is called before the first frame update
    void OnEnable()
    {
        Lifecul = maxLife;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Lifecul > 0)
            Lifecul -= Time.deltaTime;
        else
            this.gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbleUnit : MonoBehaviour
{
    public float Lifecul;//0�Ǹ� ��Ȱ��ȭ
    public float maxLife;//Ȱ��ȭ �ð�
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

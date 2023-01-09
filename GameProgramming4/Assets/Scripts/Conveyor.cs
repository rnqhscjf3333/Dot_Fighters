using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed = 1 ;
    public float max_s = 2;
    public int direction = 1;
    Rigidbody2D rBody;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 pos = rBody.position;
        rBody.position +=  Vector2.left * speed * Time.fixedDeltaTime * -1 * direction;
        rBody.MovePosition(pos);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().Convey = -1*direction;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().Convey = 0;
        }
    }
}

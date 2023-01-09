using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Explodable))]
public class ExplodeOnClick : MonoBehaviour {

	private Explodable _explodable;

	void Start()
	{
		_explodable = GetComponent<Explodable>();
	}

	public void Explode()
    {
		_explodable.explode();
		ExplosionForce ef = GameObject.FindObjectOfType<ExplosionForce>();
		ef.doExplosion(transform.position);
	}

}

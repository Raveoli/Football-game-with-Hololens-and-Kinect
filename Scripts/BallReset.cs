using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetHit : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
    }

	void OnTriggerEnter(Collider col)
    {
		if (col.gameObject.name != null && col.gameObject.name.StartsWith ("Football")) {
			GetComponent<Rigidbody> ().useGravity = true;
			GetComponent<BoxCollider> ().isTrigger = false;
			Invoke("suicide", 4.0f);
		}
        
    }

	void suicide()
    {
		gameObject.SetActive(false);
    }
}
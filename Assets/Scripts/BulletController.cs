using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter2D(Collision2D c) {
        GameObject.Destroy(this.gameObject);
        Debug.Log("Bullet oncollisionenter called");
    }

}

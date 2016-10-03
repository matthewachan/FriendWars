using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {

    // exposed vars
    public int horizontalSpeed = 0;
    public int verticalSpeed = 0;

    // components
    Rigidbody2D platform;

	// Use this for initialization
	void Start () {
        platform = GetComponent<Rigidbody2D>();	
	}
	
    void OnCollisionEnter2D(Collision2D c) {
        // Player add as child to platform
        if (c.gameObject.tag == "Player")
            c.transform.parent = platform.transform;
    }

    void OnCollisionExit2D(Collision2D c) {
        // Player removed from hierarchy
        if (c.gameObject.tag == "Player")
            c.transform.parent = null;
    }

    // Update is called once per frame
    void Update () {
        // Platform oscillates
        platform.transform.position = new Vector2(platform.transform.position.x + Mathf.Sin(Time.time)*horizontalSpeed/10, platform.transform.position.y + Mathf.Sin(Time.time) * verticalSpeed/10);
	}
}

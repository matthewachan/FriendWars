using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    const float TOLERANCE = .1f;
    // Exposed variables
    public float movementSpeed = 50;
    public float jumpForce = 500;
    public float scrollSpeed = 2;
    public float spinForce = 1000f;
    public float bulletSpeed = 100f;

    public GameObject gunPrefab;
    public GameObject bulletPrefab;

    // Player components
    GameObject gun;
    GameObject bullet;
    GameObject[] bullets;
    Rigidbody2D player_rb;
    Vector2 last_position;
    Renderer player_render;
    NetworkTransform nTrans;

    float xVelocity;
    float direction;
    
    // Flags
    bool onGround;
    bool spinning;
    bool firing;

    // BG cam components
    Renderer bg_texture;
    Vector2 currentOffset;
    Transform bg_cam_transform;

    // Main cam components
    Transform main_cam_transform;


    // Use this for initialization
    void Start() {
        player_rb = GetComponent<Rigidbody2D>();
        nTrans = GetComponent<NetworkTransform>();
        player_render = GetComponent<Renderer>();
        last_position = player_rb.transform.position;

        spinning = false;
    }

    public override void OnStartLocalPlayer() {
        main_cam_transform = GameObject.Find("Main Camera").GetComponent<Transform>();
        bg_texture = GameObject.Find("Background").GetComponent<Renderer>();
        bg_cam_transform = GameObject.Find("BG Camera").GetComponent<Transform>();
    }


    void OnCollisionEnter2D(Collision2D c) {
        if (c.gameObject.tag == "Ground") {
            // Reset player
            player_rb.rotation = 0;
            onGround = true;
            spinning = false;
            firing = false;
            GameObject.Destroy(gun);
        }
    }

    void OnCollisionExit2D(Collision2D c) {
        if (c.gameObject.tag == "Ground") {
            onGround = false;
        }
    }


    // Update is called once per frame
    void Update() {
        if (isLocalPlayer) {


            // Walking
            xVelocity = Input.GetAxis("Horizontal");
            player_rb.velocity = new Vector2(xVelocity * movementSpeed, player_rb.velocity.y);
            player_rb.rotation = -xVelocity * 12;


            // Jumping
            if (Input.GetKeyDown(KeyCode.Space) && onGround) {
                player_rb.AddForce(new Vector2(0, jumpForce));
            }



            // Spinning
            else if (Input.GetKeyDown(KeyCode.Space) && !onGround && !spinning) {
                spinning = true;
                
                gun = GameObject.Instantiate(gunPrefab);
                gun.transform.parent = player_rb.transform;
                gun.transform.localPosition = new Vector2(.2f, -.5f);
                NetworkServer.Spawn(gun);
            }




            // Firing
            else if (Input.GetKeyDown(KeyCode.Space) && !onGround && spinning && !firing) {
                CmdFire();    
            }


            // Animation
            if (spinning) {
                Vector3 target = player_rb.transform.eulerAngles + 180f * Vector3.back;
                player_rb.transform.eulerAngles = Vector3.Lerp(player_rb.transform.eulerAngles, target, 2f * Time.deltaTime);
            }

            // Main cam follows player
            main_cam_transform.position = new Vector3(player_rb.transform.position.x, player_rb.transform.position.y, -2);


            // Side scrolling background
            direction = player_rb.transform.position.x - last_position.x;
            currentOffset.Set(currentOffset.x + direction * scrollSpeed / 1000, 0);
            bg_texture.material.mainTextureOffset = currentOffset;

            last_position = player_rb.transform.position;


        }
        else {
            // disable update function (locally, does not apply to other instances)
            enabled = false;
        }


    }

    // Called after all other update functions 
    void LateUpdate() {
        if (isLocalPlayer && bg_cam_transform != null) {
            // BG cam follows the main camera
            bg_cam_transform.position = new Vector3(main_cam_transform.position.x, bg_cam_transform.position.y, bg_cam_transform.position.z);
        } else if (!isLocalPlayer) {
            // disable lateupdate
            enabled = false;
        }
        
    }

    [Command]
    void CmdFire() {
        firing = true;
        bullet = GameObject.Instantiate(bulletPrefab);
        bullet.transform.rotation = player_rb.transform.rotation;
        bullet.transform.position = GameObject.Find("BulletSpawn").transform.position;

        //Logic for bullet direction

        float z = bullet.transform.rotation.z;
        float x_dir;
        float y_dir;

        if (Mathf.Abs(z) < .7071) x_dir = 1;
        else if (Mathf.Abs(z) < .7071 + TOLERANCE && Mathf.Abs(z) > .7071 - TOLERANCE) x_dir = 0;
        else x_dir = -1;

        if (z > 0) y_dir = 1;
        else if (Mathf.Abs(z) < TOLERANCE || Mathf.Abs(z) > 1 - TOLERANCE) y_dir = 0;
        else y_dir = -1;

        bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(x_dir * bulletSpeed, y_dir * bulletSpeed);
        NetworkServer.Spawn(bullet);
    }
}

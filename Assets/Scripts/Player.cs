using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    [SerializeField] float mouseSensetivity = 100f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float playerWidth = 0.15f;
    
    private Transform camera;
    private World world;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;

    private float xRotation;
    private Vector3 velocity;
    private float verticalMomentum;
    private bool jumpRequest;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camera = GameObject.Find("Main Camera").transform;
        world = FindObjectOfType<World>();
    }
    void Update()
    {
        GetPlayerInput();

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * moveSpeed;
        velocity += Vector3.up * gravity * Time.deltaTime;
        velocity.y = CheckDownSpeed(velocity.y);
        transform.Translate(velocity, Space.World);
        camera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseHorizontal);
    }

    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseHorizontal = Input.GetAxis("Mouse X") * mouseSensetivity * Time.deltaTime;
        mouseVertical = Input.GetAxis("Mouse Y") * mouseSensetivity * Time.deltaTime;

        xRotation -= mouseVertical;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        {

            isGrounded = true;
            return 0;

        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
            
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }

    }

    

}

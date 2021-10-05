using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //References
    [SerializeField] GameObject playerCam;
    [SerializeField] Rigidbody rb;

    //Mouse input variables
    float xRotation;
    float mouseX, mouseY;
    [Space, SerializeField] float sensitivity;

    //WASD input variables
    float horizontal, vertical;
    [Space, SerializeField] float moveSpeed;
    [SerializeField] float crouchMoveSpeed;

    //Jump variables
    bool canJump = true;
    [SerializeField] float jumpForce;

    //Crouch & Stand up variables
    [HideInInspector]
    public bool isCrouching = false;
    bool canCrouch = true;

    [Space, SerializeField] float crouchSpeed;
    [SerializeField] float crouchCooldown = .5f;

    [Space, SerializeField] float crouchHeight = .5f;
    [SerializeField] float crouchPosY = .55f;
    [SerializeField] float standingHeight = 1;
    [SerializeField] float standingPosY = 1.01f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        Look();
        Jump();
        Crouch();
    }

    public void FixedUpdate()
    {
        Move();
    }

    void Look()
    {
        mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(horizontal, 0, vertical);
        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = isCrouching ? crouchMoveSpeed : moveSpeed;
        rb.MovePosition(transform.position + transform.TransformDirection(dir) * speed * Time.deltaTime);
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && canJump)
        {
            canJump = false;

            var jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpForce);
            rb.AddRelativeForce(0, jumpVelocity, 0, ForceMode.Impulse);
        }
    }

    void Crouch()
    {
        if (Input.GetButton("Crouch"))
            isCrouching = true;
        else if (Input.GetButtonUp("Crouch"))
            isCrouching = false;

        //Make player crouch
        if(canCrouch && isCrouching && transform.localScale.y > crouchHeight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y - 1 * crouchSpeed, 1);

            //Make sure the player will be exactly at crouchHeight when nearing it
            if (transform.localScale.y <= crouchHeight + .05f)
                transform.localScale = new Vector3(1, crouchHeight, 1);

            //If the player isn't airborne put player on the ground
            if (canJump && transform.position.y <= 1.1f)
                transform.position = new Vector3(transform.position.x, crouchPosY, transform.position.z);
        }
        //Make player stand up
        else if(!isCrouching && transform.localScale.y < standingHeight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y + 1 * crouchSpeed, 1);

            //Make sure the player will be exactly at standinHeight when nearing it
            if (transform.localScale.y >= standingHeight - .05f)
            {
                transform.localScale = new Vector3(1, standingHeight, 1);

                canCrouch = false;
                StartCoroutine(WaitForCooldown());
            }

            //If player isn't airborne put player on ground
            if (canJump)
                transform.position = new Vector3(transform.position.x, standingPosY, transform.position.z);
        }
    }

    IEnumerator WaitForCooldown()
    {
        yield return new WaitForSeconds(crouchCooldown);

        canCrouch = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
            canJump = true;
    }
}

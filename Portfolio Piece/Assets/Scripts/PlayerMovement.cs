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

    //Jump variables
    bool canJump = true;
    [Space, SerializeField] float jumpForce;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        Look();
        Move();
        Jump();
        Crouch();
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

    private void Move()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(horizontal, 0, vertical);
        dir = Vector3.ClampMagnitude(dir, 1f);

        transform.Translate(dir * moveSpeed * Time.deltaTime, Space.Self);
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && canJump)
        {
            var jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpForce);

            rb.AddRelativeForce(0, jumpVelocity, 0, ForceMode.Impulse);

            canJump = false;
        }
    }

    void Crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
        
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
            canJump = true;
    }
}

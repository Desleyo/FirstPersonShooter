using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject playerCam;
    [SerializeField] Rigidbody rb;
    [SerializeField] Shoot shoot;

    [Space, SerializeField] float sensitivity;
    float xRotation;
    float yRotation;

    //Look variables
    [SerializeField] float camOffset = .65f;

    [Header("Move & Crouch variables")]
    [SerializeField] float moveSpeed;
    [HideInInspector] public bool isMoving;
    float horizontal, vertical;

    [SerializeField] float crouchMoveSpeed;
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchCooldown = .5f;
    [HideInInspector] public bool isCrouching = false;
    bool canCrouch = true;

    //Jump variables
    [Space, SerializeField] float jumpForce;
    [SerializeField] float normalSphereRadius;
    [SerializeField] float crouchingSphereRadius;
    Collider[] colliders;

    [Header("Standing & Crouch values")]
    [SerializeField] float crouchHeight = .5f;
    [SerializeField] float crouchPosY = .55f;
    [SerializeField] float standingHeight = 1;
    [SerializeField] float standingPosY = 1.01f;

    [Header("Recoil variables")]
    [SerializeField] float recoilSpeed;
    [SerializeField] float recoilClampX;
    [SerializeField] float recoilClampY;
    [SerializeField] float stopRecoilThreshold = .2f;
    float recoilMaxX, recoilMaxY;
    float recoilValueX, recoilValueY;
    bool canIncreaseRecoil;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        Look();
        IncreaseRecoil();
        Jump();
        Crouch();
    }

    public void FixedUpdate()
    {
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        playerCam.transform.position = new Vector3(transform.position.x, transform.position.y + camOffset, transform.position.z);
        playerCam.transform.localRotation = Quaternion.Euler(xRotation - recoilValueY, yRotation + recoilValueX, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void AddRecoil(float valueX, float valueY)
    {
        recoilMaxX += valueX;
        recoilMaxY += valueY;

        //Clamp max recoil on the Y axis
        recoilMaxX = Mathf.Clamp(recoilMaxX, -recoilClampX, recoilClampX);
        recoilMaxY = Mathf.Clamp(recoilMaxY, 0, recoilClampY);

        canIncreaseRecoil = true;
        StartCoroutine(WaitToDecrease(.1f));
    }

    void IncreaseRecoil()
    {
        if (canIncreaseRecoil)
        {
            recoilValueX = Mathf.Lerp(recoilValueX, recoilMaxX, 1 * Time.deltaTime * recoilSpeed);
            recoilValueY = Mathf.Lerp(recoilValueY, recoilMaxY, 1 * Time.deltaTime * recoilSpeed);
        }
        else
        {
            recoilValueX = Mathf.Lerp(recoilValueX, 0, 1 * Time.deltaTime * recoilSpeed);
            recoilValueY = Mathf.Lerp(recoilValueY, 0, 1 * Time.deltaTime * recoilSpeed);
        }

        if (recoilValueX < stopRecoilThreshold && recoilValueX > -stopRecoilThreshold && recoilValueY < stopRecoilThreshold && !canIncreaseRecoil)
        {
            recoilMaxX = 0;
            recoilMaxY = 0;
        }

        if (recoilValueY - stopRecoilThreshold < 0 && !canIncreaseRecoil)
        {
            recoilValueX = 0;
            recoilValueY = 0;
        }

        if (canIncreaseRecoil)
        {
            if (!shoot.fullAuto && !Input.GetButtonDown("Fire1") || !Input.GetButton("Fire1") || shoot.currentBulletCount == 0)
                StartCoroutine(WaitToDecrease(.1f));
        }
    }

    IEnumerator WaitToDecrease(float time)
    {
        yield return new WaitForSeconds(time);

        canIncreaseRecoil = false;
    }

    void Move()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        isMoving = horizontal != 0 || vertical != 0;

        Vector3 dir = new Vector3(horizontal, 0, vertical);
        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = isCrouching ? crouchMoveSpeed : moveSpeed;
        rb.MovePosition(transform.position + transform.TransformDirection(dir) * speed * Time.deltaTime);
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && CanJump())
        {
            var jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpForce);
            rb.AddRelativeForce(0, jumpVelocity, 0, ForceMode.Impulse);
        }
    }

    public bool CanJump()
    {
        bool canJump = false;
        float finalRadius = isCrouching ? crouchingSphereRadius : normalSphereRadius;
        colliders = Physics.OverlapSphere(transform.position, finalRadius);
        foreach(Collider collider in colliders)
        {
            if (collider.CompareTag("Ground") || collider.CompareTag("BrokenWall"))
                canJump = true;
        }

        return canJump;
    }

    void Crouch()
    {
        if (Input.GetButton("Crouch"))
            isCrouching = true;
        else if (Input.GetButtonUp("Crouch"))
            isCrouching = false;

        //Make player crouch
        if (canCrouch && isCrouching && transform.localScale.y > crouchHeight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y - 1 * crouchSpeed, 1);

            //Make sure the player will be exactly at crouchHeight when nearing it
            if (transform.localScale.y <= crouchHeight + .05f)
                transform.localScale = new Vector3(1, crouchHeight, 1);

            //If the player isn't airborne put player on the ground
            if (transform.position.y <= 1.1f)
                transform.position = new Vector3(transform.position.x, crouchPosY, transform.position.z);
        }
        //Make player stand up
        else if (!isCrouching && transform.localScale.y < standingHeight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y + 1 * crouchSpeed, 1);

            //Make sure the player will be exactly at standinHeight when nearing it
            if (transform.localScale.y >= standingHeight - .05f)
            {
                transform.localScale = new Vector3(1, standingHeight, 1);

                canCrouch = false;
                StartCoroutine(WaitForCrouchCooldown());
            }

            //If player isn't airborne put player on ground
            if (CanJump())
                transform.position = new Vector3(transform.position.x, standingPosY, transform.position.z);
        }
    }

    IEnumerator WaitForCrouchCooldown()
    {
        yield return new WaitForSeconds(crouchCooldown);

        canCrouch = true;
    }
}


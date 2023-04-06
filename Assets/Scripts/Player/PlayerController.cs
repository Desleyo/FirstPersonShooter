using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerCam;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Weapon weapon;

    [Space]
    [SerializeField] private float sensitivity;
    [SerializeField] private float camOffset = .65f;
    private float xRotation;
    private float yRotation;

    [Header("Move & Crouch variables")]
    [SerializeField] private float moveSpeed;
    [HideInInspector] public bool isMoving;
    private float horizontal;
    private float vertical;

    [SerializeField] private float crouchMoveSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchCooldown = .5f;
    [HideInInspector] public bool isCrouching = false;
    private bool canCrouch = true;

    [Header("Jump variables")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float normalSphereRadius;
    [SerializeField] private float crouchingSphereRadius;

    [Header("Standing & Crouch values")]
    [SerializeField] private float standingHeight = 1;
    [SerializeField] private float standingPosY = 1.01f;
    [SerializeField] private float crouchHeight = .5f;
    [SerializeField] private float crouchPosY = .55f;

    [Header("Recoil variables")]
    [SerializeField] private float recoilSpeed;
    [SerializeField] private float recoilClampX;
    [SerializeField] private float recoilClampY;
    [SerializeField] private float stopRecoilThreshold = .2f;
    private float recoilMaxX;
    private float recoilMaxY;
    private float recoilValueX;
    private float recoilValueY;
    private bool canIncreaseRecoil;

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

    private void Look()
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

    private void IncreaseRecoil()
    {
        float recoilTargetX = canIncreaseRecoil ? recoilMaxX : 0;
        float recoilTargetY = canIncreaseRecoil ? recoilMaxY : 0;

        recoilValueX = Mathf.Lerp(recoilValueX, recoilTargetX, 1 * Time.deltaTime * recoilSpeed);
        recoilValueY = Mathf.Lerp(recoilValueY, recoilTargetY, 1 * Time.deltaTime * recoilSpeed);

        if (!canIncreaseRecoil)
        {
            if (recoilValueY - stopRecoilThreshold < 0)
            {
                recoilMaxX = 0;
                recoilMaxY = 0;
            }
        }
        else if (!Input.GetButton("Fire1") || weapon.currentBulletCount == 0)
        {
            StartCoroutine(WaitToDecrease(.1f));
        }
    }

    private IEnumerator WaitToDecrease(float time)
    {
        yield return new WaitForSeconds(time);

        canIncreaseRecoil = false;
    }

    private void Move()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        isMoving = horizontal != 0 || vertical != 0;

        Vector3 dir = new Vector3(horizontal, 0, vertical);
        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = isCrouching ? crouchMoveSpeed : moveSpeed;
        rb.MovePosition(transform.position + transform.TransformDirection(dir) * speed * Time.deltaTime);
    }

    private void Jump()
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, finalRadius);
        foreach(Collider collider in colliders)
        {
            if (collider.CompareTag("Ground") || collider.CompareTag("BrokenWall"))
            {
                canJump = true;
            }
        }

        return canJump;
    }

    private void Crouch()
    {
        isCrouching = Input.GetButton("Crouch");

        //Make player crouch
        if (canCrouch && isCrouching && transform.localScale.y > crouchHeight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y - 1 * crouchSpeed, 1);

            //Make sure the player will be exactly at crouchHeight when nearing it
            if (transform.localScale.y <= crouchHeight + .05f)
            {
                transform.localScale = new Vector3(1, crouchHeight, 1);
            }

            //If the player isn't airborne put player on the ground
            if (transform.position.y <= 1.1f)
            {
                transform.position = new Vector3(transform.position.x, crouchPosY, transform.position.z);
            }
        }
        //Make player stand up
        else if (!isCrouching && transform.localScale.y < standingHeight)
        {
            transform.localScale = new Vector3(1, transform.localScale.y + 1 * crouchSpeed, 1);

            //Make sure the player will be exactly at standingHeight when nearing it
            if (transform.localScale.y >= standingHeight - .05f)
            {
                transform.localScale = new Vector3(1, standingHeight, 1);

                canCrouch = false;
                StartCoroutine(WaitForCrouchCooldown());
            }

            //If player isn't airborne put player on ground
            if (CanJump())
            {
                transform.position = new Vector3(transform.position.x, standingPosY, transform.position.z);
            }
        }
    }

    private IEnumerator WaitForCrouchCooldown()
    {
        yield return new WaitForSeconds(crouchCooldown);

        canCrouch = true;
    }
}


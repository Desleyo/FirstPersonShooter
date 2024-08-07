using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float runningSpeed;
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float groundDrag;
    private float moveSpeed;

    [Header("Walk")]
    [SerializeField] private float walkSpeed;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchScaleY;
    private float startScaleY;

    [Header("Jump")]
    [SerializeField] private float jumpForce;

    [Header("Ground Check")]
    [SerializeField] private float normalHeight;
    [SerializeField] private float groundBuffer;
    [SerializeField] private LayerMask groundLayer;
    private float playerHeight = 2;
    private bool isGrounded = true;

    [Space]
    [SerializeField] private Transform orientation;

    private float horizontal;
    private float vertical;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        startScaleY = transform.localScale.y;
    }

    private void Update()
    {
        GetMoveInput();
        MoveSpeedHandler();

        CheckIfGrounded();
        LimitSpeed();

        Crouch();
        Jump();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GetMoveInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    private void MoveSpeedHandler()
    {
        if (Input.GetButton("Crouch") && isGrounded)
        {
            moveSpeed = crouchSpeed;
        }
        else if (Input.GetButton("Walk") && isGrounded)
        {
            moveSpeed = walkSpeed;
        }
        else if (isGrounded)
        {
            moveSpeed = runningSpeed;
        }
    }

    private void CheckIfGrounded()
    {
        float distance = playerHeight / 2 + groundBuffer;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, distance, groundLayer);

        rb.drag = isGrounded ? groundDrag : 0;
    }

    private void Move()
    {
        Vector3 moveDir = orientation.right * horizontal + orientation.forward * vertical;
        rb.AddForce(moveDir.normalized * moveSpeed * speedMultiplier, ForceMode.Force);
    }

    private void LimitSpeed()
    {
        Vector3 velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (velocity.magnitude > moveSpeed)
        {
            Vector3 limitVelocity = velocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitVelocity.x, rb.velocity.y, limitVelocity.z);
        }
    }

    private void Crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            Vector3 localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x, crouchScaleY, localScale.z);

            playerHeight = crouchHeight;

            if (isGrounded)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            Vector3 localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x, startScaleY, localScale.z);

            playerHeight = normalHeight;
        }
    }

    private void Jump()
    {
        if (!Input.GetButtonDown("Jump") || !isGrounded)
            return;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    //Call this function to get the spreadmultiplier based on the player's velocity
    public float GetSpreadMultiplier()
    {
        float spreadMultiplier;

        if (!isGrounded)
        {
            //In-air spread multiplier
            spreadMultiplier = 10f;
        }
        else if (rb.velocity.magnitude == 0)
        {
            //If we're crouching we should divide the spread by 2, otherwise leave it at 1
            spreadMultiplier = Input.GetButton("Crouch") ? .5f : 1f;
        }
        else
        {
            spreadMultiplier = rb.velocity.magnitude;
        }

        return spreadMultiplier;
    }
}

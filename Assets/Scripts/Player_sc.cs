using UnityEngine;

public class Player_sc : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] int speed = 5;
    [SerializeField] int runSpeed = 8;

    [Header("Mouse Look")]
    [SerializeField] float sensitivity = 500f;
    [SerializeField] Transform cameraTransform;

    [Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float groundCheckDistance = 1.1f;
    [SerializeField] LayerMask groundLayer;

    [Header("Climb")]
    [SerializeField] float ledgeCheckDistance = 0.8f;
    [SerializeField] float maxClimbHeight = 1.5f;

    [Header("Respawn")]
    [SerializeField] Transform checkpoint;

    Rigidbody rb;
    float xRotation = 0f;
    float yRotation = 0f;
    Vector3 moveInput;

    bool isGrounded;
    bool isClimbing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CheckGround();
        GetInput();
        Rotate();

        if (Input.GetKeyDown(KeyCode.Space) && !isClimbing)
        {
            if (!TryClimb())
            {
                if (isGrounded)
                    Jump();
            }
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    private void GetInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0, v).normalized;
    }

    private void Move()
    {
        if (isClimbing) return;

        if (moveInput.magnitude == 0) return;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed;

        Vector3 moveDir = transform.TransformDirection(moveInput);
        rb.MovePosition(rb.position + moveDir * currentSpeed * Time.fixedDeltaTime);
    }

    private void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool TryClimb()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // Bütün objelere çarpması için layer maskesi kaldırıldı (Layer kontrolü yok)
        if (Physics.Raycast(origin, transform.forward, out RaycastHit wallHit, ledgeCheckDistance))
        {
            Vector3 topCheck = wallHit.point + transform.forward * 0.1f + Vector3.up * maxClimbHeight;

            if (Physics.Raycast(topCheck, Vector3.down, out RaycastHit topHit, maxClimbHeight + 0.5f))
            {
                float climbHeight = topHit.point.y - transform.position.y;

                if (climbHeight <= maxClimbHeight && climbHeight > 0.2f)
                {
                    StartCoroutine(Climb(topHit.point));
                    return true;
                }
            }
        }

        return false;
    }

    private System.Collections.IEnumerator Climb(Vector3 targetPoint)
    {
        isClimbing = true;
        rb.isKinematic = true;

        Vector3 startPos = transform.position;
        // Objeye çıkması gereken tam nokta (üstte ve çok az ileride)
        Vector3 finalPos = targetPoint + transform.forward * 0.3f + Vector3.up * 0.1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            // Smooth şekilde aşama aşama gitmesi için Lerp
            transform.position = Vector3.Lerp(startPos, finalPos, t);
            yield return null;
        }

        // Emin olmak için son pozisyona tam oturt
        transform.position = finalPos;

        rb.isKinematic = false;
        isClimbing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("trap") && checkpoint != null)
        {
            transform.position = checkpoint.position;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("trap") && checkpoint != null)
        {
            transform.position = checkpoint.position;
            rb.linearVelocity = Vector3.zero;
        }
    }
}
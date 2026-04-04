using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float fastSpeed = 5;
    public float verticalSpeed = 15f;
    public float extraSpeed = 1;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;

    private Rigidbody rb;

    private PlayerInput playerInput;
    private InputAction forward;
    private InputAction backward;
    private InputAction left;
    private InputAction right;
    private InputAction moveUp;
    private InputAction moveDown;
    private InputAction rotate;
    private InputAction mouseDelta;

    private float rotationX = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        var map = playerInput.actions.FindActionMap("MoveCamera");
        rotate = map.FindAction("Rotate");

        mouseDelta = playerInput.actions.FindAction("Look");
    }

    void OnEnable()
    {
        playerInput.actions.Enable();
    }

    void OnDisable()
    {
        playerInput.actions.Disable();
    }


    private void FixedUpdate()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDir -= transform.forward;
        if (Input.GetKey(KeyCode.D)) moveDir += transform.right;
        if (Input.GetKey(KeyCode.A)) moveDir -= transform.right;


        if (Input.GetKey(KeyCode.LeftShift)) extraSpeed = fastSpeed;
        else extraSpeed = 1f;

        if (moveDir.sqrMagnitude > 0.01f)
            moveDir.Normalize();

        rb.linearVelocity = moveDir * moveSpeed * extraSpeed;
    }
    void Update()
    {
        //HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDir -= transform.forward;
        if (Input.GetKey(KeyCode.D)) moveDir += transform.right;
        if (Input.GetKey(KeyCode.A)) moveDir -= transform.right;

        // �e ni gibanja, ne delaj capsulecasta
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Vector3 move = moveDir.normalized * moveSpeed * Time.deltaTime;

            // CapsuleCast le v smeri premika
            if (!Physics.CapsuleCast(
                transform.position + Vector3.up * 0.5f,
                transform.position + Vector3.up * 1.5f,
                0.5f,
                moveDir.normalized,
                move.magnitude))
            {
                rb.MovePosition(rb.position + move);
            }
        }
    }


    void HandleRotation()
    {
        if (!rotate.IsPressed())
            return;

        Vector2 mouse = Mouse.current.delta.ReadValue();

        float mouseX = mouse.x * mouseSensitivity * 0.1f;
        float mouseY = mouse.y * mouseSensitivity * 0.1f;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -85f, 85f);

        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y + mouseX, 0f);
    }
}

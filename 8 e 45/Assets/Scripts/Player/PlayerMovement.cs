using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag; //para o roce contra o suelo (non o vou aplicar no aire)

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    private bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public float crouchDownForce;
    private bool isCrouching;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Orientacion")]
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState movementState;
    public enum MovementState{ //estados do movimiento
        walking,
        sprinting,
        crouching,
        air
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;
        print(transform.lossyScale.y);
    }

    // Update is called once per frame
    void Update()
    {
        //comprobo se esta no suelo con un Raycast hacia abaixo, devolve true ou false
        // indico: de onde empeza(centro do player), en que direccion(abaixo), canto mide(a mita da altura do xogador e un pouco mas), e o layer no que vai actuar
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //manejo o roce se esta no suelo
        if(grounded){
            rb.linearDamping = groundDrag;
        }else{
            rb.linearDamping = 0;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //cando saltar
        if(Input.GetKey(jumpKey) && readyToJump && grounded){
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown); //volvo a poder saltar solo dps do cooldown
        }

        //agacharme
        if(Input.GetKey(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if(!isCrouching){
                rb.AddForce(Vector3.down * crouchDownForce, ForceMode.Impulse);
                isCrouching = !isCrouching;
                print("primeira vez q agacho");
            }
        }else{
            if(isCrouching){
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                isCrouching = !isCrouching;
                print("paro de agacharme");
            }
        }
    }

    private void MovePlayer(){
        //calculo a direccion do movimiento pa que siempre vaia pa onde miro
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //se estou no aire podome mover mais rapido
        if(grounded)
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void StateHandler(){
        //crouching
        if(grounded && Input.GetKey(crouchKey)){ //que se poda agachar no aire tamen pa que poda ir pa abaixo
            movementState = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        //sprinting
        else if(grounded && Input.GetKey(sprintKey)){
            movementState = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        //walking
        else if(grounded){
            movementState = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        //air
        else{
            movementState = MovementState.air;
        }
    }

    private void SpeedControl(){
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        //se vou mais rapido que a moveSpeed, calculo a velocidad maxima e aplicolla
        if(flatVelocity.magnitude > moveSpeed){
            Vector3 limitVel = flatVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitVel.x, rb.linearVelocity.y, limitVel.z);
        }
    }

    private void Jump(){
        //reseteo a velocidad en y para siempre saltar o mismo
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump(){
        readyToJump = true;
    }
}

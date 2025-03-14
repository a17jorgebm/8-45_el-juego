using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float groundDrag; //para o roce contra o suelo (non o vou aplicar no aire)
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float wallRunSpeed;
    public float slideSpeed;
    private float desiredMovementSpeed;
    private float lastDesiredMovementSpeed; //para poder ir frenando pouco a pouco

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    [Header("State")]
    public bool sliding;
    public bool isCrouching;
    public bool wallrunning;
    public bool freeze;
    public bool activeGrapple;

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

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")] //para gestionar cando sube por pendientes, porque senon ao facer a force pa adiante non da
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope; //para poder saltar nas pendientes

    [Header("Orientation")]
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
        sliding,
        wallrunning,
        freeze, //para cando uso o gancho e me quedo parado un nada
        air
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        //comprobo se esta no suelo con un Raycast hacia abaixo, devolve true ou false
        // indico: de onde empeza(centro do player), en que direccion(abaixo), canto mide(a mita da altura do xogador e un pouco mas), e o layer no que vai actuar
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //manejo o roce se esta no suelo, se esta usando o gancho desactivoo
        if(grounded && !activeGrapple){
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
            }
        }else{
            if(isCrouching){
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                isCrouching = !isCrouching;
            }
        }
    }

    private void MovePlayer(){
        //se esta usando o gancho pa moverse desactivo esto
        if(activeGrapple) return;

        //calculo a direccion do movimiento pa que siempre vaia pa onde miro
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(OnSlope() && !exitingSlope){
            rb.AddForce(10f * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);
            if(rb.linearVelocity.y > 0){
                rb.AddForce(40f * Vector3.down, ForceMode.Force);
            }
        }
        //se estou no aire podome mover mais rapido
        else if(grounded)
            rb.AddForce(10f * moveSpeed * moveDirection, ForceMode.Force);
        else
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection, ForceMode.Force);

        //se estou en pendiente, desactivo a gravedad pa que non caia, sempre que non estea wallrunning
        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    private void StateHandler(){
        //freeze
        if(freeze){
            movementState = MovementState.freeze;
            moveSpeed = 0;
            rb.linearVelocity = Vector3.zero;
        }
        //wallrunning
        else if(wallrunning){
            movementState = MovementState.wallrunning;
            desiredMovementSpeed = wallRunSpeed;
        }
        //sliding
        else if(sliding){
            movementState = MovementState.sliding;

            if(OnSlope() && rb.linearVelocity.y < 0.1f){
                desiredMovementSpeed = slideSpeed;
            }else{
                desiredMovementSpeed = sprintSpeed;
            }
        }
        //crouching
        else if(grounded && Input.GetKey(crouchKey)){ //que se poda agachar no aire tamen pa que poda ir pa abaixo
            movementState = MovementState.crouching;
            desiredMovementSpeed = crouchSpeed;
        }
        //sprinting
        else if(grounded && Input.GetKey(sprintKey)){
            movementState = MovementState.sprinting;
            desiredMovementSpeed = sprintSpeed;
        }
        //walking
        else if(grounded){
            movementState = MovementState.walking;
            desiredMovementSpeed = walkSpeed;
        }
        //air
        else{
            movementState = MovementState.air;
        }

        //comprobo se a velocidad cambiou mui bruscamente
        if(Mathf.Abs(desiredMovementSpeed - lastDesiredMovementSpeed) > 4f && moveSpeed != 0){
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }else{
            moveSpeed = desiredMovementSpeed;
        }
        lastDesiredMovementSpeed = desiredMovementSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMovementSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMovementSpeed;
    }

    private void SpeedControl(){
        //se esta usando o gancho pa moverse desactivo esto
        if(activeGrapple) return;

        if(OnSlope() && !exitingSlope){
            if(rb.linearVelocity.magnitude > moveSpeed){
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else{
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            //se vou mais rapido que a moveSpeed, calculo a velocidad maxima e aplicolla
            if(flatVelocity.magnitude > moveSpeed){
                Vector3 limitVel = flatVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitVel.x, rb.linearVelocity.y, limitVel.z);
            }
        }
    }

    private void Jump(){
        exitingSlope =true;
        //reseteo a velocidad en y para siempre saltar o mismo
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump(){
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope(){
        if(Physics.Raycast(transform.position, Vector3.down,out slopeHit, playerHeight * 0.5f + 0.3f)){
            //.normal devolve a direccion a que apunta o elemento
            //calculo o angulo entre up(suelo plano) e o elemento no que esta o xogador (slopeHit.normal)
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }


    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight){
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity),0.1f);

        Invoke(nameof(ResetRestrictions),3f); //por si acaso algo vai mal
    }

    private Vector3 velocityToSet;

    private void SetVelocity(){
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }

    public void ResetRestrictions(){
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(enableMovementOnNextTouch){
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<Grappling>().StopGrappling();
        }
    }

    //devolvo a direccion na que teño que ir según a pendiente na que estou
    public Vector3 GetSlopeMoveDirection(Vector3 direction){
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    //obviamente estas mates se me escapan, pero é para calcular a forza que teño que aplicar para atraer o xogador hasta o destino do gancho
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}

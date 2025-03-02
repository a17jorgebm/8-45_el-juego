using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;
    private bool upwardsRunning;
    private bool downwardsRunning;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    //para arreglar o bug de ao saltar que se pegue outra vez ao muro
    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(pm.wallrunning){
            WallRunningMovement();
        }
    }

    private void CheckForWall(){
        //lanzo un rayo a dereita e esquerda pa saber si estou chocando con un muro
        wallRight= Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft= Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround(){
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine(){
        //inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        //wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall){
            if(!pm.wallrunning) StartWallRun();

            if(wallRunTimer > 0) wallRunTimer-=Time.deltaTime;

            if(wallRunTimer <= 0){
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if(Input.GetKeyDown(jumpKey)) WallJump();       
        }
        else if(exitingWall){
            //esto usoo para indicara que non se inicie outro startWallrunning
            //se esta wallruning paroo, e por cada frame restolle o tempo que pasou ao timer, se acaba poño o estado a false outra vez
            if(pm.wallrunning) StopWallRun();
            if(exitWallTimer > 0) exitWallTimer-=Time.deltaTime;
            if(exitWallTimer<=0) exitingWall=false;
        }
        else{
            if(pm.wallrunning){
                StopWallRun();  
            }  
        }
    }

    private void StartWallRun(){
        pm.wallrunning = true;
        wallRunTimer = maxWallRunTime;

        //poño a velocidad vertical a 0, pero solo campo empezo
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    private void WallRunningMovement(){
        //desactivo gravedad
        rb.useGravity = useGravity;

        //okey parte complicada, porque teño q entrar a direccion fordward do muro, 
        // e que me dea igual como estea rotado (faise con Vector3.Cross), non teño tempo eu pa entender ben estas cousas
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        //teño que facer esto porque senon ao ir pola dereita vaime para atras e non pa adiante
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        //aplicolle a forza
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //correr hacia arriba ou abaixo do muro
        if(upwardsRunning){
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        }
        if(downwardsRunning){
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);
        }

        //empujoo contra o muro para que poda ir por fora en paredes curvas, sempre que non estea intenando salir do muro con a,d
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0)){
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }

        //para contrarestar gravedad e que non me tire muito pa abaixo
        if(useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun(){
        pm.wallrunning = false;
    }

    private void WallJump(){
        //inicio o estado de exitingwall
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallHit.normal;

        //xunto a forza de empuje pa arriba como a de empuje para o lado
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        //de novo, pa que o salto se sinta mellor e sempre sea o mismo, reseteo a velocidad en y do xogador
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        //añado a forza
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}

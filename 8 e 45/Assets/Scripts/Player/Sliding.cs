using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        startYScale = playerObj.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); //a, d
        verticalInput = Input.GetAxisRaw("Vertical");//w,s

        if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0)){
            StartSlide();
        }

        if(Input.GetKeyUp(slideKey) && pm.sliding){
            StopSlide();
        }
    }

    void FixedUpdate()
    {
        if(pm.sliding){
            SlidingMovement();
        }
    }

    private void SlidingMovement(){
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //sliding normal
        if(!pm.OnSlope() || rb.linearVelocity.y > -0.1f){
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        //baixando por unha rampa, aqui ademais non lle baixo ao temporizador pa que poda ser infinito
        else{
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if(slideTimer <= 0){
            StopSlide();
        }
    }

    private void StartSlide(){
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, playerObj.localScale.y * slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void StopSlide(){
        pm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}

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

    private bool sliding;

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

        if(Input.GetKeyUp(slideKey) && sliding){
            StopSlide();
        }
    }

    void FixedUpdate()
    {
        if(sliding){
            SlidingMovement();
        }
    }

    private void SlidingMovement(){
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        slideTimer -= Time.deltaTime;

        if(slideTimer <= 0){
            StopSlide();
        }
    }

    private void StartSlide(){
        sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, playerObj.localScale.y * slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void StopSlide(){
        sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}

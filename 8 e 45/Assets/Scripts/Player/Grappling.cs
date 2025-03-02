using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;

    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;
    private Vector3 grapplePoint;

    [Header("CoolDown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(grappleKey)){
            StartGrappling();
        }

        if(grapplingCdTimer > 0){
            grapplingCdTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if(grappling){
            lr.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrappling(){
        //se esta en cd non fago nada
        if(grapplingCdTimer > 0) return;

        grappling = true;

        pm.freeze = true;

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable)){
            grapplePoint = hit.point;
            //ejecuto o grapple despois de esperar o delay
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else{
            //senon poño o punto no maximo que pode ter, esto queda raro
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            Invoke(nameof(StopGrappling),grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }


    //fai falta unha carreira de matematicas pa entender esto
    private void ExecuteGrapple(){
        pm.freeze = false;

        //basicamente calcula o camiño, aplicando o arco indicado nos parametros
        Vector3 lowestPoint = new Vector3(transform.position.x,transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if(grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);
    }

    public void StopGrappling(){
        pm.freeze = false;
        grappling = false;
        grapplingCdTimer = grapplingCd; //empezo o timer para o cd

        lr.enabled = false;
    }
}

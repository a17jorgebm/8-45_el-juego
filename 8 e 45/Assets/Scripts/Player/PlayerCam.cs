using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    //faime falta para facer o de tumbar a camara mentras fago wallrunning, porque senon estaría sobreescribindo o giro da camara 2 veces e non iría
    public Transform camHolder; 

    float xRotation;
    float yRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //pillo o input do rato
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation+=mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //roto o camholder en vez da camara, para en DoTile poder rotar a camara
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        //roto o xogador
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }

    public void DoFov(float endValue){
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt){
        transform.DOLocalRotate(new Vector3(0,0,zTilt),0.25f);
    }
}

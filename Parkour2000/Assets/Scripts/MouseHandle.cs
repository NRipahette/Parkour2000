using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHandle : MonoBehaviour
{
    [SerializeField]
    public float sensitivity = 5.0f;
    [SerializeField]
    public float smoothing = 2.0f;
    public GameObject PlayerOBJ;
    private Vector2 mouseLook;
    private Vector2 smoothV;
    private GameObject Camera;

    void Start()
    {
        Camera = this.transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0.001f)
        {
            //Mouse delta
            var md = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
            md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
            // incrementally add to the camera look
            mouseLook += smoothV;

            mouseLook.y = Mathf.Clamp(mouseLook.y, -65f, 80f);

            // vector3.right means the x-axis
            transform.localEulerAngles = new Vector3(mouseLook.y, 0, 0);
            PlayerOBJ.transform.localEulerAngles = new Vector3(0, mouseLook.x, 0);
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 PlayerVelocity;
    public float GravtiyForce = 20f;
    public float GroundCheckDistance = 0.1f;
    public float JumpForce = 90f;
    public float VitesseMax = 10f;
    public float Friction = 20f;
    public float AirAcceleration = 1f;
    public float SprintMultiplier = 2.5f;
    float pSpeed;
    public bool IsGrounded;
    public bool WasGrounded;
    public bool IsSprinting;
    public bool IsOnSlope;
    private float CheckGroundDelay = 0.05f;
    private Vector3 m_GroundNormal;
    private float m_LastTimeJumped;
    private float SlopeForce = 600f;
    private float SlopeForceRayLength = 1f;
    private CharacterController playerController;

    // Start is called before the first frame update
    void Start()
    {
        IsGrounded = true;
        WasGrounded = true;
        IsSprinting = false;
        IsOnSlope = false;
        playerController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            IsSprinting = true;
        }
        else
        {
            IsSprinting = false;
        }

        // calculate the desired velocity from inputs, max speed, and current slope
        Vector3 targetForwardVelocity = Input.GetAxis("Vertical") * transform.forward * VitesseMax;

        Vector3 targetLateralVelocity = Input.GetAxis("Horizontal") * transform.right * VitesseMax;

        //Vector3 targetUpwardVelocity = JumpForce *  Vector3.up ;
        Vector3 targetVelocity = targetForwardVelocity + targetLateralVelocity;


        GroundCheck();
        if (WasGrounded || IsGrounded)
        {

            if (IsSprinting)
            {
                targetForwardVelocity *= SprintMultiplier;
                targetVelocity = targetForwardVelocity + targetLateralVelocity;                
                PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
            }
            else
            {
                PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
            }
            // start by canceling out the vertical component of our velocity
            PlayerVelocity = new Vector3(PlayerVelocity.x, 0f, PlayerVelocity.z);
            //Jump        
            if (Input.GetKeyDown(KeyCode.Space))
            {

                // then, add the jumpSpeed value upwards
                PlayerVelocity += Vector3.up * JumpForce;
                m_LastTimeJumped = Time.time;
                IsGrounded = false;
            }

            // Slope Check 
            if (m_GroundNormal != Vector3.up)
            {
                IsOnSlope = true;
                PlayerVelocity += Vector3.down * SlopeForce * Time.deltaTime;
            }
            else
                IsOnSlope = false;

            

            
           
        }
        else
        {

            PlayerVelocity += new Vector3(targetVelocity.x * Time.deltaTime * AirAcceleration, 0, targetVelocity.z * Time.deltaTime * AirAcceleration);
            //Appliquer la gravité
            PlayerVelocity += Vector3.down * GravtiyForce * Time.deltaTime;



        }
        playerController.Move(PlayerVelocity * Time.deltaTime);
        WasGrounded = IsGrounded;
    }

    void GroundCheck()
    {

        // reset values before the ground check
        IsGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + CheckGroundDelay)
        {
            Debug.DrawLine(transform.position - Vector3.up, (transform.position - Vector3.up) + Vector3.down * (GroundCheckDistance + (PlayerVelocity * Time.deltaTime).magnitude), Color.red, 10f);
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.Raycast(transform.position - Vector3.up, Vector3.down, out RaycastHit hit, GroundCheckDistance + (PlayerVelocity * Time.deltaTime).magnitude))
            {
                IsGrounded = true;
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f)
                {


                    //// handle snapping to the ground
                    //if (hit.distance > m_Controller.skinWidth)
                    //{
                    //    m_Controller.Move(Vector3.down * hit.distance);
                    //}
                }
            }
        }
    }

    //Calcul de pente 
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

}

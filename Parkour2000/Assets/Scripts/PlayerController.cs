using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 PlayerVelocity;
    public float playerHeight = 2f;
    public float playerwidth = 1f;
    public float GravtiyForce = 20f;
    public float GroundCheckDistance = 0.1f;
    public float WallCheckDistance = 0.5f;
    public float JumpForce = 90f;
    public float WallRunForce = 10f;
    public float VitesseMax = 10f;
    public float VitesseMaxWallRun = 8f;
    public float Friction = 20f;
    public float WallFriction = 10f;
    public float AirAcceleration = 1f;
    public float SprintMultiplier = 2.5f;
    public bool IsGrounded;
    public bool WasGrounded;
    public bool IsSprinting;
    public bool IsOnSlope;
    public bool IsTouchingAWall;
    public bool IsAttachedToWall;
    public bool IsJumping;
    private bool useGravity;
    private float CheckGroundDelay = 0.05f;
    private Vector3 m_GroundNormal;
    private float m_LastTimeJumped;
    private float SlopeForce = 600f;
    private float SlopeForceRayLength = 1f;
    private CharacterController playerController;
    private BoxCollider WallRunHitboxCollider;
    private Collider wallHit;
    private RaycastHit RayWall;
    private GameObject cameraFPS;

    // Start is called before the first frame update
    void Start()
    {
        IsGrounded = true;
        WasGrounded = true;
        IsSprinting = false;
        IsOnSlope = false;
        useGravity = true;
        IsJumping = false;
        WallRunHitboxCollider = transform.Find("WallRunHitbox").GetComponent<BoxCollider>();
        playerController = GetComponent<CharacterController>();
        cameraFPS = GameObject.Find("Main Camera");
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
        if (IsGrounded)
        {
            useGravity = false;
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
                IsJumping = true;
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
        else if (IsAttachedToWall)
        {

            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, transform.forward * Input.GetAxis("Vertical") * VitesseMaxWallRun, Time.deltaTime * WallFriction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, transform.forward * Input.GetAxis("Vertical") * VitesseMaxWallRun, Time.deltaTime * WallFriction).z);
            //PlayerVelocity += targetForwardVelocity * WallRunForce * Time.deltaTime;
            //PlayerVelocity += -RayWall.normal * WallRunForce / 5 * Time.deltaTime; // Colle le joueur au mur
            PlayerVelocity = Vector3.ProjectOnPlane(PlayerVelocity, RayWall.normal); // Colle le joueur au mur
            //PlayerVelocity = Vector3.ClampMagnitude(PlayerVelocity, VitesseMaxWallRun);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(Vector3.Dot(cameraFPS.transform.forward, Vector3.up) > 0.866f) // Si regard vers le haut alors saute uniquement vers le haut proche du mur
                {
                    PlayerVelocity += (Vector3.up * JumpForce);

                }
                else
                {
                    PlayerVelocity += RayWall.normal * JumpForce + (Vector3.up * JumpForce); // Sinon saute en s'éloignant du mur
                }
                useGravity = true;
                IsAttachedToWall = false;
                wallHit = null;
            }

        }
        else if (IsTouchingAWall)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartWallRun(wallHit);

            }
        }
        else
        {
            IsAttachedToWall = false;
            PlayerVelocity += new Vector3(targetVelocity.x * Time.deltaTime * AirAcceleration, 0, targetVelocity.z * Time.deltaTime * AirAcceleration);
            //Gravité
            PlayerVelocity += Vector3.down * GravtiyForce * Time.deltaTime;
        }

        playerController.Move(PlayerVelocity * Time.deltaTime);
        HeadBumpCheck();
        WasGrounded = IsGrounded;
    }


    void GroundCheck()
    {
        IsJumping = false;
        // reset values before the ground check
        IsGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + CheckGroundDelay)
        {
            Debug.DrawLine(transform.position - Vector3.up, (transform.position - Vector3.up * playerHeight / 2) + Vector3.down * (GroundCheckDistance + (PlayerVelocity * Time.deltaTime).magnitude), Color.red, 10f);
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
    void HeadBumpCheck()
    {
        Debug.DrawLine(transform.position + Vector3.up, (transform.position + Vector3.up * playerHeight / 2) + Vector3.up * GroundCheckDistance, Color.red, 10f);
        // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.up, out RaycastHit hit, GroundCheckDistance))
        {
            PlayerVelocity = new Vector3(PlayerVelocity.x, -0.5f, PlayerVelocity.z);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("obstacle"))
        {
            wallHit = other;
            IsTouchingAWall = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("obstacle"))
        {
            IsTouchingAWall = false;

        }
    }


    //void WallCheck()
    //{

    //    // reset values before the ground check
    //    IsTouchingAWall = false;
    //    AttachedWallNormal = Vector3.zero;

    //    Debug.DrawLine(transform.position, transform.position + transform.right * (playerwidth / 2 + WallCheckDistance), Color.blue, 10f);
    //    Debug.DrawLine(transform.position, transform.position - transform.right * (playerwidth / 2 + WallCheckDistance), Color.blue, 10f);
    //    // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
    //    if (wallHit)
    //    {
    //        IsTouchingAWall = true;
    //        // storing the normal of the wall we collided to
    //        AttachedWallNormal = hit.normal;

    //        // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
    //        // and if the slope angle is lower than the character controller's limit
    //        if (Vector3.Dot(hit.normal, transform.up) > 0f)
    //        {


    //            //// handle snapping to the ground
    //            //if (hit.distance > m_Controller.skinWidth)
    //            //{
    //            //    m_Controller.Move(Vector3.down * hit.distance);
    //            //}
    //        }
    //    }
    //    else if (IsWallLeft)
    //    {
    //        IsTouchingAWall = true;
    //        // storing the normal of the wall we collided to
    //        AttachedWallNormal = hit2.normal;
    //    }
    //    else
    //        StopWallRun();


    //}

    void StartWallRun(Collider wallHit)
    {
        IsAttachedToWall = true;
        useGravity = false;
        PlayerVelocity = new Vector3(PlayerVelocity.x, 0, PlayerVelocity.z);
        wallHit.Raycast(new Ray(transform.position, wallHit.ClosestPoint(transform.position) - transform.position), out RayWall, 5f);
        if (PlayerVelocity.magnitude <= VitesseMaxWallRun)
        {
            PlayerVelocity += transform.forward * WallRunForce * Time.deltaTime;

            PlayerVelocity -= RayWall.normal * WallRunForce / 5 * Time.deltaTime;
        }
    }

    void StopWallRun()
    {
        IsAttachedToWall = false;
        useGravity = true;
    }

    //Calcul de pente 
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

}


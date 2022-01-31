using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //public Vector3 PlayerVelocity;
    public float playerHeight = 2f;
    public float playerwidth = 1f;
    public float GravtiyForce = 20f;
    //public float GroundCheckDistance = 0.1f;
    public float WallCheckDistance = 0.5f;
    //public float JumpForce = 90f;
    public float WallRunForce = 10f;
    //public float VitesseMax = 10f;
    public float VitesseMaxWallRun = 8f;
    //public float Friction = 20f;
    public float WallFriction = 10f;
    //public float AirAcceleration = 1f;
    //public float SprintMultiplier = 2.5f;
    public bool IsGrounded;
    public bool IsSprinting;
    public bool IsOnSlope;
    public bool IsTouchingAWall;
    public bool IsAttachedToWall;
    public bool IsJumping;
    public bool IsCrouching;
    public bool IsSliding;
    private bool useGravity;
    //private float CheckGroundDelay = 0.05f;
    //private Vector3 m_GroundNormal;
    //private float m_LastTimeJumped;
    //private float SlopeForce = 600f;
    //private float SlopeForceRayLength = 1f;
    private CharacterController playerController;
    private BoxCollider WallRunHitboxCollider;
    private Collider wallHit;
    private RaycastHit RayWall;
    private GameObject cameraFPS;
    private GameObject Spawn;
    private GameObject pauseMenu;
    public Vector3 PlayerVelocity;
    public float CurrentSpeed;
    public float GravityForce = 20f;
    public float GroundCheckDistance = 0.1f;
    public float JumpForce;
    public float baseJumpForce;
    public float SlopeJumpForce;
    public float VitesseMax = 25f;
    public float Friction;
    public float SlidingFriction;
    public float baseFriction;
    public float AirAcceleration = 1f;
    public float SprintMultiplier = 2.5f;
    public float CrouchMultiplier = 0.5f;
    public float SlidingMultiplier = 2f;
    private float standingHeight = 2f;
    private Vector3 standingCenter = new Vector3(0, 0, 0);
    private float crouchingHeight = 1f;
    private Vector3 crouchingCenter = new Vector3(0, -0.5f, 0);
    private float slidingHeight = 1f;
    private Vector3 slidingCenter = new Vector3(0, -0.5f, 0);
    public float interpolationCrouchFrames = 45f;
    public Vector3 crouchingView;
    public Vector3 slidingView;
    public Vector3 standingView;
    public bool WasGrounded;
    private bool startedSliding;
    private float CheckGroundDelay = 0.05f;
    public Vector3 m_GroundNormal;
    private float m_LastTimeJumped;
    public float SlopeForce = 1000f;
    public GameObject playerView;
    private float slidingStartTime;
    public float slidingLength;
    float elapsedFrames;

    public Vector3 temp;
    public Vector3 groundSlopeDir;
    public float groundSlopeAngle;







    private Vector3 targetForwardVelocity;
    private Vector3 targetLateralVelocity;
    private Vector3 targetVelocity;

    // Start is called before the first frame update
    void Start()
    {
        IsGrounded = true;
        IsSprinting = false;
        IsOnSlope = false;
        useGravity = true;
        IsJumping = false;
        WallRunHitboxCollider = transform.Find("WallRunHitbox").GetComponent<BoxCollider>();
        playerController = GetComponent<CharacterController>();
        cameraFPS = GameObject.Find("Main Camera");
        Spawn = GameObject.Find("SpawnPosition");
        pauseMenu = GameObject.Find("PauseMenu");
        pauseMenu.SetActive(false);
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
        targetForwardVelocity = Input.GetAxis("Vertical") * transform.forward * VitesseMax;
        targetLateralVelocity = Input.GetAxis("Horizontal") * transform.right * VitesseMax;

        Input_Check_Slide();
        Input_Check_Crouch();

        //Vector3 targetUpwardVelocity = JumpForce *  Vector3.up ;
        targetVelocity = targetForwardVelocity + targetLateralVelocity;

        GroundCheck();
        if (IsGrounded)
        {
            Crouch_Check();
            Sprint_Check();
            Slide_Check();

            // start by canceling out the vertical component of our velocity
            PlayerVelocity = new Vector3(PlayerVelocity.x, 0f, PlayerVelocity.z);
            Slope_Check();
            Input_Check_Jump();
            //useGravity = false;
            //GroundedMovement();            
        }
        else if (IsAttachedToWall)
        {
            WallRunMovement();


        }
        else if (IsTouchingAWall)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                
                StartWallRun(wallHit);

            }
            //Gravité
            PlayerVelocity += Vector3.down * GravtiyForce * Time.deltaTime;
        }
        else
        {
            AirMovement();
        }

        playerController.Move(PlayerVelocity * Time.deltaTime);
        HeadBumpCheck();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    void Input_Check_Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // start by canceling out the vertical component of our velocity
            PlayerVelocity = new Vector3(PlayerVelocity.x, 0f, PlayerVelocity.z);

            // then, add the jumpSpeed value upwards
            PlayerVelocity += Vector3.up * JumpForce;
            m_LastTimeJumped = Time.time;
            IsGrounded = false;
        }
    }

    void Slope_Check()
    {
        if (m_GroundNormal != Vector3.up && IsGrounded)
        {
            IsOnSlope = true;
            //JumpForce = SlopeJumpForce;
            PlayerVelocity += Vector3.down * SlopeForce * Time.deltaTime;
            //PlayerVelocity = Vector3.ProjectOnPlane(PlayerVelocity, m_GroundNormal);
        }
        else
        {
            IsOnSlope = false;
            //JumpForce = baseJumpForce;
        }
    }

    void Sprint_Check()
    {
        if (IsSprinting)
        {
            targetForwardVelocity *= SprintMultiplier;
            targetVelocity = targetForwardVelocity + targetLateralVelocity;
            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);

        } else{
            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
        }
    }

    private void GroundedMovement()
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

    private void WallRunMovement()
    {
        if (!IsTouchingAWall)
        {
            IsAttachedToWall = false;
        }
        else
        {

            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, transform.forward * Input.GetAxis("Vertical") * VitesseMaxWallRun, Time.deltaTime * WallFriction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, transform.forward * Input.GetAxis("Vertical") * VitesseMaxWallRun, Time.deltaTime * WallFriction).z);
            PlayerVelocity = Vector3.ProjectOnPlane(PlayerVelocity, RayWall.normal); // Colle le joueur au mzur
                                                                                     //PlayerVelocity = Vector3.ClampMagnitude(PlayerVelocity, VitesseMaxWallRun);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Vector3.Dot(cameraFPS.transform.forward, Vector3.up) > 0.756f) // Si regard vers le haut alors saute uniquement vers le haut proche du mur
                {
                    PlayerVelocity += (Vector3.up * JumpForce * 1.5f);

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
    }

    private void AirMovement()
    {
        IsAttachedToWall = false;
        PlayerVelocity += new Vector3(targetVelocity.x * Time.deltaTime * AirAcceleration, 0, targetVelocity.z * Time.deltaTime * AirAcceleration);
        //Gravité
        PlayerVelocity += Vector3.down * GravtiyForce * Time.deltaTime;
    }

    private void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        GameObject.Find("PlayingHUD").SetActive(false);
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
                if (hit.transform.name != "RespawnCollider")
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

    void Input_Check_Crouch()
    {
        if (Input.GetKey(KeyCode.W) && !IsSliding)
        {
            IsCrouching = true;

        }
        else
        {
            IsCrouching = false;
        }
    }

    void Input_Check_Slide()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !IsCrouching && IsGrounded)
        {
            IsSliding = true;
        }
    }

    void Crouch_Check()
    {
        float t;
        if (IsCrouching)
        {
            t = (float)elapsedFrames / interpolationCrouchFrames;
            playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, crouchingView, 0.01f);
            elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);

            playerController.height = crouchingHeight;
            playerController.center = crouchingCenter;
        }
        else if (!IsSliding && playerView.transform.localPosition.y < standingView.y - 0.2f)
        {
            t = (float)elapsedFrames / interpolationCrouchFrames;
            playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, standingView, 0.01f);
            elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);

            playerController.height = standingHeight;
            playerController.center = standingCenter;
        }

        if (IsCrouching)
        {

            targetForwardVelocity *= CrouchMultiplier;
            targetLateralVelocity *= CrouchMultiplier;
            targetVelocity = targetForwardVelocity + targetLateralVelocity;
            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);

        }
        else
        {
            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
        }
    }

    void Slide_Check()
    {
        float t;
        if (IsSliding && !startedSliding)
        {
            slidingStartTime = Time.time;
            startedSliding = true;

            playerController.height = slidingHeight;
            playerController.center = slidingCenter;

            t = (float)elapsedFrames / interpolationCrouchFrames;
            playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, slidingView, 0.01f);
            elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);

            targetForwardVelocity *= SlidingMultiplier;
            targetVelocity = targetForwardVelocity + targetLateralVelocity;
            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);

        }
        else if (startedSliding && Time.time - slidingStartTime < slidingLength)
        {
            t = (float)elapsedFrames / interpolationCrouchFrames;
            playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, slidingView, 0.01f);
            elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);

            targetForwardVelocity *= SlidingMultiplier;
            targetVelocity = targetForwardVelocity + targetLateralVelocity;
            PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
        }
        else if (startedSliding && Time.time - slidingStartTime >= slidingLength)
        {
            startedSliding = false;
            IsSliding = false;

            playerController.height = standingHeight;
            playerController.center = standingCenter;

            t = (float)elapsedFrames / interpolationCrouchFrames;
            playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, standingView, 0.01f);
            elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);
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

    public void Respawn()
    {
        SceneManager.GetActiveScene(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    void StartWallRun(Collider wallHit)
    {
        IsAttachedToWall = true;
        useGravity = false;
        PlayerVelocity = new Vector3(PlayerVelocity.x, 0, PlayerVelocity.z);
        if(wallHit != null)
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Vector3 PlayerVelocity;
	public float CurrentSpeed;
	public float GravityForce = 20f;
	public float GroundCheckDistance = 0.1f;
	public float JumpForce;
	public float baseJumpForce;
	public float SlopeJumpForce;
	public float VitesseMax = 10f;
	public float Friction;
	public float SlidingFriction;
	public float baseFriction;
	public float AirAcceleration = 1f;
	public float SprintMultiplier = 2.5f;
	public float CrouchMultiplier = 0.5f;
	public float SlidingMultiplier = 2f;
	private float crouchingHeight = 1.5f;
	private Vector3 standingCenter = new Vector3(0, 0, 0);
	private float standingHeight = 2f;
	private Vector3 crouchingCenter = new Vector3(0, -0.25f, 0);
	private float slidingHeight = 1f;
	private Vector3 slidingCenter = new Vector3(0, -0.5f, 0);
	public float interpolationCrouchFrames = 45f;
	public Vector3 crouchingView;
	public Vector3 slidingView;
	public Vector3 standingView;
	public bool IsGrounded;
	public bool WasGrounded;
	public bool IsSprinting;
	public bool IsCrouching;
	public bool IsSliding;
	public bool IsOnSlope;
	private bool startedSliding;
	private float CheckGroundDelay = 0.05f;
	public Vector3 m_GroundNormal;
	private float m_LastTimeJumped;
	public float SlopeForce = 1000f;
	private float SlopeForceRayLength = 1f;
	private CharacterController playerController;
	public GameObject playerView;
	public Material slide_m;
	public Material normal_m;
	private MeshRenderer renderer;
	private float slidingStartTime;
	public float slidingLength;
	float elapsedFrames;

	public Vector3 temp;
	public Vector3 groundSlopeDir;
	public float groundSlopeAngle;

	// Start is called before the first frame update
	void Start()
	{
		IsGrounded = true;
		WasGrounded = true;
		IsSprinting = false;
		IsOnSlope = false;
		playerController = GetComponent<CharacterController>();
		renderer = GetComponent<MeshRenderer>();
		IsCrouching = false;
		IsSliding = false;
		Friction = baseFriction;
		elapsedFrames = float.Epsilon;
		startedSliding = false;
	}

	// Update is called once per frame
	void Update()
	{
		CurrentSpeed = Mathf.Abs(PlayerVelocity.magnitude);
		if (Input.GetKey(KeyCode.LeftShift) && !IsSliding)
		{
			IsSprinting = true;
		}
		else
		{
			IsSprinting = false;
		}

		if (Input.GetKey(KeyCode.W) && !IsSliding)
		{
			IsCrouching = true;

		}
		else
		{
			IsCrouching = false;
		}

		if (Input.GetKeyDown(KeyCode.LeftAlt) && !IsCrouching)
		{
			IsSliding = true;
		}


		//if (IsSliding && Time.time - slidingStartTime >= slidingLength)
		//{
		//	float t = slidingLength / 10.0f;
		//	Friction = baseFriction;
		//	IsSliding = false;
		//	CurrentSpeed = VitesseMax * CrouchMultiplier;
		//	renderer.material = normal_m;
		//	Friction = Mathf.Lerp(Friction, baseFriction, 0.001f);
		//}
		////if (IsSliding && Friction > baseFriction - 0.01f)
		////{
		////	IsSliding = false;
		////	Friction = baseFriction;
		////	renderer.material = normal_m;
		////}
		//else if (CurrentSpeed >= VitesseMax * SlidingMultiplier && IsCrouching && !IsSliding)
		//{
		//	slidingStartTime = Time.time;
		//	IsSliding = true;
		//	Friction = SlidingFriction;
		//	renderer.material = slide_m;
		//}




		// calculate the desired velocity from inputs, max speed, and current slope
		Vector3 targetForwardVelocity = Input.GetAxis("Vertical") * transform.forward * VitesseMax;

		Vector3 targetLateralVelocity = Input.GetAxis("Horizontal") * transform.right * VitesseMax;

		//Vector3 targetUpwardVelocity = JumpForce *  Vector3.up ;
		Vector3 targetVelocity = targetForwardVelocity + targetLateralVelocity;


		float t;
		if (IsCrouching)
		{
			t = (float)elapsedFrames / interpolationCrouchFrames;
			playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, crouchingView, 0.01f);
			elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);

			playerController.height = crouchingHeight;
			playerController.center = crouchingCenter;
		}
		else if(!IsSliding)
		{
			t = (float)elapsedFrames / interpolationCrouchFrames;
			playerView.transform.localPosition = Vector3.Lerp(playerView.transform.localPosition, standingView, 0.01f);
			elapsedFrames = (elapsedFrames + 1) % (interpolationCrouchFrames + 1f);

			playerController.height = standingHeight;
			playerController.center = standingCenter;
		}


		

		GroundCheck();
		if (IsGrounded)
		{
			//temp = Vector3.Cross(m_GroundNormal, Vector3.down);
			//groundSlopeDir = Vector3.Cross(temp, m_GroundNormal);
			//groundSlopeAngle = Vector3.Angle(m_GroundNormal, Vector3.up);
			//transform.rotation = Quaternion.Euler(new Vector3(-groundSlopeAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));

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

			if (IsSprinting)
			{
				targetForwardVelocity *= SprintMultiplier;
				targetVelocity = targetForwardVelocity + targetLateralVelocity;
				PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
			}
			if (IsCrouching)
			{
				if (IsSliding)
				{
					targetForwardVelocity *= SlidingMultiplier;
					targetLateralVelocity *= SlidingMultiplier;
					targetVelocity = targetForwardVelocity + targetLateralVelocity;
					PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);

				}
				else
				{
					targetForwardVelocity *= CrouchMultiplier;
					targetLateralVelocity *= CrouchMultiplier;
					targetVelocity = targetForwardVelocity + targetLateralVelocity;
					PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);

				}
			}
			else
			{
				PlayerVelocity = new Vector3(Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).x, PlayerVelocity.y, Vector3.Lerp(PlayerVelocity, targetVelocity, Time.deltaTime * Friction).z);
			}

			// start by canceling out the vertical component of our velocity
			PlayerVelocity = new Vector3(PlayerVelocity.x, 0f, PlayerVelocity.z);

			// Slope Check 
			if (m_GroundNormal != Vector3.up && IsGrounded)
			{
				IsOnSlope = true;
				//JumpForce = SlopeJumpForce;
				PlayerVelocity += Vector3.down * SlopeForce * Time.deltaTime;
				//PlayerVelocity = Vector3.ProjectOnPlane(PlayerVelocity, m_GroundNormal);
				Debug.DrawLine(transform.position, transform.position + PlayerVelocity, Color.green);
			}
			else
			{
				IsOnSlope = false;
				//JumpForce = baseJumpForce;
			}
			//Jump        
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
		else
		{

			PlayerVelocity += new Vector3(targetVelocity.x * Time.deltaTime * AirAcceleration, 0, targetVelocity.z * Time.deltaTime * AirAcceleration);
			//Appliquer la gravité
			PlayerVelocity += Vector3.down * GravityForce * Time.deltaTime;

		}
		PlayerVelocity = Vector3.ClampMagnitude(PlayerVelocity, VitesseMax * 2);

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

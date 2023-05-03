using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using Color = UnityEngine.Color;
using System.Linq;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour {
	private static string ANI_RUN_SPEED = "runSpeed";
	private static string ANI_IS_JUMPING = "isJumping";
	private static string ANI_IS_FLAGPOLE = "isFlagpole";

	public GameObject camera;
	public float moveSpeed = 5f;
	public float runSpeed = 8f;
	public float jumpHeight = 8.4f;
	public float maxJumpTime = 0.3f;
	public float cameraSpeed = 5f;
	public float viewportLeft = 0.03f;
	public LayerMask groundLayer;
	public float groundCheckDistance = .01f;
	public float cameraMoveScreenPercent = 30;
	public float counterJumpForce = 5;
	public float maxWalkVelocity = 1;
	public float initialImpulseForce = 1;
	public float maxRunVelocity = 2;
	public float skidDeceleration = 2f;
	public float skidDuration = 0.5f;
	public bool isBig = false;
	public bool isInvincible = false;

	private Rigidbody2D rb;
	private Animator animator;
	private Collider2D collider;
	private float jumpTime;
	private Collision collision;
	private bool isGrounded = false;
	private bool isJumpKeyHeld = false;
	private bool isJumping;
	private bool isRunning = false;
	private Vector3 originalScale;
	private Mario inputActions;
	private float cameraWidth;
	private bool isSkidding;
	private float skidTimer;
	private Vector2 moveDir;
	private bool isTeleported = false;

	public enum Form {
		small, big, fire
	}

	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rb.freezeRotation = true; // freeze rotation
		animator = GetComponent<Animator>();
		collider = GetComponent<Collider2D>();
		collider.sharedMaterial = new PhysicsMaterial2D("NoFriction");
		collider.sharedMaterial.friction = 0f;
		originalScale = transform.localScale;

		inputActions = new Mario();
		inputActions.Enable();
		inputActions.Player.Jump.performed += OnJumpPerformed;
		inputActions.Player.Jump.canceled += OnJumpCanceled;
		inputActions.Player.Run.performed += OnRunPerformed;
		inputActions.Player.Run.canceled += OnRunCanceled;

		moveSpeed *= transform.localScale.x;
		runSpeed *= transform.localScale.x;
		jumpHeight *= transform.localScale.x;
		counterJumpForce *= transform.localScale.x;
		rb.gravityScale *= transform.localScale.x;
		maxWalkVelocity *= transform.localScale.x;
		maxRunVelocity *= transform.localScale.x;
		initialImpulseForce *= transform.localScale.x;
		cameraWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
	}

	private void OnDestroy() {
		inputActions.Player.Jump.performed -= OnJumpPerformed;
		inputActions.Player.Jump.canceled -= OnJumpCanceled;
		inputActions.Player.Run.performed -= OnRunPerformed;
		inputActions.Player.Run.canceled -= OnRunCanceled;
		inputActions.Disable();
	}

	private void Move() {
		moveDir = inputActions.Player.Move.ReadValue<Vector2>();
		if(moveDir == Vector2.zero) {
			animator.SetFloat(ANI_RUN_SPEED, 0);
			return;
		}
		float horizontalInput = moveDir.x;
		if(collision.collider.gameObject.TryGetComponent(out Pipe pipe) &&
			((collision.Right && moveDir.x > 0f) || (collision.Left && moveDir.x < 0f) || (collision.Bottom && moveDir.y < 0f))) {
			transform.position = pipe.destination.transform.position;
			Vector3 p = pipe.cameraDestination.transform.position;
			camera.transform.position = new Vector3(p.x, p.y, camera.transform.position.z);
		}
		//Debug.Log(horizontalInput + ", " + verticalInput + ", " + collision.collision);
		//if(collision != null && collision.collision != null && collision.collision.gameObject.TryGetComponent(out Pipe pipe) && ((collision.Right && horizontalInput > 0f) || (collision.Left && horizontalInput < 0f) || (collision.Bottom && verticalInput < 0f))){
		//	transform.position = pipe.destination.transform.localPosition;
		//}
		// don't cling to walls
		if((collision.Left && horizontalInput < 0f) || (collision.Right && horizontalInput > 0f)) {
			animator.SetFloat(ANI_RUN_SPEED, 0);
			return;
		}
		// Start skidding if changing direction while moving
		if(!isSkidding && Mathf.Abs(horizontalInput) > 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(rb.velocity.x)) {
			isSkidding = true;
			skidTimer = 0f;
		}
		// Move the player horizontally
		float currentMoveSpeed = isRunning ? runSpeed : moveSpeed;
		//Vector2 movement = new Vector2(horizontalInput * currentMoveSpeed, rb.velocity.y);
		//rb.velocity = movement;
		// Apply initial movement as an impulse
		//if(isSkidding) {
		//	// Decelerate while skidding
		//	if(Mathf.Abs(rb.velocity.x) > 0) {
		//		float deceleration = skidDeceleration * rb.mass;
		//		rb.AddForce(new Vector2(-Mathf.Sign(rb.velocity.x) * deceleration, 0f));
		//	}

		//	// End skidding after skid duration
		//	skidTimer += Time.fixedDeltaTime;
		//	if(skidTimer >= skidDuration) {
		//		isSkidding = false;
		//	}
		//} else
		if(isGrounded && Mathf.Abs(rb.velocity.x) < (isRunning ? maxRunVelocity : maxWalkVelocity)) {
			rb.AddForce(new Vector2(horizontalInput * initialImpulseForce, 0f) * rb.mass, ForceMode2D.Impulse);
		} else // won't max velocity has been reached then keep applying velocity in that direction to keep movement smooth
			rb.velocity = new Vector2(horizontalInput * currentMoveSpeed, rb.velocity.y);


		Vector3 characterPosition = transform.position;
		transform.position = characterPosition;

		// Flip the player horizontally if moving left
		if(horizontalInput < 0f) {
			// Check if the player is to the left of the camera's left edge
			//float leftEdge = Camera.main.WorldToViewportPoint(transform.localPosition).x;
			float leftEdge = Camera.main.WorldToScreenPoint(transform.localPosition).x;
			//Debug.Log(leftEdge + ", " + transform.position + ", " + camera.transform.position + ", " + cameraWidth + ", " + (transform.position.x - Camera.main.transform.position.x));
			if(leftEdge < viewportLeft || collision.Left) {
				// if the player moves left to make them off screen to set velocity to zero so they don't move
				rb.velocity = Vector2.zero;
				//Debug.Log("Fixing: " + Camera.main.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z)));
				//transform.position = new Vector3(leftEdge, transform.position.y, transform.position.z);
			}
			transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
		} else if(horizontalInput > 0) {
			transform.localScale = originalScale;
		}

		// Update the animator parameters
		animator.SetFloat(ANI_RUN_SPEED, Mathf.Abs(horizontalInput));
	}

	private void OnJumpPerformed(InputAction.CallbackContext obj) {
		// Check if the player is grounded then they can jump
		//isGrounded = Physics2D.IsTouchingLayers(collider, groundLayer);
		//isGrounded = Physics2D.OverlapBox(collider.bounds.center, new Vector2(collider.bounds.extents.x, collider.bounds.extents.y*2), 0f, groundLayer);
		// cast a ray downwards from the player's position
		//RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

		// check if the ray hits a collider with the specified tag or layer
		//isGrounded = hit.collider != null;
		//isGrounded = collider.IsTouchingLayers(groundLayer);
		//Debug.Log(isGrounded);
		if(isGrounded) {
			isJumping = true;
			isJumpKeyHeld = true;
			isGrounded = false;
			jumpTime = Time.time + maxJumpTime;
			rb.AddForce(Vector2.up * GetJumpForce(jumpHeight) * rb.mass, ForceMode2D.Impulse); 
		}
	}

	private void OnJumpCanceled(InputAction.CallbackContext obj) {
		isJumpKeyHeld = false;
	}

	private void OnRunPerformed(InputAction.CallbackContext context) {
		isRunning = true;
		//animator.SetBool("IsRunning", true);
	}

	private void OnRunCanceled(InputAction.CallbackContext context) {
		isRunning = false;
		//animator.SetBool("IsRunning", false);
	}
	public float GetJumpForce(float jumpHeight) {
		//h = v^2/2g
		//2gh = v^2
		//sqrt(2gh) = v
		return Mathf.Sqrt(2 * Physics2D.gravity.magnitude * jumpHeight);
	}

	void OnCollisionEnter2D(Collision2D collision2D) {
		collision = new Collision(collision2D);
		//Debug.Log("Enter: " + collision);
		//if(collision.Bottom)
		//	SetGrounded(true);
		Debug.Log("Collided with: " + collision2D.gameObject + ", " + isGrounded + ", " + isJumping + ", " + collision);
		// i hit a block from below ask if something hasppens
		if(isJumping && collision.Top != null && collision2D.gameObject.TryGetComponent<IActivateFromBelow>(out IActivateFromBelow activate)) {
			activate.Hit(this, collision2D);
		}
		if(collision2D.gameObject.TryGetComponent(out ITouched touched)) {
			touched.Touch(this);
		}
	}
	//void OnCollisionExit2D(Collision2D collision2D) {
	//	collision = new Collision(collision2D);
	//	Debug.Log("Exit: " + collision);
	//	if(collision.Bottom)
	//		SetGrounded(true);
	//}

	private void Update() {

	}

	void SetGrounded(bool isGrounded) {
		this.isGrounded = isGrounded;
		if(isGrounded) {
			isJumping = false;
		}
	}

	private void FixedUpdate() {
		collision = new Collision(collider, groundLayer);
		//Debug.Log(collision.Bottom.gameObject);
		if(collision.Bottom != null && collision.Bottom.gameObject) {
			//Debug.Log(collision.Bottom.gameObject);
			SetGrounded(true);
		} else
			isJumping = true;
		animator.SetBool(ANI_IS_JUMPING, isJumping);
		Move();
		if(!isJumpKeyHeld && jumpTime > Time.time) {
			// apply additional jump velocity
			//rb.velocity = new Vector2(rb.velocity.x, jumpForce + (maxJumpTime - jumpTime));
			rb.AddForce(new Vector2(0, -counterJumpForce) * rb.mass); // 
		}
		//if(isGrounded)
		//	isJumping = false;
		// check if player is in the air because of a jump then set animaiton
		//Debug.Log(isJumping);
	}

	private void LateUpdate() {
		Vector3 playerViewportPosition = Camera.main.WorldToViewportPoint(transform.localPosition);
		// Only move the camera if the player is moving right and past the halfway point of the view
		if(playerViewportPosition.x > cameraMoveScreenPercent / 100f) {
			// Calculate the position the camera should move to
			// player is x ahead now of where the camera should be so move camera ahead by x
			float targetX = Camera.main.ViewportToWorldPoint(new Vector3(playerViewportPosition.x, playerViewportPosition.y, playerViewportPosition.z)).x;

			// Calculate the distance between the camera and the target position
			float distance = targetX - camera.transform.position.x;
			//Debug.Log(targetX + ", " + distance);
			// Move the camera towards the target position at a speed based on the distance
			if(distance > 0)
				camera.transform.position += Vector3.right * distance * cameraSpeed * Time.deltaTime;
		}
	}

	internal bool IsBig() {
		return isBig;
	}

	internal void ChangeForm(Form form) {
		switch(form) {
			case Form.big:
				// grow big animation
				isBig = true;
				break;
			case Form.small:
				isBig = false;
				break;
			default: break;
		}
	}

	internal void SetInvincible(bool isInvincible) {
		this.isInvincible = isInvincible;
	}
}
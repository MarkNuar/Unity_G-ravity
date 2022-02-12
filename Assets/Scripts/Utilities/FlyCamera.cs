using System;
using UnityEngine;

[RequireComponent( typeof(Camera) )]
public class FlyCamera : MonoBehaviour {
	public float acceleration = 50; // how fast you accelerate
	public float rollSpeed = 0.3f;
	public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
	public float lookSensitivity = 1; // mouse look sensitivity
	public float dampingCoefficient = 10; // how quickly you break to a halt after you stop your input
	public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable

	
	Vector3 velocity; // current velocity

	static bool Focused {
		get => Cursor.lockState == CursorLockMode.Locked;
		set {
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = value == false;
		}
	}

	void OnEnable() {
		if( focusOnEnable ) Focused = true;
	}

	void OnDisable() => Focused = false;

	void Update() {
		// Input
		if( Focused )
			UpdateInput();
		else if(Input.GetMouseButtonDown( 0 ) && !GameManager.Instance.gamePaused )
			Focused = true;

		// Physics
		velocity = Vector3.Lerp( velocity, Vector3.zero, dampingCoefficient * Time.deltaTime );
		transform.position += velocity * Time.deltaTime;
	}

	void UpdateInput() {
		// Position
		velocity += GetAccelerationVector() * Time.deltaTime;

		// Rotation
		Vector2 mouseDelta = lookSensitivity * new Vector2( Input.GetAxis( "Mouse X" ), -Input.GetAxis( "Mouse Y" ) );
		float rollDelta = 0;
		var q = Input.GetKey(KeyCode.Q);
		var e = Input.GetKey(KeyCode.E);
		if (q && e)
			rollDelta = 0;
		else if (q)
			rollDelta = 1;
		else if (e)
			rollDelta = -1;

		Quaternion rotation = transform.rotation;
		Quaternion deltaRotation = Quaternion.Euler(mouseDelta.y, mouseDelta.x, rollDelta * rollSpeed);
		transform.rotation = rotation * deltaRotation;
		
		// Leave cursor lock
		if( Input.GetKeyDown( KeyCode.Escape ) )
			Focused = false;
	}
	
	Vector3 GetAccelerationVector() {
		Vector3 moveInput = default;

		void AddMovement( KeyCode key, Vector3 dir ) {
			if( Input.GetKey( key ) )
				moveInput += dir;
		}

		AddMovement( KeyCode.W, Vector3.forward );
		AddMovement( KeyCode.S, Vector3.back );
		AddMovement( KeyCode.D, Vector3.right );
		AddMovement( KeyCode.A, Vector3.left );
		AddMovement( KeyCode.Space, Vector3.up );
		AddMovement( KeyCode.LeftControl, Vector3.down );
		Vector3 direction = transform.TransformVector( moveInput.normalized );

		if( Input.GetKey( KeyCode.LeftShift ) )
			return direction * ( acceleration * accSprintMultiplier ); // "sprinting"
		return direction * acceleration; // "walking"
	}
}
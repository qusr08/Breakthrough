using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockParticle : MonoBehaviour {
	[SerializeField] private Rigidbody2D rigidBody2D;
	[SerializeField, Range(0f, 180f)] private float coneSizeDegrees;
	[SerializeField] private float minVelocity;
	[SerializeField] private float maxVelocity;
	[SerializeField, Range(0f, 180f)] private float angularVelocity;

	#region Unity Functions
	private void Start ( ) {
		// Generate a random velocity vector within the range of the cone spawn area
		float randomRadians = (Random.Range(-coneSizeDegrees, coneSizeDegrees) * Mathf.Deg2Rad) + (Mathf.PI / 2f);
		Vector2 velocityVector = new Vector2(Mathf.Cos(randomRadians), Mathf.Sin(randomRadians));
		rigidBody2D.velocity = velocityVector * Random.Range(minVelocity, maxVelocity);

		// Generate a random angular velocity speed
		rigidBody2D.angularVelocity = Random.Range(-angularVelocity, angularVelocity);
	}

	private void Update ( ) {
		// If the board particle falls below the bottom of the camera view, then destroy it
		if (transform.position.y < Camera.main.transform.position.y - (Camera.main.orthographicSize * 2f)) {
			Destroy(gameObject);
		}
	}
	#endregion
}

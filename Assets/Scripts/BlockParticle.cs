using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockParticle : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private Rigidbody2D rigidBody2D;
	[Header("Properties")]
	[SerializeField, Min(0f), Tooltip("The maximum velocity that can be applied to this particle when it is created.")] private float maxVelocity;
	[SerializeField, Min(0f), Tooltip("The minimum velocity that can be applied to this particle when it is created.")] private float minVelocity;
	[SerializeField, Min(0f), Tooltip("The maximum angular velocity that can be applied to this particle when it is created.")] private float maxAngularVelocity;
	[SerializeField, Min(0f), Tooltip("The minimum angular velocity that can be applied to this particle when it is created.")] private float minAngularVelocity;

	private void Start ( ) {
		rigidBody2D.velocity = Random.insideUnitCircle.normalized * Random.Range(minVelocity, maxVelocity);
		rigidBody2D.angularVelocity = Random.Range(minAngularVelocity, maxAngularVelocity);
	}

	private void Update ( ) {
		if (transform.position.y < Camera.main.transform.position.y - (Camera.main.orthographicSize * 2f)) {
			Destroy(gameObject);
		}
	}
}

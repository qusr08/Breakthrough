using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpecialText : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private ParticleSystem textParticleSystem;
	[SerializeField] private TextMeshPro textMeshPro;
	[Header("Properties")]
	[SerializeField, Min(0f), Tooltip("The speed of animations in seconds when this special text is appearing and disappearing.")] private float animationTime;
	[SerializeField, Tooltip("The position offset for when the text appears and disappears.")] private float positionOffset;
	[SerializeField, Range(0f, 45f), Tooltip("The range of angles that the text can move to when it appears.")] private float angleRange;

	private Vector3 toPositionVelocity;
	private Vector3 toRotationVelocity;
	private float toAlphaVelocity;

	private void Start ( ) {
		StartCoroutine(Show( ));
	}

	public IEnumerator Show ( ) {
		yield return new WaitForSeconds(2f);

		// Set the initial alpha of the text
		Color color = textMeshPro.color;
		color.a = 0f;
		textMeshPro.color = color;

		// Set the initial position and rotation of the tranform
		Vector3 toPosition = transform.position;
		transform.position += Vector3.up * positionOffset;
		Vector3 fromPosition = transform.position;

		Quaternion toRotation = Quaternion.Euler(0f, 0f, Random.Range(-angleRange, angleRange));
		transform.rotation = Quaternion.identity;
		Quaternion fromRotation = transform.rotation;

		float elapsedTime = 0f;
		while (elapsedTime < animationTime) {
			// Get a more accurate smoothdamp time
			float smoothTime = Mathf.SmoothStep(0f, 1f, elapsedTime / animationTime);
			elapsedTime += Time.deltaTime;

			// Smoothly transition the alpha value
			color.a = Mathf.Lerp(0f, 1f, smoothTime);
			textMeshPro.color = color;

			// Smoothly transition the position and rotation
			transform.position = Vector3.Lerp(fromPosition, toPosition, smoothTime);
			transform.rotation = Quaternion.Slerp(fromRotation, toRotation, smoothTime);

			yield return null;
		}

		// Set the end alpha, position, and rotation
		// This prevents any of these values from being slightly off due to inconsistancies with smoothdamp
		color.a = 1f;
		textMeshPro.color = color;
		transform.position = toPosition;

		// Spawn the particles for the text
		textParticleSystem.Play( );
	}

	public void Hide ( ) {

	}
}

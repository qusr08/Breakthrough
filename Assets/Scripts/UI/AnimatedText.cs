
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimatedText : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private ParticleSystem textParticleSystem;
	[SerializeField] private TextMeshPro textMeshPro;
	[Header("Properties")]
	[SerializeField] private AnimationCurve animationCurve;
	[SerializeField, Min(0f), Tooltip("The speed of animations in seconds when this special text is appearing and disappearing.")] private float animationTime;
	[SerializeField, Tooltip("The position offset for when the text appears and disappears.")] private float positionOffset;
	[SerializeField, Range(0f, 45f), Tooltip("The range of angles that the text can move to when it appears.")] private float angleRange;
	[SerializeField, Tooltip("Whether or not the start this text shown or not.")] private bool isVisible;

	private float fromAlpha;
	private float toAlpha;

	private Vector3 fromPosition;
	private Vector3 toPosition;

	private Quaternion fromRotation;
	private Quaternion toRotation;

	public bool IsVisible {
		get {
			return isVisible;
		}
		set {
			isVisible = value;

			// Set the alpha of the text
			SetTextAlpha(isVisible ? 1f : 0f);
		}
	}

	private void OnValidate ( ) {
		IsVisible = isVisible;
	}

	/*private void Start ( ) {
		StartCoroutine(Loop( ));
	}*/

	/// <summary>
	/// Show the text
	/// </summary>
	/// <param name="startPosition">The starting position of the text</param>
	/// <param name="playParticles">Whether or not to play particles at the end of the tween</param>
	public void ShowText (Vector3 startPosition, bool playParticles) {
		fromAlpha = 0f;
		toAlpha = 1f;

		fromPosition = startPosition + (Vector3.up * positionOffset);
		toPosition = startPosition;

		fromRotation = transform.rotation;
		toRotation = Quaternion.Euler(0f, 0f, Random.Range(-angleRange, angleRange));

		StartCoroutine(Animate(playParticles));
	}

	/// <summary>
	/// Hide the text
	/// </summary>
	public void HideText ( ) {
		fromAlpha = 1f;
		toAlpha = 0f;

		fromPosition = transform.position;
		toPosition = transform.position + (Vector3.down * positionOffset);

		fromRotation = transform.rotation;
		toRotation = Quaternion.identity;

		StartCoroutine(Animate(false));
	}

	/// <summary>
	/// Move the text to a certain position
	/// </summary>
	/// <param name="moveToPosition">The position to move to</param>
	public void MoveText (Vector3 moveToPosition) {
		fromAlpha = 1f;
		toAlpha = 1f;

		fromPosition = transform.position;
		toPosition = moveToPosition;

		fromRotation = transform.rotation;
		toRotation = Quaternion.identity;

		StartCoroutine(Animate(false));
	}

	/// <summary>
	/// Set the text
	/// </summary>
	/// <param name="text">The text to set to</param>
	public void SetText (string text) {
		textMeshPro.text = text;
	}

	/// <summary>
	/// Set the alpha of the text
	/// </summary>
	/// <param name="alpha">The alpha to set to</param>
	private void SetTextAlpha (float alpha) {
		Color color = textMeshPro.color;
		color.a = alpha;
		textMeshPro.color = color;
	}

	/// <summary>
	/// Animate the text based on the set position, rotatio, and color
	/// </summary>
	/// <param name="playParticles">Whether or not to play the particle system at the end of the tween</param>
	/// <returns></returns>
	private IEnumerator Animate (bool playParticles) {
		// Set the initial values of the text
		SetTextAlpha(fromAlpha);
		transform.SetPositionAndRotation(fromPosition, Quaternion.identity);

		// https://stackoverflow.com/questions/69954720/mathf-smoothdamp-takes-longer-than-it-should-inside-a-coroutine
		float elapsedTime = 0f;
		while (elapsedTime < animationTime) {
			float smoothTime = animationCurve.Evaluate(elapsedTime / animationTime);
			elapsedTime += Time.deltaTime;

			// Smoothly transition values
			SetTextAlpha(Mathf.Lerp(fromAlpha, toAlpha, smoothTime));
			transform.position = Vector3.Lerp(fromPosition, toPosition, smoothTime);
			transform.rotation = Quaternion.Slerp(fromRotation, toRotation, smoothTime);

			yield return null;
		}

		// Set the end alpha, position, and rotation
		// This prevents any of these values from being slightly off due to inconsistancies with smoothdamp
		SetTextAlpha(toAlpha);
		transform.SetPositionAndRotation(toPosition, toRotation);

		// Spawn the particles for the text
		if (playParticles && textParticleSystem != null) {
			textParticleSystem.Play( );
		}
	}

	/*private IEnumerator Loop ( ) {
		while (true) {
			yield return new WaitForSeconds(animationTime * 1.5f);
			StartCoroutine(Animate(true));
			yield return new WaitForSeconds(animationTime * 1.5f);
			StartCoroutine(Animate(false));
		}
	}*/
}

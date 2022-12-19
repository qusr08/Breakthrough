using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUIParticle : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private TrailRenderer trailRenderer;
	[Header("Scene GameObjects")]
	[SerializeField] private GameManager gameManager;
	[Header("Properties")]
	[SerializeField] public bool _isInitialized = false;
	[SerializeField] public Vector3 _fromPosition;
	[SerializeField] public Vector3 _toPosition;
	[SerializeField] private float minRotation = -3f;
	[SerializeField] private float maxRotation = 3f;
	[SerializeField] private float minOffset = -3f;
	[SerializeField] private float maxOffset = 3f;
	[SerializeField, Min(0f)] private float lifetime;

	public Vector3 FromPosition {
		get {
			return _fromPosition;
		}

		set {
			transform.position = _fromPosition = value;
		}
	}
	public Vector3 ToPosition {
		get {
			return _toPosition;
		}

		set {
			_toPosition = value;
		}
	}
	public bool IsInitialized {
		get {
			return _isInitialized;
		}

		set {
			_isInitialized = value;

			if (_isInitialized) {
				startTime = Time.time;
			}
		}
	}

	private float startTime;
	private Quaternion fromRotation;
	private Quaternion toRotation;
	private Vector3 toMedianPosition;

	private void OnValidate ( ) {
		trailRenderer = GetComponent<TrailRenderer>( );
		spriteRenderer = GetComponent<SpriteRenderer>( );
		gameManager = FindObjectOfType<GameManager>( );
	}

	private void Start ( ) {
		OnValidate( );

		fromRotation = Quaternion.identity;
		toRotation = Quaternion.Euler(0, 0, 360 * Random.Range(minRotation, maxRotation));
	}

	private void Update ( ) {
		// If the particle has not been initialized, then do not update the position of the particle
		if (!IsInitialized) {
			return;
		}

		float t = (Time.time - startTime) / lifetime;

		transform.rotation = Quaternion.Lerp(fromRotation, toRotation, t);
		transform.position = Vector3.Lerp(FromPosition, ToPosition, t);

		// If this particle has reach the position it is moving towards, destroy the particle
		if (t >= 1) {
			gameManager.BoardPointsUILabel.TriggerTextAnimation( );

			Destroy(gameObject);
		}
	}

	public void SetSprite (Sprite sprite) {
		spriteRenderer.sprite = sprite;
	}
}

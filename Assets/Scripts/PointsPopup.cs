using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*

public class PointsPopup : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private TextMeshPro textMesh;
	[Header("Properties")]
	[SerializeField] public float Lifetime = 1f;
	[Space]
	[SerializeField] [Range(-10f, 10f)] private float moveY = 0f;
	[SerializeField] [Range(-10f, 10f)] private float moveX = 0f;
	[SerializeField] [Min(0f)] private float moveTime = 1f;
	[Space]
	[SerializeField] [Range(0f, 90f)] private float rotateFromAngleRange = 0f;
	[SerializeField] [Range(0f, 90f)] private float rotateToAngleRange = 0f;
	[SerializeField] [Min(0f)] private float rotateTime = 1f;
	[Space]
	[SerializeField] [Range(0f, 10f)] private float scaleFrom = 1f;
	[SerializeField] [Range(0f, 10f)] private float scaleTo = 1f;
	[SerializeField] [Min(0f)] private float scaleTime = 1f;
	[Space]
	[SerializeField] [Range(0f, 1f)] private float fadeFrom = 1f;
	[SerializeField] [Range(0f, 1f)] private float fadeTo = 1f;
	[SerializeField] [Min(0f)] private float fadeTime = 1f;
	[Space]
	[SerializeField] private int _points;
	[SerializeField] public string Title;

	private float startTime = 0;
	
	private float TimeElapsed {
		get {
			return (Time.time - startTime);
		}
	}

	private float MoveProgress {
		get {
			return (TimeElapsed / moveTime);
		}
	}

	public int Points {
		get {
			return _points;
		}

		set {
			_points = value;

			// Set the text of the points popup
			textMesh.SetText($"{Title}\n+{_points}");
			// textMesh.SetText($"+{_points}");

			// Reset the lifetime counter
			// If the lifetime counter was already going, then resetting this also resets the progress
			// This is good for "combos"
			startTime = Time.time;

			// Randomly rotate the points popup when it is given a new value
			transform.rotation = Quaternion.Euler(0, 0, Random.Range(-rotateFromAngleRange, rotateFromAngleRange));
		}
	}

	private void OnValidate ( ) {
		textMesh = GetComponent<TextMeshPro>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Update ( ) {
		if (Time.time - startTime >= Lifetime) {
			Destroy(gameObject);
		}
	}
}

*/
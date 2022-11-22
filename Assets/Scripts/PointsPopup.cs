using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsPopup : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private TextMeshPro textMesh;
	[Header("Properties")]
	[SerializeField] private int _points;
	[SerializeField] private float lifetime = 1f;
	
	public int Points {
		get {
			return _points;
		}

		set {
			_points = value;

			textMesh.SetText($"+{_points}");
		}
	}

	private void OnValidate ( ) {
		textMesh = GetComponent<TextMeshPro>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Start ( ) {
		
	}

	private void Update ( ) {
		lifetime -= Time.deltaTime;
		if (lifetime < 0f) {
			Destroy(gameObject);
		}
	}
}

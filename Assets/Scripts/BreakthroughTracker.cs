using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BreakthroughTracker : MonoBehaviour {
	[Header("Components - Breakthrough Tracker")]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Board board;
	[Space]
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[SerializeField] private List<GameObject> dots;
	[Header("Properties - Breakthrough Tracker")]
	[SerializeField, Range(0f, 1f)] private float dotSpacing;

#if UNITY_EDITOR
	private void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	private void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		gameManager = FindObjectOfType<GameManager>( );
		board = FindObjectOfType<Board>( );

		// Set the position of the breakthrough tracker
		transform.localPosition = new Vector3((board.Width / 2) + board.BorderThickness + board.BoardPadding, -(board.Height / 2) + 0.5f);

		// Set glow size
		glowSpriteRenderer.size = new Vector2(1, dots.Count * (board.BorderThickness + dotSpacing)) + (Vector2.one * (board.GlowThickness * 2));

		for (int i = 0; i < dots.Count; i++) {
			dots[i].GetComponent<SpriteRenderer>( ).size = board.BorderThickness * Vector2.one;
			dots[i].transform.localPosition = new Vector3(0f, i * (board.BorderThickness + dotSpacing));
		}
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardTextManager : MonoBehaviour {
	[Header("Components - Board Text Manager")]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Board board;
	[Space]
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[SerializeField] private BoardText totalPointsBoardText;
	[SerializeField] private BoardText boardPointsBoardText;
	[SerializeField] private BoardText percentageClearBoardText;
	[Header("Properties - Board Text Manager")]
	[SerializeField, Range(0f, 1f)] private float textSpacing;

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
		transform.localPosition = new Vector3(
			(board.Width / 2) + board.BorderThickness + board.BoardPadding + (totalPointsBoardText.Width / 2f),
			(board.Height / 2)
		);

		// TO BE CONTINUED

		// Set glow size
		glowSpriteRenderer.size = new Vector2(totalPointsBoardText.Width, (3 * totalPointsBoardText.Height) + (2 * textSpacing)) + (Vector2.one * (board.GlowThickness * 2));
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}

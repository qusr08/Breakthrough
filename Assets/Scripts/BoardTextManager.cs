using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardTextManager : MonoBehaviour {
	[Header("Components - Board Text Manager")]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Board board;
	[Space]
	[SerializeField] public BoardText TotalPointsBoardText;
	[SerializeField] public BoardText BoardPointsBoardText;
	[SerializeField] public BoardText PercentageClearBoardText;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[SerializeField] private Transform backgroundTransform;
	[SerializeField] private SpriteRenderer backgroundSpriteRenderer;
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

		// Set the position of the background panel
		float width = TotalPointsBoardText.Width + (board.BorderThickness * 2);
		float height = (TotalPointsBoardText.Height * 3) + (textSpacing * 2) + (board.BorderThickness * 2);

		transform.localPosition = new Vector3((board.Width / 2f) + board.BorderThickness + board.BoardPadding, board.Height / 2);
		backgroundTransform.localPosition = new Vector3(width / 2f, -height / 2f);
		backgroundSpriteRenderer.size = new Vector2(width, height);

		// Set the position of the text objects
		TotalPointsBoardText.transform.localPosition = new Vector2(board.BorderThickness, -board.BorderThickness);
		BoardPointsBoardText.transform.localPosition = new Vector2(board.BorderThickness, -board.BorderThickness - TotalPointsBoardText.Height - textSpacing);
		PercentageClearBoardText.transform.localPosition = new Vector2(board.BorderThickness, -board.BorderThickness - (TotalPointsBoardText.Height * 2f) - (textSpacing * 2f));

		// Set glow size
		glowSpriteRenderer.size = new Vector2(width, height) + (Vector2.one * (board.GlowThickness * 2));
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}

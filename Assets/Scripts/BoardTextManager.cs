using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardTextManager : MonoBehaviour {
	[Header("Components - Board Text Manager")]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Board board;
	[Space]
	[SerializeField] public BoardText BreakthroughsBoardText;
	[SerializeField] public BoardText TotalPointsBoardText;
	[SerializeField] public BoardText BoardPointsBoardText;
	[SerializeField] public BoardText PercentageClearBoardText;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[SerializeField] private Transform backgroundTransform;
	[SerializeField] private SpriteRenderer backgroundSpriteRenderer;
	[Space]
	[SerializeField] private BoardText versionBoardText;
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
		float x = (gameManager.GameSettings.BoardWidth / 2f) + board.BorderThickness + board.BoardPadding;
		float y = gameManager.GameSettings.BoardHeight / 2f;
		float width = TotalPointsBoardText.Width + (board.BorderThickness * 2);
		// This height assumes that all of the board text objects are the same height (as they should be)
		float height = (TotalPointsBoardText.Height * 4) + (textSpacing * 3) + (board.BorderThickness * 2);

		transform.localPosition = new Vector3(x, y);
		backgroundTransform.localPosition = new Vector3(width / 2f, -height / 2f);
		backgroundSpriteRenderer.size = new Vector2(width, height);

		// Set the position of the text objects
		TotalPointsBoardText.transform.localPosition = GetTextPositionFromIndex(0);
		BoardPointsBoardText.transform.localPosition = GetTextPositionFromIndex(1);
		PercentageClearBoardText.transform.localPosition = GetTextPositionFromIndex(2);
		BreakthroughsBoardText.transform.localPosition = GetTextPositionFromIndex(3);

		// Set glow size
		glowSpriteRenderer.size = new Vector2(width, height) + (Vector2.one * (board.GlowThickness * 2));

		// Set misc. text positions
		versionBoardText.transform.localPosition = new Vector3(board.BorderThickness, -height - board.BoardPadding);
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	/// <summary>
	/// Get the position for a board text based on its index
	/// </summary>
	/// <param name="index">The index of the board text</param>
	/// <returns>The position of the board text</returns>
	private Vector2 GetTextPositionFromIndex (int index) {
		return new Vector2(board.BorderThickness, -board.BorderThickness + -(TotalPointsBoardText.Height + textSpacing) * index);
	}
}

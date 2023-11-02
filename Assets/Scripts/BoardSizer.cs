using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSizer : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game board sprite renderer.")] private SpriteRenderer boardSpriteRenderer;
	[SerializeField, Tooltip("A reference to the game board's border sprite renderer.")] private SpriteRenderer borderSpriteRenderer;
	[SerializeField, Tooltip("A reference to the main camera.")] private Camera gameCamera;
	[Space]
	[SerializeField, Min(0f), Tooltip("The padding between the game board and the edge of the screen.")] private float boardPadding;
	[SerializeField, Min(0f), Tooltip("The thickness of the border around the game board.")] private float borderThickness;

	#region Properties

	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		// Set the size and position of the board
		float positionX = (GameSettingsManager.BoardWidth / 2) - (GameSettingsManager.BoardWidth % 2 == 0 ? 0.5f : 0f);
		float positionY = (GameSettingsManager.BoardHeight / 2) - (GameSettingsManager.BoardHeight % 2 == 0 ? 0.5f : 0f);
		transform.position = new Vector3(positionX, positionY);
		boardSpriteRenderer.size = new Vector2(GameSettingsManager.BoardWidth, GameSettingsManager.BoardHeight);
		boardSpriteRenderer.color = ThemeSettingsManager.BackgroundColor;

		// Set the size and position of the border
		float borderWidth = GameSettingsManager.BoardWidth + (borderThickness * 2f);
		float borderHeight = GameSettingsManager.BoardHeight + (borderThickness * 2f);
		borderSpriteRenderer.size = new Vector2(borderWidth, borderHeight);
		borderSpriteRenderer.color = ThemeSettingsManager.DetailColor;

		// Set the size and position of the camera
		gameCamera.orthographicSize = (GameSettingsManager.BoardHeight + boardPadding) / 2f;
		gameCamera.transform.position = new Vector3(positionX, positionY, gameCamera.transform.position.z);
		gameCamera.backgroundColor = ThemeSettingsManager.BackgroundColor;
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion
}

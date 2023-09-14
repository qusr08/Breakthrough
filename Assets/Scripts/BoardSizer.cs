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
		float positionX = (GameSettingsManager.Instance.BoardWidth / 2) - (GameSettingsManager.Instance.BoardWidth % 2 == 0 ? 0.5f : 0f);
		float positionY = (GameSettingsManager.Instance.BoardHeight / 2) - (GameSettingsManager.Instance.BoardHeight % 2 == 0 ? 0.5f : 0f);
		transform.position = new Vector3(positionX, positionY);
		boardSpriteRenderer.size = new Vector2(GameSettingsManager.Instance.BoardWidth, GameSettingsManager.Instance.BoardHeight);
		boardSpriteRenderer.color = ThemeSettingsManager.Instance.BackgroundColor;

		// Set the size and position of the border
		float borderWidth = GameSettingsManager.Instance.BoardWidth + (borderThickness * 2f);
		float borderHeight = GameSettingsManager.Instance.BoardHeight + (borderThickness * 2f);
		borderSpriteRenderer.size = new Vector2(borderWidth, borderHeight);
		borderSpriteRenderer.color = ThemeSettingsManager.Instance.DetailColor;

		// Set the size and position of the camera
		gameCamera.orthographicSize = (GameSettingsManager.Instance.BoardHeight + boardPadding) / 2f;
		gameCamera.transform.position = new Vector3(positionX, positionY, gameCamera.transform.position.z);
		gameCamera.backgroundColor = ThemeSettingsManager.Instance.BackgroundColor;
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion
}

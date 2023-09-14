using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAreaSizer : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the line sprite renderer.")] private SpriteRenderer lineSpriteRenderer;
	[SerializeField, Tooltip("A reference to the area sprite renderer.")] private SpriteRenderer areaSpriteRenderer;
	[SerializeField, Tooltip("A reference to the board area that this script will be adjusting the size of.")] private BoardArea boardArea;
	[SerializeField, Range(0f, 1f), Tooltip("The thickness of the line that separates this board area from the rest of the board.")] private float lineThickness;
	[SerializeField, Range(0f, 1f), Tooltip("The opacity value of the board area.")] private float areaOpacity;

	#region Properties

	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		boardArea = GetComponent<BoardArea>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion

	public void Recalculate (float height) {
		// Set the position of the board area
		float x = -0.5f + (GameSettingsManager.Instance.BoardWidth / 2.0f);
		float y = -0.5f + (boardArea.IsFlipped ? GameSettingsManager.Instance.BoardHeight - height : height);
		transform.position = new Vector3(x, y, 0);
		transform.eulerAngles = new Vector3(0f, 0f, boardArea.IsFlipped ? 180f : 0f);

		// Set the position and size of the sprite renderers
		areaSpriteRenderer.transform.position = transform.position + Vector3.up * (height / (boardArea.IsFlipped ? 2.0f : -2.0f));
		areaSpriteRenderer.size = new Vector2(GameSettingsManager.Instance.BoardWidth, height);
		lineSpriteRenderer.size = new Vector2(GameSettingsManager.Instance.BoardWidth, lineThickness);
	}

	public void Recolor (Color color) {
		lineSpriteRenderer.color = color;
		areaSpriteRenderer.color = Utils.ColorWithAlpha(color, areaOpacity);
	}
}

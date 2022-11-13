using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardArea : MonoBehaviour {
	[SerializeField] private Board board;
	[Space]
	[SerializeField] private Transform lineTransform;
	[SerializeField] private Transform tintedAreaTransform;
	[Space]
	[SerializeField] private Color color;
	[SerializeField] private bool areaIsAbove;
	[SerializeField] [Range(0, Constants.BOARD_HEIGHT)] private int height;

	/// <summary>
	/// Whether or not the active Mino is within the area
	/// </summary>
	public bool IsMinoInArea {
		get {
			// If there happens to be no Mino on the board, there will not be a Mino in the area
			if (board.ActiveMino == null) {
				return false;
			}

			// Check the position of the Mino relative to the board area line
			bool isMinoAboveLine = (board.ActiveMino.transform.position.y > transform.position.y);
			if ((areaIsAbove && isMinoAboveLine) || (!areaIsAbove && !isMinoAboveLine)) {
				return true;
			}

			return false;
		}
	}

	private void OnValidate ( ) {
		// Get gameobjects within the scene
		board = FindObjectOfType<Board>( );
		lineTransform = transform.Find("Line");
		tintedAreaTransform = transform.Find("Tinted Area");
		
		// Set the position of the board area line based on the height specified
		transform.position = new Vector3(-0.5f + Constants.BOARD_WIDTH / 2.0f, -0.5f + height, 0);

		// Set the position and size of the line
		lineTransform.position = transform.position;
		lineTransform.localScale = new Vector3(Constants.BOARD_WIDTH, Constants.BOARD_AREA_LINE_HEIGHT, 1);
		lineTransform.GetComponent<SpriteRenderer>( ).color = color;

		// Set the position and size of the tinted area
		tintedAreaTransform.position = transform.position + Vector3.up * (areaIsAbove ? (Constants.BOARD_HEIGHT - height) / 2.0f : height / -2.0f);
		tintedAreaTransform.localScale = new Vector3(Constants.BOARD_WIDTH, (areaIsAbove ? Constants.BOARD_HEIGHT - height : height), 1);
		tintedAreaTransform.GetComponent<SpriteRenderer>( ).color = new Color(color.r, color.g, color.b, Constants.BOARD_AREA_TINT_ALPHA);
	}

	private void Awake ( ) {
		OnValidate( );
	}
}

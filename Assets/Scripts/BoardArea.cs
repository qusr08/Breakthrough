using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardArea : MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private Board board;
	[Header("Components")]
	[SerializeField] private Transform lineTransform;
	[SerializeField] private Transform tintedAreaTransform;
	[Header("Properties")]
	[SerializeField] [Range(0f, 1f)] private float lineThickness = 0.25f;
	[SerializeField] [Range(0f, 1f)] private float areaOpacity = 0.1f;
	[Space]
	[SerializeField] public Color Color;
	[SerializeField] public bool IsAreaAbove;
	[SerializeField] [Min(0)] public int Height;

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
			if ((IsAreaAbove && isMinoAboveLine) || (!IsAreaAbove && !isMinoAboveLine)) {
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

		// Do not do any of the below calculations if the board was not found
		if (board == null) {
			return;
		}

		// Set the position of the board area line based on the height specified
		transform.position = new Vector3(-0.5f + board.Width / 2.0f, -0.5f + Height, 0);

		// Set the position and size of the line
		lineTransform.position = transform.position;
		lineTransform.localScale = new Vector3(board.Width, lineThickness, 1);
		lineTransform.GetComponent<SpriteRenderer>( ).color = Color;

		// Set the position and size of the tinted area
		tintedAreaTransform.position = transform.position + Vector3.up * (IsAreaAbove ? (board.Height - Height) / 2.0f : Height / -2.0f);
		tintedAreaTransform.localScale = new Vector3(board.Width, (IsAreaAbove ? board.Height - Height : Height), 1);
		tintedAreaTransform.GetComponent<SpriteRenderer>( ).color = new Color(Color.r, Color.g, Color.b, areaOpacity);
	}

	private void Awake ( ) {
		OnValidate( );
	}
}

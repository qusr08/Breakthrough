using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Space]
	[SerializeField] [Range(10, 30)] private int boardWidth = 20;
	[SerializeField] [Range(10, 30)] private int boardHeight = 20;

	private GameObject[ , ] board;

#if UNITY_EDITOR
	protected void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	protected void _OnValidate ( ) {
#if UNITY_EDITOR
		// This is used to suppress warnings that Unity oh so kindy throws
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		// Update components
		spriteRenderer = GetComponent<SpriteRenderer>( );

		// Set the board size and position so the bottom left corner is at (0, 0)
		spriteRenderer.size = new Vector2(boardWidth, boardHeight);
		float positionX = (boardWidth / 2) - (boardWidth % 2 == 0 ? 0.5f : 0);
		float positionY = (boardHeight / 2) - (boardHeight % 2 == 0 ? 0.5f : 0);
		transform.position = new Vector3(positionX, positionY);

		// Set the camera orthographic size and position so it fits the entire board
		Camera.main.orthographicSize = boardHeight / 2;
		Camera.main.transform.position = new Vector3(positionX, positionY, Camera.main.transform.position.z);
	}
}

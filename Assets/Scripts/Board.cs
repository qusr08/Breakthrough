using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {
	[SerializeField] private GameObject[ ] pieces;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Space]
	[SerializeField] [Range(10, 40)] public int BoardWidth = 20;
	[SerializeField] [Range(10, 40)] public int BoardHeight = 20;
	[SerializeField] [Range(2, 10)] private int topSpawnGap = 4;

	private Transform[ , ] board;

#if UNITY_EDITOR
	protected void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	protected void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		// Update components
		spriteRenderer = GetComponent<SpriteRenderer>( );

		// Set the board size and position so the bottom left corner is at (0, 0)
		spriteRenderer.size = new Vector2(BoardWidth, BoardHeight);
		float positionX = (BoardWidth / 2) - (BoardWidth % 2 == 0 ? 0.5f : 0f);
		float positionY = (BoardHeight / 2) - (BoardHeight % 2 == 0 ? 0.5f : 0f);
		transform.position = new Vector3(positionX, positionY);

		// Set the camera orthographic size and position so it fits the entire board
		Camera.main.orthographicSize = BoardHeight / 2;
		Camera.main.transform.position = new Vector3(positionX, positionY, Camera.main.transform.position.z);
	}

	private void Awake ( ) {
		board = new Transform[BoardWidth, BoardHeight];
	}

	private void Start ( ) {
		SpawnRandomPiece( );
	}

	public void SpawnRandomPiece ( ) {
		Vector3 spawnPosition = new Vector3((BoardWidth / 2) - 0.5f, BoardHeight - topSpawnGap - 0.5f);

		Instantiate(pieces[Random.Range(0, pieces.Length)], spawnPosition, Quaternion.identity);
	}

	public bool IsInBounds (Vector3 position) {
		bool isInBounds = (position.x >= 0 && position.x < BoardWidth && position.y >= 0 && position.y < BoardHeight);

		return (isInBounds && board[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)] == null);
	}

	public void AddPieceToBoard (Piece piece) {
		foreach (Transform tile in piece.transform) {
			int x = Mathf.RoundToInt(tile.transform.position.x);
			int y = Mathf.RoundToInt(tile.transform.position.y);

			board[x, y] = tile;
		}
	}
}

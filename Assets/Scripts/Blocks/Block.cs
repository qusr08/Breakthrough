using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockDirection {
	UP, RIGHT, DOWN, LEFT
}

public class Block : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game board.")] protected Board board;
	[SerializeField, Tooltip("A reference to this blocks sprite renderer.")] protected SpriteRenderer spriteRenderer;
	[SerializeField, Tooltip("The current position of this block on the game board.")] private Vector2Int _position;
	[SerializeField, Tooltip("The current direction that this block is facing.")] private BlockDirection _blockDirection;
	[SerializeField, Tooltip("The current block group that moves this block.")] private BlockGroup _blockGroup;
	[SerializeField, Tooltip("The current health of this block.")] private int _health;
	[SerializeField, Range(0f, 1f), Tooltip("The new scale of this block object to add a gap between other blocks next to it on the game board.")] private float blockScale;

	#region Properties
	public BlockGroup BlockGroup { get => _blockGroup; set => value.TransferBlock(this); }
	public int Health {
		get => _health;
		set {
			_health = value;
			OnHealthChange( );
		}
	}
	public Vector2Int Position { get => _position; set => _position = value; }
	public BlockDirection BlockDirection { get => _blockDirection; set => _blockDirection = value; }
	#endregion

	#region Unity Functions
	protected virtual void OnValidate ( ) {
		board = FindObjectOfType<Board>( );

		transform.localScale = new Vector3(blockScale, blockScale, 1f);
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}
	#endregion

	protected virtual void OnHealthChange ( ) {
		// If the block has 
		if (Health <= 0) {
			Destroy(gameObject);
		}
	}

	protected void SetColor (Color color) {
		spriteRenderer.color = color;
	}

	public void SetLocation (int x, int y) => SetLocation(new Vector2Int(x, y));
	public void SetLocation (Vector2Int location) {
		// Set the position temporarily to be off the board
		// Needed to properly set the position of this block in the grid array
		Position = -Vector2Int.one;
		board.MoveBlockTo(this, location);

		// Set the transform position of this block its grid position
		if (board.IsPositionValid(Position)) {
			transform.position = (Vector3Int) Position;
		}
	}
}

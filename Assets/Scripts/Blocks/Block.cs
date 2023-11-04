using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public enum BlockDirection {
	RIGHT, DOWN, LEFT, UP
}

public class Block : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game board.")] protected GameManager gameManager;
	[SerializeField, Tooltip("A reference to this blocks sprite renderer.")] protected SpriteRenderer spriteRenderer;
	[SerializeField, Tooltip("The current position of this block on the game board.")] private Vector2Int _boardPosition;
	[SerializeField, Tooltip("The current direction that this block is facing.")] private BlockDirection _blockDirection;
	[SerializeField, Tooltip("The current block group that moves this block.")] private BlockGroup _blockGroup;
	[SerializeField, Tooltip("The current health of this block.")] private int _health;
	[SerializeField, Range(0f, 1f), Tooltip("The new scale of this block object to add a gap between other blocks next to it on the game board.")] private float blockScale;

	#region Properties
	/// <summary>
	///		The current block group this block is a part of
	/// </summary>
	public BlockGroup BlockGroup {
		get => _blockGroup;
		set {
			// Do nothing if the block is already a part of this block group
			if (value.Blocks.Contains(this)) {
				return;
			}

			// Remove the block from the other block group
			if (_blockGroup != null) {
				_blockGroup.Blocks.Remove(this);
			}

			// Set the blocks transform parent to this block group
			transform.SetParent(value.transform, true);
			value.Blocks.Add(this);
			_blockGroup = value;
		}
	}

	/// <summary>
	///		The amount of health the block has before it is destroyed
	/// </summary>
	public int Health {
		get => _health;
		set {
			_health = value;
			OnHealthChange( );
		}
	}

	/// <summary>
	///		The position of the block on the board
	/// </summary>
	public Vector2Int BoardPosition {
		get => _boardPosition;
		set {
			// Only set the new position of this block if the inputted position is valid
			if (gameManager.Board.IsPositionOnBoard(value)) {
				gameManager.Board.Grid[_boardPosition.x, _boardPosition.y] = null;
				_boardPosition = value;
				gameManager.Board.Grid[value.x, value.y] = this;

				return;
			}

			// If the block has been set to a y position that is below the bottom of the board, then destroy this block
			if (value.y < 0) {
				gameManager.Board.Grid[_boardPosition.x, _boardPosition.y] = null;
				Health = 0;
			}
		}
	}

	/// <summary>
	///		The direction that the block is facing
	/// </summary>
	public BlockDirection BlockDirection { get => _blockDirection; set => _blockDirection = value; }
	#endregion

	#region Unity Functions
	protected virtual void OnValidate ( ) {
		gameManager = FindObjectOfType<GameManager>( );

		transform.localScale = new Vector3(blockScale, blockScale, 1f);
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}
	#endregion

	/// <summary>
	///		Update called when the health of this block changes
	/// </summary>
	protected virtual void OnHealthChange ( ) {
		// If the block has been destroyed, update various parts of the game
		if (Health <= 0) {
			// Set this block's block group to be modified because this block was destroyed
			BlockGroup.IsModified = true;

			// Remove the block from the board
			gameManager.Board.Blocks.Remove(this);
			Destroy(gameObject);
		}
	}

	/// <summary>
	///		Set the color of the spriterenderer attached to this block
	/// </summary>
	/// <param name="color">The color to set this block to</param>
	protected void SetColor (Color color) {
		spriteRenderer.color = color;
	}

	/// <summary>
	///		Set the physical location of this block in Unity coordinates
	/// </summary>
	/// <param name="location">The location to set this block to</param>
	public void SetLocation (Vector2Int location) {
		BoardPosition = location;

		// Set the transform position of this block its grid position
		if (gameManager.Board.IsPositionOnBoard(BoardPosition)) {
			transform.position = (Vector3Int) BoardPosition;
		}
	}
}

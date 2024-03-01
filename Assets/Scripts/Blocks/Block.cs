using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block : MonoBehaviour {
	[Header("References")]
	[SerializeField] protected SpriteRenderer spriteRenderer;
	[Header("Properties")]
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private BlockGroup _blockGroup;
	[SerializeField] private int _health;
	[SerializeField] private Color _blockColor;

	private bool hasBoardPositionBeenSet = false;

	/// <summary>
	///		The current position of this block on the board
	/// </summary>
	public Vector2Int BoardPosition {
		get => _boardPosition;
		set {
			// If this block is already at the same position, then return
			if (_boardPosition == value) {
				return;
			}

			// If the old block position is on the board, set that position to null
			if (hasBoardPositionBeenSet && BoardManager.Instance.IsPositionOnBoard(_boardPosition)) {
				BoardManager.Instance.SetBlock(_boardPosition, null);
			}

			_boardPosition = value;

			// If the new block position is on the board, set that position to be a reference to this block
			if (BoardManager.Instance.IsPositionOnBoard(_boardPosition)) {
				BoardManager.Instance.SetBlock(_boardPosition, this);
				hasBoardPositionBeenSet = true;
			}
		}
	}

	/// <summary>
	///		The current block group that this block is a part of
	/// </summary>
	public BlockGroup BlockGroup {
		get => _blockGroup;
		set {
			// If this block is already part of the new block group, then return
			if (_blockGroup == value) {
				return;
			}

			// Remove this block from the old block group if it is not equal to null
			if (_blockGroup != null) {
				_blockGroup.RemoveBlock(this);
			}

			_blockGroup = value;

			// Add this block to the new block group if it is not equal to null
			if (_blockGroup != null) {
				_blockGroup.AddBlock(this);
			}
		}
	}

	/// <summary>
	///		The current health of this block
	/// </summary>
	public int Health {
		get => _health;
		set {
			_health = value;
			OnHealthChange( );
		}
	}

	/// <summary>
	///		The color of this block
	/// </summary>
	public Color BlockColor {
		get => _blockColor;
		set => _blockColor = spriteRenderer.color = value;
	}

	protected virtual void OnHealthChange ( ) {

	}

	protected virtual void OnValidate ( ) {
		spriteRenderer = GetComponent<SpriteRenderer>( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}
}

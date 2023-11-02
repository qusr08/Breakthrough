using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BlockGroup : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game manager.")] protected GameManager gameManager;
	[SerializeField, Tooltip("A list of all the blocks that are a part of this block group.")] private List<Block> _blocks;
	[SerializeField, Tooltip("Set to true if any block that is part of this block group is destroyed.")] private bool _isModified;

	private Vector2 _toPosition;
	private Vector3 _toRotation;

	protected float fallTimer;

	#region Properties
	/// <summary>
	///		A list of all the blocks that are a part of this block group
	/// </summary>
	public List<Block> Blocks { get => _blocks; protected set => _blocks = value; }

	/// <summary>
	///		Whether or not the block group has been modified. When this is true, this block group will be destroyed during the next merge
	/// </summary>
	public bool IsModified { get => _isModified; set => _isModified = value; }

	/// <summary>
	///		Whether or not this block group has no blocks that are a part of it
	/// </summary>
	public bool IsEmpty => Blocks.Count == 0;

	/// <summary>
	///		The position that this block group is going to move to
	/// </summary>
	public Vector2 ToPosition { get => _toPosition; protected set => _toPosition = value; }

	/// <summary>
	///		The rotation that this block group is going to rotate to
	/// </summary>
	public Vector3 ToRotation { get => _toRotation; protected set => _toRotation = value; }
	#endregion

	#region Unity Functions
	protected virtual void OnValidate ( ) {
		gameManager = FindObjectOfType<GameManager>( );
		Blocks = GetComponentsInChildren<Block>( ).ToList( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );

		fallTimer = 0f;
	}

	protected virtual void Update ( ) {
		// Update the smoothing position of this block group

		// Update this block group using specific logic to each type of block group
		UpdateBlockGroup( );
	}
	#endregion

	/// <summary>
	///		Update this block group using specific 
	/// </summary>
	public abstract void UpdateBlockGroup ( );

	/// <summary>
	///		Check to see if this block group can move in the specified direction
	/// </summary>
	/// <param name="direction">The direction to check</param>
	/// <returns>
	///		<strong>true</strong> if this block group can move in the specified direction<br/>
	///		<strong>false</strong> if this block group cannot move in the specified direction
	/// </returns>
	public bool CheckMove (Vector2Int direction) {
		// Check to see if the block group can move by looping through every block in the block group
		foreach (Block block in Blocks) {
			// Get the block that is directly next to the current block
			Vector2Int blockMovePosition = block.BoardPosition + direction;
			Block checkBlock = gameManager.Board.GetBlockAt(blockMovePosition);

			// If there is no block in the direction that the current block will move, then continue to the next block
			if (checkBlock == null) {
				continue;
			}

			// If the check block and the current block are a part of the same block group, then move on to the next block
			if (checkBlock.BlockGroup == this) {
				continue;
			}

			// If the check block and the current block are part of a different block groups, then this block group cannot move
			return false;
		}

		return true;
	}

	/// <summary>
	///		Try to move this block group in the specified direction
	/// </summary>
	/// <param name="direction">The cardinal direction to move this block group</param>
	/// <param name="moveInstantly">Whether or not to move this block group instantly or smoothly</param>
	/// <returns>
	///		<strong>true</strong> if this block group was able to move in the specified direction<br/>
	///		<strong>false</strong> if this block group was not able to move in the specified direction
	/// </returns>
	public bool TryMove (Vector2Int direction, bool moveInstantly = false) {
		// If this block group cannot move in the specified direction, then exit out of this function
		if (!CheckMove(direction)) {
			return false;
		}

		// Alter the position that this block group will move to
		ToPosition += direction;

		// Alter the board positions of all the blocks in this block group
		foreach (Block block in Blocks) {
			block.BoardPosition += direction;
		}

		// If it has been specified to move the block group instantly, then set the transform position
		if (moveInstantly) {
			transform.position += (Vector3Int) direction;
		}

		return true;
	}

	/// <summary>
	///		Check to see if this block group can rotate in the specified direction
	/// </summary>
	/// <param name="angle">The angle in degrees to rotate this block group</param>
	/// <returns>
	///		<strong>true</strong> if this block group can rotate in the specified direction<br/>
	///		<strong>false</strong> if this block group cannot rotate in the specified direction
	/// </returns>
	public bool CheckRotate (float angle) {
		// Check to see if the block group can rotate by looping through every block in the block group
		foreach (Block block in Blocks) {
			// Calculate the position that the current block will be in when it rotates 
			Vector2Int blockRotatePosition = Utils.RotatePositionAroundPivot2D(block.BoardPosition, ToPosition, angle);
			Block checkBlock = gameManager.Board.GetBlockAt(blockRotatePosition);

			// If the check block is null, then continue to the next block in this block group
			if (checkBlock == null) {
				continue;
			}

			// If the check block and the current block have the same block group, then continue to the next block
			if (checkBlock.BlockGroup == this) {
				continue;
			}

			// If the check block and the current block have different block groups, then the block cannot move and therefore the entire block group cannot rotate
			return false;
		}

		return true;
	}

	/// <summary>
	///		Rotate this block group by one block in the specified direction. This function does not check to see if this block group can rotate in the specified direction, that must be checked beforehand
	/// </summary>
	/// <param name="angle">The angle in degrees to rotate this block group</param>
	/// <param name="rotateInstantly">Whether or not to rotate this block group instantly or smoothly</param>
	/// <returns>
	///		<strong>true</strong> if this block group was able to rotate in the specified direction<br/>
	///		<strong>false</strong> if this block group was not able to rotate in the specified direction
	/// </returns>
	public bool TryRotate (float angle, bool rotateInstantly = false) {
		if (!CheckRotate(angle)) {
			return false;
		}

		// Alter the rotation that this block group will rotate to
		Vector3Int rotationVector = Utils.Vect3Round(Quaternion.Euler(0, 0, angle) * Vector3.one);
		ToRotation += rotationVector;

		// Alter the board positions and directions of all the blocks in this block group
		foreach (Block block in Blocks) {
			block.BoardPosition = Utils.RotatePositionAroundPivot2D(block.BoardPosition, ToPosition, angle);
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - gameManager.RotateDirection) % 4);
		}

		// If it has been specified to move the block group instantly, then set the transform position
		if (rotateInstantly) {
			transform.eulerAngles += rotationVector;
		}

		return true;
	}

	/// <summary>
	///		Transfer the specified block from another block group to this block group
	/// </summary>
	/// <param name="block">The block to transfer</param>
	public void TransferBlock (Block block) {
		// Do nothing if the block is already a part of this block group
		if (Blocks.Contains(block)) {
			return;
		}

		// Remove the block from the other block group
		BlockGroup previousBlockGroup = block.BlockGroup;
		if (previousBlockGroup != null) {
			previousBlockGroup.Blocks.Remove(block);
		}

		// Set the blocks transform parent to this block group
		block.transform.SetParent(transform, true);
		Blocks.Add(block);
		block.BlockGroup = this;
	}

	/// <summary>
	///		Merge this block group to another block group
	/// </summary>
	/// <param name="blockGroup">The block group to merge all blocks to</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that is the block group this block group was merged into
	/// </returns>
	public BlockGroup MergeToBlockGroup (BlockGroup blockGroup) {
		// Transfer all of the blocks to the input block group
		for (int i = Blocks.Count - 1; i >= 0; i--) {
			Blocks[i].BlockGroup = blockGroup;
		}

		// Since the block group is now empty, destroy it
		gameManager.Board.BlockGroups.Remove(this);
		Destroy(gameObject);

		return blockGroup;
	}

	/// <summary>
	///		Merge the input block group with this one
	/// </summary>
	/// <param name="blockGroup">The block group to merge with this block group</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that is the two block groups merged together
	/// </returns>
	public BlockGroup MergeBlockGroup (BlockGroup blockGroup) {
		// If the block groups are the same, then just return
		if (blockGroup == this) {
			return blockGroup;
		}

		// If one of the block groups are a player controlled block group, make sure that one is always destroyed
		// This prevents the player from being able to control block groups from minos that have already been placed
		if (this is MinoBlockGroup) {
			return MergeToBlockGroup(blockGroup);
		}
		if (blockGroup is MinoBlockGroup) {
			return blockGroup.MergeToBlockGroup(this);
		}

		// Merge the smaller block group into the larger block group
		// This saves processing time
		if (blockGroup.Blocks.Count >= Blocks.Count) {
			return MergeToBlockGroup(blockGroup);
		} else {
			return blockGroup.MergeToBlockGroup(this);
		}
	}
}

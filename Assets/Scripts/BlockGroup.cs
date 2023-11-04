using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class BlockGroup : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game manager.")] private GameManager gameManager;
	[SerializeField, Tooltip("A list of all the blocks that are a part of this block group.")] private List<Block> _blocks;
	[SerializeField, Tooltip("Set to true if any block that is part of this block group is destroyed.")] private bool _isModified;
	[SerializeField, Tooltip("Whether or not this block group can fall into the breakthrough board area.")] private bool _canFallBelow;

	private Vector2 _toPosition;
	private Vector3 _toRotation;

	#region Properties
	/// <summary>
	///		Whether or not this block group can fall into the breakthrough board area
	/// </summary>
	public bool CanFallBelow { get => _canFallBelow; set => _canFallBelow = value; }

	/// <summary>
	///		A list of all the blocks that are a part of this block group
	/// </summary>
	public List<Block> Blocks { get => _blocks; private set => _blocks = value; }

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
	public Vector2 ToPosition { get => _toPosition; private set => _toPosition = value; }

	/// <summary>
	///		The rotation that this block group is going to rotate to
	/// </summary>
	public Vector3 ToRotation { get => _toRotation; private set => _toRotation = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		gameManager = FindObjectOfType<GameManager>( );
		Blocks = GetComponentsInChildren<Block>( ).ToList( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Update ( ) {
		// Update the smoothing position of this block group
	}
	#endregion

	private bool IsBlockMovementValid (Block block, Vector2Int blockPosition) {
		// If the position that the block is going to move to is off the board in the x direction, do not move to that position
		// Since blocks can fall off the bottom of the board on the y axis
		if (blockPosition.x < 0 || blockPosition.x >= GameSettingsManager.BoardWidth) {
			return false;
		}

		// If the block has been set to a y position that is below the bottom of the board, then destroy this block and remove it from the grid
		if (blockPosition.y < 0) {
			gameManager.Board.Grid[block.BoardPosition.x, block.BoardPosition.y] = null;
			block.Health = 0;
			return true;
		}

		// If this block group cannot fall below the breakthrough board area but the current block is trying to, then this movement is not valid
		if (!CanFallBelow && blockPosition.y < gameManager.Board.BreakthroughBoardArea.Height) {
			return false;
		}

		// Get a reference to the block at the position
		Block checkBlock = gameManager.Board.GetBlockAt(blockPosition);

		// If there is no block in the direction that the current block will move, then continue to the next block
		if (checkBlock == null) {
			return true;
		}

		// If the check block and the current block are a part of the same block group, then move on to the next block
		if (checkBlock.BlockGroup == this) {
			return true;
		}

		// If the check block and the current block are part of a different block groups, then this block group cannot move
		return false;
	}

	/// <summary>
	///		Check to see if this block group can move on the board based on the specified translation
	/// </summary>
	/// <param name="translation">The direction to check</param>
	/// <returns>
	///		<strong>true</strong> if this block group can move in the specified direction<br/>
	///		<strong>false</strong> if this block group cannot move in the specified direction
	/// </returns>
	public bool CheckTranslation (Vector2Int translation) {
		// Check to see if the block group can move by looping through every block in the block group
		foreach (Block block in Blocks) {
			// Get the board position that this block will moved to based on the input movement variables
			Vector2Int newBlockPosition = block.BoardPosition + translation;

			// If this block's movement is not valid for this block group, then the specified translation cannot be completed
			if (!IsBlockMovementValid(block, newBlockPosition)) {
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///		Check to see if this block group can move on the board based on the specified rotation
	/// </summary>
	/// <param name="rotation">The angle in degrees to rotate this block group</param>
	/// <returns>
	///		<strong>true</strong> if this block group can rotate in the specified direction<br/>
	///		<strong>false</strong> if this block group cannot rotate in the specified direction
	/// </returns>
	public bool CheckRotation (float rotation) {
		// Check to see if the block group can move by looping through every block in the block group
		foreach (Block block in Blocks) {
			// Get the board position that this block will moved to based on the input movement variables
			Vector2Int newBlockPosition = Utils.RotatePositionAroundPivot2D(block.BoardPosition, ToPosition, rotation);

			// If this block's movement is not valid for this block group, then the specified translation cannot be completed
			if (!IsBlockMovementValid(block, newBlockPosition)) {
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///		Try to move this block group in the specified direction
	/// </summary>
	/// <param name="translation">The cardinal direction to move this block group</param>
	/// <param name="moveInstantly">Whether or not to move this block group instantly or smoothly</param>
	/// <returns>
	///		<strong>true</strong> if this block group was able to move in the specified direction<br/>
	///		<strong>false</strong> if this block group was not able to move in the specified direction
	/// </returns>
	public bool TryTranslate (Vector2Int translation, bool moveInstantly = true) {
		// If this block group cannot move in the specified direction, then exit out of this function
		if (!CheckTranslation(translation)) {
			return false;
		}

		// Alter the position that this block group will move to
		ToPosition += translation;

		// Alter the board positions of all the blocks in this block group
		foreach (Block block in Blocks) {
			block.BoardPosition += translation;
		}

		// If it has been specified to move the block group instantly, then set the transform position
		if (moveInstantly) {
			transform.position += (Vector3Int) translation;
		}

		return true;
	}

	/// <summary>
	///		Rotate this block group by one block in the specified direction. This function does not check to see if this block group can rotate in the specified direction, that must be checked beforehand
	/// </summary>
	/// <param name="rotation">The angle in degrees to rotate this block group</param>
	/// <param name="rotateInstantly">Whether or not to rotate this block group instantly or smoothly</param>
	/// <returns>
	///		<strong>true</strong> if this block group was able to rotate in the specified direction<br/>
	///		<strong>false</strong> if this block group was not able to rotate in the specified direction
	/// </returns>
	public bool TryRotate (float rotation, bool rotateInstantly = false) {
		if (!CheckRotation(rotation)) {
			return false;
		}

		// Alter the rotation that this block group will rotate to
		Vector3Int rotationVector = Utils.Vect3Round(Quaternion.Euler(0, 0, rotation) * Vector3.one);
		ToRotation += rotationVector;

		// Alter the board positions and directions of all the blocks in this block group
		foreach (Block block in Blocks) {
			block.BoardPosition = Utils.RotatePositionAroundPivot2D(block.BoardPosition, ToPosition, rotation);
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - gameManager.RotateDirection) % 4);
		}

		// If it has been specified to move the block group instantly, then set the transform position
		if (rotateInstantly) {
			transform.eulerAngles += rotationVector;
		}

		return true;
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

		// Merge the smaller block group into the larger block group
		// This saves processing time
		if (blockGroup.Blocks.Count >= Blocks.Count) {
			return MergeToBlockGroup(blockGroup);
		} else {
			return blockGroup.MergeToBlockGroup(this);
		}
	}
}

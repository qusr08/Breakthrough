using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public enum BoardState {
	PLACING_MINO, MERGING_BLOCKGROUPS, UPDATING_BOOMBLOCKS, UPDATING_BLOCKGROUPS
}

public class Board : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameManager gameManager;
	[Space]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer borderSpriteRenderer;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[SerializeField] private Camera gameCamera;
	[Space]
	[SerializeField] private BoardArea breakthroughBoardArea;
	[SerializeField] private BoardArea hazardBoardArea;
	[SerializeField] private List<GameObject> minoPrefabs;
	[Header("Properties")]
	[SerializeField] private BoardState _boardState;
	[SerializeField] private int _width;
	[SerializeField] private int _height;
	[SerializeField] private float cameraPadding;
	[SerializeField] private float borderThickness;
	[SerializeField] private float glowThickness;

	private Block[ , ] blocks;
	private List<List<Block>> minos = new List<List<Block>>( );
	private List<BoomBlockFrames> boomBlockFrames = new List<BoomBlockFrames>( );
	private List<BlockGroup> blockGroups = new List<BlockGroup>( );

	private Vector3 minoSpawnPosition;

	#region Properties
	public int BreakthroughBoardAreaHeight => breakthroughBoardArea.Height;
	public int HazardBoardAreaHeight => hazardBoardArea.Height;

	public int Width => _width;
	public int Height => _height;
	public BoardState BoardState {
		get => _boardState;
		set {
			_boardState = value;

			switch (value) {
				case BoardState.PLACING_MINO:
					GenerateMino( );
					break;
				case BoardState.MERGING_BLOCKGROUPS:
					break;
				case BoardState.UPDATING_BOOMBLOCKS:
					break;
				case BoardState.UPDATING_BLOCKGROUPS:
					break;
			}
		}
	}

	public Block this[int x, int y] {
		get => blocks[x, y];
		set => blocks[x, y] = value;
	}
	#endregion

	#region Unity
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

		gameManager = FindObjectOfType<GameManager>( );

		minoSpawnPosition = new Vector3(Width / 2f, Height - 2f);

		// Set the board size and position so the bottom left corner is at (0, 0)
		// This makes it easier when converting from piece transform position to a board array index
		float positionX = (Width / 2) - (Width % 2 == 0 ? 0.5f : 0f);
		float positionY = (Height / 2) - (Height % 2 == 0 ? 0.5f : 0f);
		spriteRenderer.size = new Vector2(Width, Height);
		transform.position = new Vector3(positionX, positionY);

		// Set the size of the border
		borderSpriteRenderer.size = new Vector2(Width + (borderThickness * 2), Height + (borderThickness * 2));
		glowSpriteRenderer.size = new Vector2(Width + (glowThickness * 2), Height + (glowThickness * 2));

		// Set the camera orthographic size and position so it fits the entire board
		gameCamera.orthographicSize = (Height + cameraPadding) / 2f;
		gameCamera.transform.position = new Vector3(positionX, positionY, gameCamera.transform.position.z);
	}

	protected void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	private void Start ( ) {
		blocks = new Block[Width, Height];

		BoardState = BoardState.PLACING_MINO;
	}

	private void Update ( ) {
		switch (BoardState) {
			case BoardState.PLACING_MINO:
				break;
			case BoardState.MERGING_BLOCKGROUPS:
				break;
			case BoardState.UPDATING_BOOMBLOCKS:
				break;
			case BoardState.UPDATING_BLOCKGROUPS:
				break;
		}
	}
	#endregion

	/// <summary>
	/// Whether or not the input position is within the bounds of the board
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>true if position is within the bounds of the board, false otherwise</returns>
	public bool IsPositionOnBoard (Vector2Int position) {
		bool inX = (position.x >= 0 && position.x < Width);
		bool inY = (position.y >= 0 && position.y < Height);
		return (inX && inY);
	}

	/// <summary>
	/// Check to see if there is a block at the input position
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <param name="blockGroupID">The block group ID to check</param>
	/// <returns>true if there is a block at the input position, false otherwise. Also returns true if the position is out of bounds of the board. Also returns false if the block has the same specified block group ID</returns>
	public bool IsBlockAt (Vector2Int position, int blockGroupID = -1) {
		// If the position is out of bounds, then return true because we want to trick the program into thinking there is a block there
		// The boundaries of the board are solid
		if (!IsPositionOnBoard(position)) {
			return true;
		}

		// Get a reference to the block at the input position
		Block block = blocks[position.x, position.y];

		// If the reference to the block is null, then there is not a block at the input position
		if (block == null) {
			return false;
		}

		// If a block group ID has been specified and the block at the input position has the same block group ID, then the block at the position should be ignored and treated as no block being there
		// This makes things easier with determining valid moves for block groups
		if (blockGroupID != -1 && block.BlockGroupID == blockGroupID) {
			return false;
		}

		// If all above checks pass, then return that there is a valid block at the input position
		return true;
	}

	/// <summary>
	/// Damage the specified block
	/// </summary>
	/// <param name="block">The block to damage</param>
	/// <param name="destroy">Whether or not to ignore the health of the block and completely destroy it</param>
	/// <returns>true if the block was completely destroyed, false otherwise</returns>
	public bool DamageBlock (Block block, bool destroy = false) {
		return DamageBlockAt(block.Position, destroy);
	}

	/// <summary>
	/// Damage the block at the specified position
	/// </summary>
	/// <param name="position">The position of the block to damage</param>
	/// <param name="destroy">Whether or not to ignore the health of the block and completely destroy it</param>
	/// <returns>true if the block at the specified position was completely destroyed, false otherwise</returns>
	public bool DamageBlockAt (Vector2Int position, bool destroy = false) {
		// If there is not a block at the specified position, then no block can be destroyed
		if (!IsBlockAt(position)) {
			return false;
		}

		// Get a reference to the block and damage it
		Block block = blocks[position.x, position.y];
		block.Health -= (destroy ? block.Health : 1);

		// If the block now has 0 health, as in the block has been completely destroyed
		if (block.Health == 0) {
			// If the block has a block group, make sure the block group knows that it was modified
			if (block.BlockGroupID != -1) {
				block.BlockGroup.IsModified = true;
			}

			// If the block has a mino index, remove the reference to the block in the mino list
			if (block.MinoIndex != -1) {
				minos[block.MinoIndex].Remove(block);

				// If the mino index now has a size of 0, the entire mino has been destroyed
				if (minos[block.MinoIndex].Count == 0) {
					gameManager.AddBoardPoints(PointsType.DESTROYED_MINO);
				}
			}

			Destroy(block);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Create boom block frames from a boom block
	/// </summary>
	/// <param name="block">The block to create boom block frames from</param>
	private void GenerateBoomBlockFrames (Block block) {
		if (!block.IsBoomBlock) {
			return;
		}

		boomBlockFrames.Add(new BoomBlockFrames(this, gameManager, block));
	}

	/// <summary>
	/// Generate a random mino on the board
	/// </summary>
	private void GenerateMino ( ) {
		gameManager.ActiveMino = Instantiate(minoPrefabs[Random.Range(0, minoPrefabs.Count)], minoSpawnPosition, Quaternion.identity).GetComponent<PlayerControlledBlockGroup>( );
	}
}

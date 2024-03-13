using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum BoardState {
	GENERATE_WALL, MERGE_BLOCK_GROUPS, UPDATE_BLOCK_GROUPS, UPDATE_MINO, STOP_GAME, UPDATE_BREAKTHROUGH
}

public class BoardManager : Singleton<BoardManager>, IThemeElement {
	[Header("Prefabs")]
	[SerializeField] private GameObject minoBlockPrefab;
	[SerializeField] private GameObject wallBlockPrefab;
	[SerializeField] private GameObject boomBlockPrefab;
	[SerializeField] private GameObject blockGroupPrefab;
	[Header("References")]
	[SerializeField] private SpriteRenderer backgroundSpriteRenderer;
	[SerializeField] private SpriteRenderer borderSpriteRenderer;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[SerializeField] private Camera gameCamera;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float cameraScaleReference;
	[SerializeField] private float cameraPadding;
	[SerializeField] private float borderThickness;
	[SerializeField] private float glowThickness;
	[Space]
	[SerializeField] private int _width;
	[SerializeField] private int _height;
	[SerializeField] private BoardState _boardState;

	private Block[ , ] blocks;
	private List<BlockGroup> blockGroups;
	private WeightedList<MinoType> weightedMinoList;
	private Vector2 minoSpawnPosition;

	/// <summary>
	///		The current state of the board
	/// </summary>
	public BoardState BoardState {
		get => _boardState;
		set {
			_boardState = value;

			switch (_boardState) {
				case BoardState.GENERATE_WALL:
					break;
				case BoardState.MERGE_BLOCK_GROUPS:
					break;
				case BoardState.UPDATE_BLOCK_GROUPS:
					break;
				case BoardState.UPDATE_MINO:
					break;
				case BoardState.STOP_GAME:
					break;
				case BoardState.UPDATE_BREAKTHROUGH:
					break;
			}
		}
	}

	/// <summary>
	///		The width of the board
	/// </summary>
	public int Width {
		get => _width;
		set => _width = value;
	}

	/// <summary>
	///		The height of the board
	/// </summary>
	public int Height {
		get => _height;
		set => _height = value;
	}


#if UNITY_EDITOR
	private void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	private void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		// orthosize = orthosizeref + campad
		// campad = campadref * orthoscale
		// orthoscale = orthosize / orthoscaleref

		// constants: orthosizeref, campadref, orthoscaleref
		// find: orthosize, orthoscale

		// variable substitutions (for readability):
		// > x = orthosize
		// > xr = orthosizeref
		// > y = campad
		// > yr = campadref
		// > z = orthoscale
		// > zr = orthoscaleref

		// finding orthosize:
		// x = xr + y
		// x = xr + (yr * z)
		// x = xr + (yr * (x / zr))
		// x = xr + ((yr * x) / zr)
		// x - ((yr * x) / zr) = xr
		// ((zr * x) / zr) - ((yr * x) / zr) = xr
		// ((zr * x) - (yr * x)) / zr = xr
		// (zr * x) - (yr * x) = xr * zr
		// x * (zr - yr) = xr * zr
		// x = (xr * zr) / (zr - yr)
		// orthosize = (orthosizeref * orthoscaleref) / (orthoscaleref - campadref)
		float cameraSize = Mathf.Max(Width / 2f / gameCamera.aspect, Height / 2f);
		gameCamera.orthographicSize = (cameraSize * cameraScaleReference) / (cameraScaleReference - cameraPadding);

		// finding orthoscale:
		// z = x / zr
		// z = (xr + y) / zr
		// z = (xr + (yr * z)) / zr
		// z = (xr / zr) + ((yr * z) / zr)
		// z - ((yr * z) / zr) = xr / zr
		// ((zr * z) / zr) - ((yr * z) / zr) = xr / zr
		// ((zr * z) - (yr * z)) / zr = xr / zr
		// (zr * z) - (yr * z) = xr
		// z * (zr - yr) = xr
		// z = xr / (zr - yr)
		// orthoscale = orthosizeref / (orthoscaleref - campadref)
		float gameCameraScale = cameraSize / (cameraScaleReference - cameraPadding);

		// Resize the background sprite renderer
		backgroundSpriteRenderer.size = new Vector2(Width, Height);

		// Set the size of scalable sprite renderers
		float scaledBorderThickness = borderThickness * gameCameraScale;
		borderSpriteRenderer.size = new Vector2(scaledBorderThickness * 2 + Width, scaledBorderThickness * 2 + Height);
		float scaledGlowThickness = glowThickness * gameCameraScale;
		glowSpriteRenderer.size = new Vector2(scaledGlowThickness * 2 + Width, scaledGlowThickness * 2 + Height);

		// Set the position of board elements based on the width and height of the board
		Vector2 boardCenter = new Vector3((Width / 2f) - 0.5f, (Height / 2f) - 0.5f);
		backgroundSpriteRenderer.transform.localPosition = boardCenter;
		borderSpriteRenderer.transform.localPosition = boardCenter;
		glowSpriteRenderer.transform.localPosition = boardCenter;
		gameCamera.transform.localPosition = new Vector3(boardCenter.x, boardCenter.y, gameCamera.transform.position.z);
	}

	protected override void Awake ( ) {
		base.Awake( );
		OnValidate( );

		// Calculate the spawn position of minos on the board
		float offsetX = Width % 2 == 0 ? 0.5f : 0f;
		minoSpawnPosition = new Vector2((Width / 2f) - offsetX, Height - 2.5f);

		blocks = new Block[Width, Height];
		blockGroups = new List<BlockGroup>( );
		// weightedMinoList = new WeightedList<MinoType>( );
	}

	public void Start ( ) {
		CreateMinoBlock(new Vector2Int(0, 9), MinoType.O); /// TEST
		CreateWallBlock(new Vector2Int(1, 9), 2); /// TEST
		CreateBoomBlock(new Vector2Int(2, 9), MinoType.D, BoomBlockType.PYRA); /// TEST
	}

	/// <summary>
	///		Check to see if a position is within the bounds of the board or not
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>
	///		<strong>true</strong> if the position is on the board, <strong>false</strong> otherwise
	/// </returns>
	public bool IsPositionOnBoard (Vector2Int position) {
		// If the position is out of the bounds of the board in the x direction, then return false
		if (position.x < 0 || position.x >= Width) {
			return false;
		}

		// If the position is out of the bounds of the board in the y direction, then return false
		if (position.y < 0 || position.y >= Height) {
			return false;
		}

		return true;
	}

	/// <summary>
	///		Get a block at a position on the board
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>
	///		A reference to the <strong>Block Object</strong> if there is a block at the specified position, <strong>null</strong> otherwise
	/// </returns>
	public Block GetBlock (Vector2Int position) {
		// If the position is not within the bounds of the board, then return null
		if (!IsPositionOnBoard(position)) {
			return null;
		}

		// Get and return the block at the specified position
		return blocks[position.x, position.y];
	}

	/// <summary>
	///		Set a block at a position on the board
	/// </summary>
	/// <param name="position">The position of the block to set</param>
	/// <param name="block">A reference to the block to set</param>
	/// <returns><strong>true</strong> if the block was successfully set at the position, <strong>false</strong> otherwise</returns>
	public bool SetBlock (Vector2Int position, Block block) {
		// If the position is not within the bounds of the board, then return false
		if (!IsPositionOnBoard(position)) {
			return false;
		}

		// Place the inputted block at the specified position
		blocks[position.x, position.y] = block;

		return true;
	}

	/// <summary>
	///		Create a mino block on the board
	/// </summary>
	/// <param name="position">The position to create the block at</param>
	/// <param name="minoType">The type of mino that this block will be created as</param>
	/// <param name="blockGroup">The block group to create the block in. If set to <strong>null</strong>, a new block group will be generated</param>
	/// <returns>
	///		A reference to the <strong>Mino Block Object</strong> if the block was successfully created, <strong>null</strong> otherwise
	/// </returns>
	public MinoBlock CreateMinoBlock (Vector2Int position, MinoType minoType, BlockGroup blockGroup = null) {
		// If there is a block at the position already, return null
		if (GetBlock(position) != null) {
			return null;
		}

		// Create the block object in the game scene
		MinoBlock minoBlock = Instantiate(minoBlockPrefab, (Vector2) position, Quaternion.identity).GetComponent<MinoBlock>( );
		minoBlock.BoardPosition = position;
		minoBlock.MinoType = minoType;
		minoBlock.Health = 1;

		// Set the block's block group
		// If the inputted block group was null, create a new block group
		if (blockGroup == null) {
			blockGroup = CreateBlockGroup(Vector2Int.zero);
		}
		minoBlock.BlockGroup = blockGroup;

		return minoBlock;
	}

	/// <summary>
	///		Create a wall block on the board
	/// </summary>
	/// <param name="position">The position to create the block at</param>
	/// <param name="health">The starting health of the wall block</param>
	/// <param name="blockGroup">The block group to create the block in. If set to <strong>null</strong>, a new block group will be generated</param>
	/// <returns>
	///		A reference to the <strong>Wall Block Object</strong> if the block was successfully created, <strong>null</strong> otherwise
	/// </returns>
	public WallBlock CreateWallBlock (Vector2Int position, int health, BlockGroup blockGroup = null) {
		// If there is a block at the position already, return null
		if (GetBlock(position) != null) {
			return null;
		}

		// Create the block object in the game scene
		WallBlock wallBlock = Instantiate(wallBlockPrefab, (Vector2) position, Quaternion.identity).GetComponent<WallBlock>( );
		wallBlock.BoardPosition = position;
		wallBlock.Health = health;

		// Set the block's block group
		// If the inputted block group was null, create a new block group
		if (blockGroup == null) {
			blockGroup = CreateBlockGroup(Vector2Int.zero);
		}
		wallBlock.BlockGroup = blockGroup;

		return wallBlock;
	}

	/// <summary>
	///		Create a wall block on the board
	/// </summary>
	/// <param name="position">The position to create the block at</param>
	/// <param name="minoType">The type of mino that this block will be created as</param>
	/// <param name="boomBlockType">The type of boom block pattern that this block will explode in</param>
	/// <param name="blockGroup">The block group to create the block in. If set to <strong>null</strong>, a new block group will be generated</param>
	/// <returns>
	///		A reference to the <strong>Boom Block Object</strong> if the block was successfully created, <strong>null</strong> otherwise
	/// </returns>
	public BoomBlock CreateBoomBlock (Vector2Int position, MinoType minoType, BoomBlockType boomBlockType, BlockGroup blockGroup = null) {
		// If there is a block at the position already, return null
		if (GetBlock(position) != null) {
			return null;
		}

		// Create the block object in the game scene
		BoomBlock boomBlock = Instantiate(boomBlockPrefab, (Vector2) position, Quaternion.identity).GetComponent<BoomBlock>( );
		boomBlock.BoardPosition = position;
		boomBlock.MinoType = minoType;
		boomBlock.BoomBlockType = boomBlockType;
		boomBlock.Health = 1;

		// Set the block's block group
		// If the inputted block group was null, create a new block group
		if (blockGroup == null) {
			blockGroup = CreateBlockGroup(Vector2Int.zero);
		}
		boomBlock.BlockGroup = blockGroup;

		return boomBlock;
	}

	/// <summary>
	///		Create a new block group on the board
	/// </summary>
	/// <param name="position">The position to create the block group at</param>
	/// <returns>
	///		A reference to the <strong>Block Group Object</strong> if the block group was successfully created, <strong>null</strong> otherwise
	/// </returns>
	public BlockGroup CreateBlockGroup (Vector2 position = default) {
		// Create the block group in the game scene
		BlockGroup blockGroup = Instantiate(blockGroupPrefab, position, Quaternion.identity, transform).GetComponent<BlockGroup>( );
		blockGroups.Add(blockGroup);

		return blockGroup;
	}

	public BlockGroup CreateMino ( ) {
		// Create a block group for the mino
		BlockGroup minoBlockGroup = CreateBlockGroup(position: minoSpawnPosition);

		return minoBlockGroup;
	}

	public void UpdateThemeElements ( ) {
		backgroundSpriteRenderer.color = ThemeSettingsManager.Instance.ActiveThemeSettings.BackgroundColor;
		borderSpriteRenderer.color = ThemeSettingsManager.Instance.ActiveThemeSettings.DetailColor;
		glowSpriteRenderer.color = ThemeSettingsManager.Instance.ActiveThemeSettings.GlowColor;
		gameCamera.backgroundColor = ThemeSettingsManager.Instance.ActiveThemeSettings.BackgroundColor;
	}
}

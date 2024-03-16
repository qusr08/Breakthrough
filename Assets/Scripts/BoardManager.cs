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
	[Space]
	[SerializeField] private Sprite backgroundSprite;
	[SerializeField] private Sprite borderSprite;
	[SerializeField] private Sprite glowSprite;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float cameraScaleReference;
	[SerializeField, Min(0f)] private float cameraPadding;
	[SerializeField, Min(0f)] private float borderThickness;
	[SerializeField, Min(0f)] private float glowThickness;
	[Space]
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

#if UNITY_EDITOR
	public void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	public void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		// Calculate the camera's orthrographic size based on the board width and height
		float minCameraWidth = GameSettingsManager.Instance.ActiveGameSettings.BoardWidth / ((cameraScaleReference * gameCamera.aspect) - (borderThickness + cameraPadding));
		float minCameraHeight = GameSettingsManager.Instance.ActiveGameSettings.BoardHeight / (cameraScaleReference - (borderThickness + cameraPadding));
		gameCamera.orthographicSize = cameraScaleReference * Mathf.Max(minCameraWidth, minCameraHeight) / 2f;

		// Get the camera's scale based on the calculated orthographic scale and a reference value
		// This will be used to make sure all UI looks the same even when the camera gets bigger
		float gameCameraScale = gameCamera.orthographicSize / cameraScaleReference;
		Vector2 boardSize = new Vector2(GameSettingsManager.Instance.ActiveGameSettings.BoardWidth, GameSettingsManager.Instance.ActiveGameSettings.BoardHeight);

		// Set the size of scalable sprite renderers
		float scaledBorderThickness = borderThickness * gameCameraScale;
		float scaledGlowThickness = glowThickness * gameCameraScale;
		backgroundSpriteRenderer.size = boardSize;
		borderSpriteRenderer.size = boardSize + (2 * scaledBorderThickness * Vector2.one);
		glowSpriteRenderer.size = boardSize + (2 * scaledGlowThickness * Vector2.one);

		// Set the sprite's pixels per unit value
		float scaledBackgroundPixelsPerUnit = backgroundSprite.pixelsPerUnit / gameCameraScale;
		float scaledBorderPixelsPerUnit = borderSprite.pixelsPerUnit / gameCameraScale;
		float scaledGlowPixelsPerUnit = glowSprite.pixelsPerUnit / gameCameraScale;
		backgroundSpriteRenderer.sprite = Sprite.Create(backgroundSprite.texture, backgroundSprite.rect, backgroundSprite.pivot / backgroundSprite.rect.size, scaledBackgroundPixelsPerUnit, 1, SpriteMeshType.FullRect, backgroundSprite.border, true);
		borderSpriteRenderer.sprite = Sprite.Create(borderSprite.texture, borderSprite.rect, borderSprite.pivot / borderSprite.rect.size, scaledBorderPixelsPerUnit, 1, SpriteMeshType.FullRect, borderSprite.border, true);
		glowSpriteRenderer.sprite = Sprite.Create(glowSprite.texture, glowSprite.rect, glowSprite.pivot / glowSprite.rect.size, scaledGlowPixelsPerUnit, 1, SpriteMeshType.FullRect, glowSprite.border, true);

		// Set the position of board elements based on the width and height of the board
		Vector2 boardCenter = (boardSize / 2f) - (0.5f * Vector2.one);
		backgroundSpriteRenderer.transform.localPosition = boardCenter;
		borderSpriteRenderer.transform.localPosition = boardCenter;
		glowSpriteRenderer.transform.localPosition = boardCenter;
		gameCamera.transform.localPosition = new Vector3(boardCenter.x, boardCenter.y, gameCamera.transform.position.z);
	}

	protected override void Awake ( ) {
		base.Awake( );
		OnValidate( );

		// Calculate the spawn position of minos on the board
		float offsetX = GameSettingsManager.Instance.ActiveGameSettings.BoardWidth % 2 == 0 ? 0.5f : 0f;
		minoSpawnPosition = new Vector2((GameSettingsManager.Instance.ActiveGameSettings.BoardWidth / 2f) - offsetX, GameSettingsManager.Instance.ActiveGameSettings.BoardHeight - 2.5f);

		blocks = new Block[GameSettingsManager.Instance.ActiveGameSettings.BoardWidth, GameSettingsManager.Instance.ActiveGameSettings.BoardHeight];
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
		if (position.x < 0 || position.x >= GameSettingsManager.Instance.ActiveGameSettings.BoardWidth) {
			return false;
		}

		// If the position is out of the bounds of the board in the y direction, then return false
		if (position.y < 0 || position.y >= GameSettingsManager.Instance.ActiveGameSettings.BoardWidth) {
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

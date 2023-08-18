using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
	#region Blocks
	public const float BLOCK_SCALE = 0.95f; // Blocks are scaled down this amount on the board
	
	public const int BOOM_BLOCK_GUAR = 10; // The amount of Minos that can be generated before the chance to spawn a Boom Block is 100%
	#endregion

	#region Points
	public const int POINT_DSTRY_BLOCK = 6;
	public const int POINT_DROP_BLOCK = 12;
	public const int POINT_BRKTH = 600;
	public const int POINT_DSTRY_MINO = 60;
	public const int POINT_FAST_DROP = 2;
	#endregion

	#region Game Settings
	public const float DIFF_VALUE = 0.07f; // The difficulty scaling value. The larger this is, the faster the game will scale in difficulty

	public const float WEIGHT_PERC = 1f / 3f; // The percentage taken of a chosen weighted index. The higher this number is, the more likely repeat indices will be chosen
	#endregion

	#region UI
	public const float UI_FADE_TIME = 0.15f; // The fade time for all UI element transitions
	public const int UI_GRID_SIZE = 120; // The grid size for all UI elements
	public const float UI_MENU_TRANS_TIME = 0.3f; // How long it takes for the menus to change levels
	public static float UI_RANGE_SIZE = 40f; // How big the area of influence the mouse has on grid components

	public const float BACK_BLOCK_ALPHA = 0.5f; // The alpha value of the background blocks during gameplay

	public static Vector2Int SCRN_RES = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height); // The resolution of the screen
	public const float CAM_DEFLT_VALUE = 15f;
	#endregion

	public const float CLOSE_ENOUGH = 0.01f;

}

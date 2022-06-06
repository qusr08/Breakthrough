using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
	public const int BOARD_WIDTH = 16;
	public const int BOARD_HEIGHT = 28;
	public const int BOARD_TOP_PADDING = 4;
	public const int BOARD_BOTTOM_PADDING = 2;
	public const int BOARD_CAMERA_PADDING = 3;
	public const int BOARD_WALL_HEIGHT = 7; // Varies with level
	public const float BOARD_WALL_GEN_SMOOTHNESS = 0.4f;

	public const float MINO_PERCENT_BOOM = 0.6f; // Might vary with level
	public const float MINO_FALL_TIME = 1.0f; // Varies with level
	public const float MINO_FALL_TIME_ACCELERATED = MINO_FALL_TIME / 20f;
	public const float MINO_MOVE_TIME = 0.15f;
	public const float MINO_MOVE_TIME_ACCELERATED = MINO_MOVE_TIME / 2f;
	public const float MINO_ROTATE_TIME = 0.25f;
	public const float MINO_PLACE_TIME = 0.75f;
	public const float MINO_ROTATE_DIRECTION = -1; // Clockwise
	public const float MINO_TILE_SCALE = 0.95f;
	public const float MINO_DAMP_SPEED = 0.04f;

	public const int BOOM_SURROUND_SIZE = 2;
	public const int BOOM_DIRECTION_SIZE = 4;
	public const float BOOM_ANIMATION_SPEED = 0.05f;

	public const float CLOSE_ENOUGH = 0.01f;
}

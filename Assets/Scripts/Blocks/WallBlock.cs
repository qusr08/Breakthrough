using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallBlockColor {
	WALL_0, WALL_1, WALL_2, WALL_3
}

public class WallBlock : Block {
	[SerializeField] private WallBlockColor _wallBlockColor;

	#region Properties
	/// <summary>
	///		The color of the wall block
	/// </summary>
	public WallBlockColor WallBlockColor {
		get => _wallBlockColor;
		set {
			_wallBlockColor = value;
			SetColor(ThemeSettingsManager.Instance.WallBlockColors[_wallBlockColor]);
		}
	}
	#endregion

	protected override void OnHealthChange ( ) {
		base.OnHealthChange( );

		if (Health > 0) {
			WallBlockColor = (WallBlockColor) Health;
		}
	}
}

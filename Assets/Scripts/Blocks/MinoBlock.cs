using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MinoType {
	X, C, D, F, O, Z_L, Z_R, S_L, S_R
}

public class MinoBlock : Block, IThemeElement {
	[Separator("Mino Block Subclass")]
	[Header("Properties")]
	[SerializeField] private MinoType _minoType;

	/// <summary>
	///		The type of mino that this block is
	/// </summary>
	public MinoType MinoType {
		get => _minoType;
		set {
			_minoType = value;
			BlockColor = ThemeSettingsManager.Instance.ActiveThemeSettings.MinoBlockColors[_minoType];
		}
	}

	public virtual void UpdateThemeElements ( ) {
		BlockColor = ThemeSettingsManager.Instance.ActiveThemeSettings.MinoBlockColors[MinoType];
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MinoType {
	X, C, D, F, O, Z_L, Z_R, S_L, S_R
}

public class MinoBlock : Block {
	[SerializeField] protected MinoType _minoType;

	#region Properties
	public MinoType MinoType {
		get => _minoType;
		set {
			_minoType = value;
			SetColor(ThemeSettingsManager.Instance.MinoBlockColors[_minoType]);
		}
	}
	#endregion
}

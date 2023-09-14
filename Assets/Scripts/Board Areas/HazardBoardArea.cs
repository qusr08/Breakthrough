using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardBoardArea : BoardArea {
	#region Unity Functions
	protected override void OnValidate ( ) {
		base.OnValidate( );

		boardAreaSizer.Recolor(ThemeSettingsManager.Instance.HazardColor);
	}
	#endregion

	public override void OnBlockGroupMerge ( ) {
		throw new System.NotImplementedException( );
	}

	public override void OnBreakthrough ( ) {
		throw new System.NotImplementedException( );
	}

	public override void OnHeightChange ( ) {
		throw new System.NotImplementedException( );
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakthroughBoardArea : BoardArea {
	#region Unity Functions
	protected override void OnValidate ( ) {
		base.OnValidate( );

		boardAreaSizer.Recolor(ThemeSettingsManager.Instance.BreakthroughColor);
	}
	#endregion

	public override void OnBlockGroupMerge ( ) {
		throw new System.NotImplementedException( );
	}

	public override void OnBreakthrough ( ) {
		throw new System.NotImplementedException( );
	}

	public override void OnHeightChange ( ) { }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextGridComponent : TextGridComponent {
	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		textMeshProUGUI.color = themeManager.GetRandomButtonColor( );
	}
	#endregion
}

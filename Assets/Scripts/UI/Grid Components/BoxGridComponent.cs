using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BoxGridComponent : GridComponent {
	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		onHoverColorFunction = ( ) => themeManager.GetRandomMinoColor( );
		onIdleColorFunction = ( ) => themeManager.GetRandomBackgroundDetailColor( );

		backgroundImage.color = onIdleColorFunction( );
	}
	#endregion
}

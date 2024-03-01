using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///		Apply to any GameObject that has certain elements that need to be updated when the theme changes
/// </summary>
public interface IThemeElement {
	/// <summary>
	///		Update this GameObject with new theme settings
	/// </summary>
	public void UpdateThemeElements ( );
}

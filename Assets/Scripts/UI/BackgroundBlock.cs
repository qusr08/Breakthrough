using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlock : MonoBehaviour {
	[Header("Components")]
	[SerializeField] public BackgroundBlockSpawner BackgroundBlockSpawner;

	private void Update ( ) {
		// If the background block is no longer inside the bounds of the background, then recalculate its values
		if (!BackgroundBlockSpawner.IsWithinBackgroundBounds(transform.position)) {
			BackgroundBlockSpawner.CalculateValues(this);
		}
	}
}
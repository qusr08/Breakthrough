using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PanelButton : MonoBehaviour {
	private bool wasMouseHovering;

	private bool IsMouseHovering {
		get {
			// Raycast all of the ui elements at the mouse position
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			eventData.position = Mouse.current.position.ReadValue();
			List<RaycastResult> raysastResults = new List<RaycastResult>( );
			EventSystem.current.RaycastAll(eventData, raysastResults);

			// If this panel button is one of the objects being hovered over, then return true
			// If not, then return false
			for (int index = 0; index < raysastResults.Count; index++) {
				if (raysastResults[index].gameObject == gameObject) {
					return true;
				}
			}

			return false;
		}
	}

	private void Update ( ) {
		// Determine when the mouse enters and exits the panel button
		if (IsMouseHovering) {
			if (!wasMouseHovering) {
				Debug.Log("Mouse enter");
			}
		} else {
			if (wasMouseHovering) {
				Debug.Log("Mouse exit");
			}
		}

		wasMouseHovering = IsMouseHovering;
	}
}

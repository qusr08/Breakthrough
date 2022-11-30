using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private TextMeshProUGUI totalPointsTextMesh;
	[SerializeField] private TextMeshProUGUI levelPointsTextMesh;
	[SerializeField] private TextMeshProUGUI breakthroughsTextMesh;
	[Space]
	[SerializeField] private Transform pointsEventParent;
	[Header("Prefabs")]
	[SerializeField] private GameObject pointsEventPrefab;
	[Header("Properties")]
	[SerializeField] public int BoardPoints;
	[SerializeField] public int TotalPoints;
	[SerializeField] public int Breakthroughs;

	private PointsEvent[ ] pointsEventList;

	private void Awake ( ) {
		pointsEventList = new PointsEvent[Utils.GetEnumSize(typeof(PointsEventType))];
	}

	public void TriggerPointsEvent (PointsEventType pointsEventType) {
		// If the points event type is not in the array already, it is not currently active
		// Create a points event in that case
		if (pointsEventList[(int) pointsEventType] == null) {
			pointsEventList[(int) pointsEventType] = Instantiate(pointsEventPrefab, pointsEventParent).GetComponent<PointsEvent>( );
			pointsEventList[(int) pointsEventType].PointsEventType = pointsEventType;
		}

		// Trigger the points event
		pointsEventList[(int) pointsEventType].Trigger( );
	}
}
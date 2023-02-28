using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] private GameObject boardUIParticlePrefab;
	[Header("Scene GameObjects")]
	[SerializeField] public BoardUILabel TotalPointsUILabel;
	[SerializeField] public BoardUILabel BoardPointsUILabel;
	[SerializeField] public BoardUILabel BreakthroughsUILabel;
	[Header("Points Values")]
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each destroyed block.")] public int PointsPerDestroyedBlock = 6;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each block dropped into the breakthrough area.")] public int PointsPerDroppedBlock = 12;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each breakthrough.")] public int PointsPerBreakthrough = 600;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each fully destroyed mino.")] public int PointsPerDestroyedMino = 60;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each time they fast drop the mino.")] public int PointsPerFastDrop = 2;

	public int BoardPoints {
		get => int.Parse(BoardPointsUILabel.Value.text);
		set => BoardPointsUILabel.Value.text = value.ToString( );
	}
	public int TotalPoints {
		get => int.Parse(TotalPointsUILabel.Value.text);
		set => TotalPointsUILabel.Value.text = value.ToString( );
	}
	public int Breakthroughs {
		get => int.Parse(BreakthroughsUILabel.Value.text);
		set => BreakthroughsUILabel.Value.text = value.ToString( );
	}
}
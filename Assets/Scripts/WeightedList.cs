using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedList<T> {
	private List<T> _list;
	private List<bool> _enabledIndices;
	private List<T> _enabledList;
	private List<float> _percentages;
	private float _weightPercentage;

	#region Properties
	public List<T> List { get => _list; set => _list = value; }
	public List<bool> EnabledIndices { get => _enabledIndices; set => _enabledIndices = value; }
	public List<T> EnabledList { get => _enabledList; private set => _enabledList = value; }
	public List<float> Percentages { get => _percentages; private set => _percentages = value; }
	public float WeightPercentage { get => _weightPercentage; set => _weightPercentage = value; }

	public T this[int i] => List[i];
	#endregion

	public WeightedList (List<T> list, List<bool> enabledIndices, float weightPercentage) {
		List = list;
		EnabledIndices = enabledIndices;
		WeightPercentage = weightPercentage;

		EnabledList = new List<T>( );

		Refresh( );
	}

	public T GetWeightedValue ( ) {
		// Generate a random number between 0 and 1
		float randomValue = Random.Range(0f, 1f);

		// Loop through all indices of the array
		float percentageSum = 0f;
		int chosenIndex = -1;
		for (int i = 0; i < EnabledList.Count; i++) {
			// Ingore percentage values that have not been set
			if (Percentages[i] == 0f) {
				continue;
			}

			// Add up percentage values until it is over the random value
			percentageSum += _percentages[i];

			// If the percentage sum is less than the percentage sum,
			// the chosen index is the current iterator value
			if (randomValue <= percentageSum) {
				chosenIndex = i;

				break;
			}
		}

		// Calculate the percentage that will be subtracted from the chosen index
		float percentageSubtracted = Percentages[chosenIndex] * (1 - WeightPercentage);
		Percentages[chosenIndex] *= WeightPercentage;
		float percentageIncrease = percentageSubtracted / (EnabledList.Count - 1);

		// Adjust the percentage values to weight the indices based on the one that has been chosen
		for (int i = 0; i < EnabledList.Count; i++) {
			// Ignore percentage values that have not been set
			// Also ignore the chosen index as nothing will be added to it
			if (Percentages[i] == 0f || i == chosenIndex) {
				continue;
			}

			Percentages[i] += percentageIncrease;
		}

		return List[chosenIndex];
	}

	public void Refresh ( ) {
		// Make sure the lists are the same size
		while (EnabledIndices.Count < List.Count) {
			EnabledIndices.Add(default);
		}
		while (List.Count < EnabledIndices.Count) {
			List.Add(default);
		}

		// Create a list of all the enabled list items in the array
		EnabledList = new List<T>( );
		for (int i = 0; i < List.Count; i++) {
			if (EnabledIndices[i]) {
				EnabledList.Add(List[i]);
			}
		}

		// Reset the percentages array
		Percentages = new List<float>( );
		for (int i = 0; i < EnabledList.Count; i++) {
			Percentages.Add(1f / EnabledList.Count);
		}
	}
}
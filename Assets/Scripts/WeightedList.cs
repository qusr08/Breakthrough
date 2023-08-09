using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class WeightedList {
	private float[ ] percentages;
	private int allowedSum;
	private long activeIndicesBits;

	private int _size;

	#region Properties
	public float this[int index] => percentages[index];
	public int Size { get => _size; private set => _size = value; }
	#endregion

	public WeightedList (int size, long activeIndicesBits) {
		Size = size;
		this.activeIndicesBits = activeIndicesBits;

		Reset( );
	}

	public int GetWeightedValue ( ) {
		// Generate a random number between 0 and 1
		float randomValue = Random.Range(0f, 1f);

		// Loop through all indices of the array
		float percentageSum = 0f;
		int chosenIndex = -1;
		for (int i = 0; i < Size; i++) {
			// Ingore percentage values that have not been set
			if (percentages[i] == 0f) {
				continue;
			}

			// Add up percentage values until it is over the random value
			percentageSum += percentages[i];

			// If the percentage sum is less than the percentage sum,
			// the chosen index is the current iterator value
			if (randomValue <= percentageSum) {
				chosenIndex = i;

				break;
			}
		}

		// Calculate the percentage that will be subtracted from the chosen index
		float percentageSubtracted = percentages[chosenIndex] * (1 - Constants.WEIGHT_PERC);
		percentages[chosenIndex] *= Constants.WEIGHT_PERC;
		float percentageIncrease = percentageSubtracted / (allowedSum - 1);

		// Adjust the percentage values to weight the indices based on the one that has been chosen
		for (int i = 0; i < Size; i++) {
			// Ignore percentage values that have not been set
			// Also ignore the chosen index as nothing will be added to it
			if (percentages[i] == 0f || i == chosenIndex) {
				continue;
			}

			percentages[i] += percentageIncrease;
		}

		Utils.PrintArray(percentages);

		return chosenIndex;
	}

	public void Reset ( ) {
		percentages = new float[Size];

		// https://forum.tutorials7.com/1346/bitwise-c%23-extract-bit-from-integer

		// Find the sum of all the allowed indices
		allowedSum = 0;
		for (int i = 0; i < Size; i++) {
			if (((activeIndicesBits >> i) & 1) == 1) {
				allowedSum++;
			}
		}

		// Set default values for each of the percentages
		for (int i = 0; i < Size; i++) {
			if (((activeIndicesBits >> i) & 1) == 1) {
				percentages[i] = 1f / allowedSum;
			}
		}
	}
}

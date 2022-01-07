using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Reiikz.UnityUtils {

    public class RandomChoice
    {
        /// <summary>
        ///     Takes in an array of weights (probabilities) and returns the index of the selected choice
        /// </summary>
        public static int WeightedIndex(int[] weights)
        {
            // Get the total sum of all the weights.
            int weightSum = 0;
            for (int i = 0; i < weights.Length; ++i)
            {
                weightSum += weights[i];
            }
        
            // Step through all the possibilities, one by one, checking to see if each one is selected.
            int index = 0;
            int lastIndex = weights.Length - 1;
            while (index < lastIndex)
            {
                // Do a probability check with a likelihood of weights[index] / weightSum.
                if (Random.Range(0, weightSum) < weights[index])
                {
                    return index;
                }
        
                // Remove the last item from the sum of total untested weights and try again.
                weightSum -= weights[index++];
            }
        
            // No other item was selected, so return very last index.
            return index;
        }
    }

}


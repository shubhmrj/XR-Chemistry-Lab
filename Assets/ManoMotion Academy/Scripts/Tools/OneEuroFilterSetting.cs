using System;
using UnityEngine;

namespace ManoMotion
{
    /// <summary>
    /// OneEuroFilter settings to make it faster to implement new filters.
    /// </summary>
    [Serializable]
    public class OneEuroFilterSetting
    {
        public float freq = 120;
        public float mincutoff = 0.0001f;
        public float beta = 500f;
        public float dcutoff = 1f;

        static float minBeta = 25, maxBeta = 250;

        public OneEuroFilterSetting(float freq, float mincutoff, float beta, float dcutoff)
        {
            this.freq = freq;
            this.mincutoff = mincutoff;
            this.beta = beta;
            this.dcutoff = dcutoff;
        }

        public float CalculateOneEuroSmoothing(float smoothingValue)
        {
            float inverseSmoothing = Mathf.Abs(1 - smoothingValue);
            beta = inverseSmoothing * (maxBeta - minBeta) + minBeta;
            return beta;
        }
    }
}
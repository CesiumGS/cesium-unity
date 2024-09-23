using Unity.Mathematics;

namespace CesiumForUnity
{
    public class CesiumSampleHeightResult
    {
        public double3[] longitudeLatitudeHeightPositions { get; set; }
        public bool[] heightSampled { get; set; }
        public string[] warnings { get; set; }
    }
}

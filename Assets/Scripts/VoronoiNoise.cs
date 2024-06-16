using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Voronoi Noise implementation. <br/>
/// Using https://github.com/Scrawk/Procedural-Noise/blob/master/Assets/ProceduralNoise/Noise/PermutationTable.cs
/// </summary>
namespace Voronoi
{
    public class VoronoiNoise
    {

        public enum VORONOI_DISTANCE { EUCLIDIAN, MANHATTAN, CHEBYSHEV };

        public enum VORONOI_COMBINATION { D0, D1_D0, D2_D0 };

        public VORONOI_DISTANCE Distance { get; set; }

        public VORONOI_COMBINATION Combination { get; set; }

        private PermutationTable Perm { get; set; }

        public float Frequency;
        public float Amplitude;
        public Vector3 Offset;

        public VoronoiNoise(int seed, float frequency, float amplitude = 1.0f)
        {
            Frequency = frequency;
            Amplitude = amplitude;
            Offset = Vector3.zero;

            Distance = VORONOI_DISTANCE.EUCLIDIAN;
            Combination = VORONOI_COMBINATION.D1_D0;

            Perm = new PermutationTable(1024, int.MaxValue, seed);

        }

        /// <summary>
        /// Update the seed.
        /// </summary>
        public void UpdateSeed(int seed)
        {
            Perm.Build(seed);
        }

        /// <summary>
        /// Sample the noise in 2 dimensions.
        /// </summary>
        public float Sample2D(float x, float y)
        {
            //The 0.75 is to make the scale simliar to the other noise algorithms
            x = (x + Offset.x) * Frequency * 0.75f;
            y = (y + Offset.y) * Frequency * 0.75f;

            int lastRandom, numberFeaturePoints;
            float randomDiffX, randomDiffY;
            float featurePointX, featurePointY;
            int cubeX, cubeY;

            Vector3 distanceArray = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

            //1. Determine which cube the evaluation point is in
            int evalCubeX = (int)Mathf.Floor(x);
            int evalCubeY = (int)Mathf.Floor(y);

            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    cubeX = evalCubeX + i;
                    cubeY = evalCubeY + j;

                    //2. Generate a reproducible random number generator for the cube
                    lastRandom = Perm[cubeX, cubeY];

                    //3. Determine how many feature points are in the cube
                    numberFeaturePoints = ProbLookup(lastRandom * Perm.Inverse);

                    //4. Randomly place the feature points in the cube
                    for (int l = 0; l < numberFeaturePoints; ++l)
                    {
                        lastRandom = Perm[lastRandom];
                        randomDiffX = lastRandom * Perm.Inverse;

                        lastRandom = Perm[lastRandom];
                        randomDiffY = lastRandom * Perm.Inverse;

                        featurePointX = randomDiffX + cubeX;
                        featurePointY = randomDiffY + cubeY;

                        //5. Find the feature point closest to the evaluation point. 
                        //This is done by inserting the distances to the feature points into a sorted list
                        // distanceArray = Insert(distanceArray, Distance2(x, y, featurePointX, featurePointY));

                        float newDistance = Distance2(x, y, featurePointX, featurePointY);
                        float temp;
                        for (int d = 2; d >= 0; d--)
                        {
                            if (newDistance > distanceArray[d]) break;
                            temp = distanceArray[d];
                            distanceArray[d] = newDistance;
                            if (d + 1 < 3) distanceArray[d + 1] = temp;
                        }
                    }
                }
            }
            return Combine(distanceArray) * Amplitude;
        }

        private float Distance2(float p1x, float p1y, float p2x, float p2y)
        {
            return Distance switch
            {
                VORONOI_DISTANCE.EUCLIDIAN => (p1x - p2x) * (p1x - p2x) + (p1y - p2y) * (p1y - p2y),
                VORONOI_DISTANCE.MANHATTAN => Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y),
                VORONOI_DISTANCE.CHEBYSHEV => Math.Max(Math.Abs(p1x - p2x), Math.Abs(p1y - p2y)),
                _ => 0,
            };
        }

        private float Combine(Vector3 arr)
        {
            return Combination switch
            {
                VORONOI_COMBINATION.D0 => arr[0],
                VORONOI_COMBINATION.D1_D0 => arr[1] - arr[0],
                VORONOI_COMBINATION.D2_D0 => arr[2] - arr[0],
                _ => 0,
            };
        }

        /// <summary>
        /// Given a uniformly distributed random number this function returns the number of feature points in a given cube.
        /// </summary>
        /// <param name="value">a uniformly distributed random number</param>
        /// <returns>The number of feature points in a cube.</returns>
        int ProbLookup(float value)
        {
            //Poisson Distribution
            if (value < 0.0915781944272058) return 1;
            if (value < 0.238103305510735) return 2;
            if (value < 0.433470120288774) return 3;
            if (value < 0.628836935299644) return 4;
            if (value < 0.785130387122075) return 5;
            if (value < 0.889326021747972) return 6;
            if (value < 0.948866384324819) return 7;
            if (value < 0.978636565613243) return 8;

            return 9;
        }

        /// <summary>
        /// Inserts value into array using insertion sort. If the value is greater than the largest value in the array
        /// it will not be added to the array.
        /// </summary>
        /// <param name="arr">The array to insert the value into.</param>
        /// <param name="value">The value to insert into the array.</param>
        Vector3 Insert(Vector3 arr, float value)
        {
            float temp;
            for (int i = 2; i >= 0; i--)
            {
                if (value > arr[i]) break;
                temp = arr[i];
                arr[i] = value;
                if (i + 1 < 3) arr[i + 1] = temp;
            }

            return arr;
        }
    }
}

namespace Voronoi
{
    internal class PermutationTable
    {

        public int Size { get; private set; }

        public int Seed { get; private set; }

        public int Max { get; private set; }

        public float Inverse { get; private set; }

        private int Wrap;

        private int[] Table;

        internal PermutationTable(int size, int max, int seed)
        {
            Size = size;
            Wrap = Size - 1;
            Max = Math.Max(1, max);
            Inverse = 1.0f / Max;
            Build(seed);
        }

        internal void Build(int seed)
        {
            if (Seed == seed && Table != null) return;

            Seed = seed;
            Table = new int[Size];

            System.Random rnd = new System.Random(Seed);

            for (int i = 0; i < Size; i++)
            {
                Table[i] = rnd.Next();
            }
        }

        internal int this[int i]
        {
            get
            {
                return Table[i & Wrap] & Max;
            }
        }

        internal int this[int i, int j]
        {
            get
            {
                return Table[(j + Table[i & Wrap]) & Wrap] & Max;
            }
        }
    }
}

#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public struct Args
    {
        public const int RESOLUTION = 0;
        /// <summary>
        /// Actually it's the simulation space, depending on the Space property of the biome set to World or Self
        /// </summary>
        public const int WORLD_BOUNDS = 1;
        public const int TERRAIN_HEIGHT = 2;
        public const int SEED = 3;
        public const int OUTPUT_TEMP_HEIGHT = 4;
        public const int BIOME_SCALE = 5;
        public const int BIOME_SPACE = 6;
        public const int BIOME_WORLD_BOUNDS = 7;

        public int intValue { get; set; }
        public float floatValue { get; set; }
        public Vector4 vectorValue { get; set; }
        public bool boolValue { get; set; }

        public static Args Create(int v)
        {
            Args args = new Args();
            args.intValue = v;
            return args;
        }

        public static Args Create(float v)
        {
            Args args = new Args();
            args.floatValue = v;
            return args;
        }

        public static Args Create(Vector4 v)
        {
            Args args = new Args();
            args.vectorValue = v;
            return args;
        }

        public static Args Create(bool v)
        {
            Args args = new Args();
            args.boolValue = v;
            return args;
        }
    }
}
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;

namespace blockengine
{
    struct WorldGen_Carver
    {
        public FastNoiseLite fnl;
        public float amp;
        public int offset;
        public WorldGen_Carver(float amplitude, int _offset, FastNoiseLite _fnl) {
            fnl = _fnl;
            amp = amplitude;
            offset = _offset;
        }

        public bool Carve(Int3 pos)
        {
            return fnl.GetNoise(pos.x + offset, pos.y + offset, pos.z + offset) > amp;
        }
    }

    enum OreGen_Type
    {
        OreGen_Single,
        OreGen_Cluster,
        OreGen_Vein
    }

    struct WorldGen_Ore
    {
        public OreGen_Type gentype;
        public int oreblock;
        public int rarity;
        public WorldGen_Ore(int _genblock, int _rarity ,OreGen_Type _gentype)
        {
            oreblock = _genblock;
            gentype = _gentype;
            rarity = _rarity;
        }
    }

    public class WorldGenerator
    {
        private int world_seed;

        private List<WorldGen_Carver> carvers;
        public WorldGenerator(int _seed)
        {
            world_seed = _seed;

            carvers = new List<WorldGen_Carver>();

            Raylib.SetRandomSeed((uint)_seed);
        }

        //ADDS A CARVER TO THE WORLD GEN
        //CARVERS are responsible for carving shapes out of the world for caves
        public void AddCarver(float amplitude, float frequency, int octaves, int offset = 0, FastNoiseLite.NoiseType noise = FastNoiseLite.NoiseType.OpenSimplex2, FastNoiseLite.FractalType fractal = FastNoiseLite.FractalType.FBm)
        {
            var fnl = new FastNoiseLite(world_seed);

            fnl.SetFrequency(frequency);
            fnl.SetFractalOctaves(octaves);
            fnl.SetNoiseType(noise);
            fnl.SetFractalType(fractal);

            carvers.Add(new WorldGen_Carver(amplitude, offset, fnl));
        }

        public void AddCarverEX(float amplitude, int offset, FastNoiseLite fnl)
        {
            carvers.Add(new WorldGen_Carver(amplitude, offset, fnl));
        }

        public string GetBlock(Int3 world_block_pos)
        {
            string block = "GREY_STONE";  /// the default block

            //CARVING STAGE
            ///CARVES HOLES

            foreach (WorldGen_Carver carver in carvers)
            {
                if (carver.Carve(world_block_pos))
                {
                    block = "AIR";
                }
            }

            return block;
        }
    }
}

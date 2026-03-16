using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using System.Numerics;
using System.ComponentModel;

namespace blockengine
{
    
    public enum BiomeType : int
    {
        Caverns = 0,

        Mushroom,

        BiomeCount
    }

    public struct TempRange
    {
        public float Min;
        public float Max;

        public TempRange(float min = -1f, float max = 1f)
        {
            Min = min;
            Max = max;
        }
    }
    public struct Biome
    {
        public BiomeType type;
        public BlockType dirt_block; // the dirt/stone block that takes up most of the space
        public BlockType grass_block;
        public TempRange temp_range; // the range of world temperture at which this biome generates
        public Color fog_color;

        //public FastNoiseLite noise;
        public float noise_amp;
        public float noise_scale;
        
        public Biome(BiomeType type, BlockType dirt_block, BlockType grass_block, Color fog_color, TempRange temp_range,float noise_amp= 0.7f, float noise_scale = 0.01f)
        {
            this.noise_amp = noise_amp;
            this.type = type;
            this.dirt_block = dirt_block;
            this.grass_block = grass_block;
            this.noise_scale = noise_scale;
            this.temp_range = temp_range;
            this.fog_color = fog_color;
        }
    }

    public struct StructureGenPos
    {
        public Int3 position;
        public StructuresType type;

        public StructureGenPos(Int3 pos,StructuresType type)
        {
            position = pos;
            this.type = type;
        }
    }

    public struct GenPoint
    {
        public Int3 pos;
        public Int3 raw_pos;
        public Int3 cell_pos;
        public Int3 area_pos;

        public GenPoint(Int3 p, Int3 raw_p,Int3 cell_pos, Int3 area_pos)
        {
            pos = p; raw_pos = raw_p; this.cell_pos = cell_pos; this.area_pos = area_pos;
        }
    }

    public struct GeneratorBiomeProvider
    {
        public FastNoiseLite noise;
        public BiomeType type;
        public Int3 noise_offset;

        public GeneratorBiomeProvider(BiomeType type,FastNoiseLite fnl,Int3 offset)
        {
            this.type = type;
            noise = fnl;
            noise_offset = offset;
        }

        public Biome GetDef()
        {
            return Globals.BiomeDefinitions[type];
        }
    }

    public abstract class WorldGenerator
    {
        private int world_seed;
        private Random rand;
        private Dictionary<StructuresType, Int3> structure_offsets = new Dictionary<StructuresType, Int3>();
        private Dictionary<BiomeType,GeneratorBiomeProvider> biomes = new Dictionary<BiomeType,GeneratorBiomeProvider>();
        private FastNoiseLite graphfnl;
        private FastNoiseLite graphfnl2;
        private FastNoiseLite graphfnl3;
        public WorldGenerator(WorldInfo worldinfo)
        {
            rand = new Random(worldinfo.world_seed);
            world_seed = worldinfo.world_seed;

            var freq = 1;

            graphfnl = new FastNoiseLite(worldinfo.world_seed * 2);
            graphfnl.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            graphfnl.SetFrequency(freq);

            graphfnl2 = new FastNoiseLite(worldinfo.world_seed * 3);
            graphfnl2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            graphfnl2.SetFrequency(freq);

            graphfnl3 = new FastNoiseLite(worldinfo.world_seed * 4);
            graphfnl3.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            graphfnl3.SetFrequency(freq);

            for (int i = 0; i<(int)StructuresType.Count; i++)
            {
                structure_offsets[(StructuresType)i] = new Int3(rand.Next(-1000, 1000), rand.Next(-1000, 1000), rand.Next(-1000, 1000));
            }
        }

        public void AddBiome(BiomeType biometype)
        {
            Biome biome_def = Globals.BiomeDefinitions[biometype];
            FastNoiseLite fnl = new FastNoiseLite(world_seed + ((int)biometype * 100));
            fnl.SetFrequency(biome_def.noise_scale);
            Int3 offset = new Int3(rand.Next(-1024, 1024), rand.Next(-1024, 1024), rand.Next(-1024, 1024));
            GeneratorBiomeProvider provider = new GeneratorBiomeProvider(biometype,fnl,offset);
            biomes.Add(biometype, provider);
        }

        public GeneratorBiomeProvider GetBiome(BiomeType type)
        {
            return biomes[type];
        }

        public bool BiomeExists(BiomeType type)
        {
            return biomes.ContainsKey(type);
        }

        public float GetTemp(Int3 blockpos) // the temperture map of the world
        {
            //return blockpos.z;
            return Math.Clamp(blockpos.z / 512f,-2f,2f);
        }

        public abstract bool IsAir(Int3 blockpos);

        public bool IsBiome(Int3 blockpos,BiomeType biome)
        {
            GeneratorBiomeProvider provider = GetBiome(biome);
            return provider.noise.GetNoise(blockpos.x, blockpos.y, blockpos.z) > provider.GetDef().noise_amp;
        }

        public BiomeType GetChunkBiometype(Int3 chunkpos)
        {
            Int3 cp_wp = (chunkpos * Globals.chunk_size) + (Globals.chunk_size/2);
            return (BiomeType)GetBiomeAt(cp_wp);
        }

        public float GetTempFade(Int3 blockpos,BiomeType biometype)
        {
            Biome biome = Globals.BiomeDefinitions[biometype];
            float temp = GetTemp(blockpos);
            //Console.WriteLine(biome.temp_range.Max);
            if (temp > biome.temp_range.Max)
            {
                float abt = (temp - biome.temp_range.Max) / 0.15f;
                return 1 - abt;
            }
            else if (temp < biome.temp_range.Min)
            {
                float abt = MathF.Abs(temp - biome.temp_range.Min) / 0.15f;
                return 1 - abt;
            }
            return 1;
        }

        public bool IsBiomeBlock(Int3 blockpos, BiomeType biometype) // checks if the block is a part of the biome
        {
            if (!biomes.ContainsKey(biometype))
            {
                return false;
            }

            GeneratorBiomeProvider provider = GetBiome(biometype);
            Biome biome = provider.GetDef();

            Int3 p = blockpos + provider.noise_offset;

            float d = Globals.SampleDither(p) / 3f;
            float tempfade = GetTempFade(blockpos, biometype);
            float cn = (provider.noise.GetNoise(p.x, p.y, p.z) * 0.5f) + 0.5f;
            float n2 = cn * tempfade;

            if (n2 > biome.noise_amp)
            {
                return true;
            }
            else if (n2 > (biome.noise_amp - (6 * biome.noise_scale) * d) && n2 < biome.noise_amp)
            {
                return true;
            }
            else if (n2 > (biome.noise_amp - (12 * biome.noise_scale) * d) && n2 < biome.noise_amp)
            {
                return true;
            }
            return false;
        }

        public virtual int GetDefaultBiomeAt(Int3 blockpos)
        {
            return (int)BiomeType.Caverns;
        }

        protected int GetBiomeAt(Int3 blockpos)
        {
            for (int bi = 0; bi < (int)BiomeType.BiomeCount; bi++)
            {
                if (IsBiomeBlock(blockpos, (BiomeType)bi))
                {
                    return bi;
                }
            }
            return GetDefaultBiomeAt(blockpos);
        }


        public virtual BlockType GetBlockAt(Int3 blockpos)
        {
            if (IsAir(blockpos))
            {
                return BlockType.AirBlock;
            }

            int biome_id = GetBiomeAt(blockpos);

            Biome biome = Globals.BiomeDefinitions[(BiomeType)biome_id];

            if (biome.grass_block != biome.dirt_block)
            {
                if (IsAir(blockpos + new Int3(0,0,1)))
                {
                    return biome.grass_block;
                }
            }
            return biome.dirt_block;
        }

        //SRUCTURE GENERATION

        public List<GenPoint> GetStructurePoints(Int3 chunk_pos,StructuresType structuretype,bool force_raw = false)
        {
            List<GenPoint> points = new List<GenPoint>();

            Structure structure = Globals.StructureDefinitions[structuretype];

            int w_xmax = structure.width - structure.anchor_pos.x;
            int h_ymax = structure.height - structure.anchor_pos.y;
            int d_zmax = structure.depth - structure.anchor_pos.z;

            int w_xmin = structure.anchor_pos.x;
            int h_ymin = structure.anchor_pos.y;
            int d_zmin = structure.anchor_pos.z;

            Int3 graph_scale = new Int3(Math.Max(structure.graph_scale.x,structure.width+1), Math.Max(structure.graph_scale.y,structure.height+1), Math.Max(structure.graph_scale.z, structure.depth+1));
            //Console.WriteLine(graph_scale + " -> " + structuretype);
            Int3 of = structure_offsets[structuretype];

            int randomness_x_min = Math.Max((graph_scale.x / 2) - w_xmin, 0);
            int randomness_y_min = Math.Max((graph_scale.y / 2) - h_ymin, 0);
            int randomness_z_min = Math.Max((graph_scale.z / 2) - d_zmin, 0);

            int randomness_x_max = Math.Max((graph_scale.x / 2) - w_xmax, 0);
            int randomness_y_max = Math.Max((graph_scale.y / 2) - h_ymax, 0);
            int randomness_z_max = Math.Max((graph_scale.z / 2) - d_zmax, 0);

            Int3 cp_wp = chunk_pos * Globals.chunk_size;

            int area_min_x = cp_wp.x - w_xmin;
            int area_min_y = cp_wp.y - h_ymin;
            int area_min_z = cp_wp.z - d_zmin;

            int area_max_x = (cp_wp.x + Globals.chunk_size.x) + w_xmax;
            int area_max_y = (cp_wp.y + Globals.chunk_size.y) + h_ymax;
            int area_max_z = (cp_wp.z + Globals.chunk_size.z) + d_zmax;

            Int3 start = new Int3(
                (int)(MathF.Floor((float)area_min_x / (float)graph_scale.x) * (float)graph_scale.x),
                (int)(MathF.Floor((float)area_min_y / (float)graph_scale.y) * (float)graph_scale.y),
                (int)(MathF.Floor((float)area_min_z / (float)graph_scale.z) * (float)graph_scale.z)
            );

            //Console.WriteLine(area_min_x);
            //Console.WriteLine(area_min_y);
            //Console.WriteLine(area_min_z);
            //Console.WriteLine(start.x + ", " + start.y + ", " + start.z + " --> " + chunk_pos.x + "," + chunk_pos.y + ", " + chunk_pos.z);
            

            for (int x = 0; x < ((area_max_x - area_min_x) / graph_scale.x) + 2; x++)
            {
                for (int y = 0; y < ((area_max_y - area_min_y) / graph_scale.y) + 2; y++)
                {
                    for (int z = 0; z < ((area_max_z - area_min_z) / graph_scale.z) + 2; z++)
                    {
                        Int3 po = new Int3(x * graph_scale.x, y * graph_scale.y, z * graph_scale.z);
                        Int3 p = new Int3(start.x+po.x,start.y+po.y,start.z+po.z);
                        //Console.WriteLine(p.x/graph_scale + ", " + p.y/graph_scale + ", " + p.z/graph_scale);
                        Int3 cellpos = new Int3(p.x,p.y,p.z);

                        int cellminx = cellpos.x - (graph_scale.x / 2);
                        int cellminy = cellpos.y - (graph_scale.y / 2);
                        int cellminz = cellpos.z - (graph_scale.z / 2);

                        int cellmaxx = cellpos.x + (graph_scale.x / 2);
                        int cellmaxy = cellpos.y + (graph_scale.y / 2);
                        int cellmaxz = cellpos.z + (graph_scale.z / 2);

                        
                        float noise_x = graphfnl.GetNoise(p.x + of.x, p.y + of.y, p.z + of.z);
                        float noise_y = graphfnl2.GetNoise(p.x + of.x, p.y + of.y, p.z + of.z);
                        float noise_z = graphfnl3.GetNoise(p.x + of.x, p.y + of.y, p.z + of.z);
                        Vector3 n = new Vector3(noise_x, noise_y, noise_z);
                        if (n.X < 0)
                        {
                            n.X *= randomness_x_min;
                        }
                        else
                        {
                            n.X *= randomness_x_max;
                        }
                        if (n.Y < 0)
                        {
                            n.Y *= randomness_y_min;
                        }
                        else
                        {
                            n.Y *= randomness_y_max;
                        }
                        if (n.Z < 0)
                        {
                            n.Z *= randomness_z_min;
                        }
                        else
                        {
                            n.Z *= randomness_z_max;
                        }

                        p += new Int3((int)n.X, (int)n.Y, (int)n.Z);

                        bool generate = true;
                        
                        if (structure.structurePositionType == StructurePositionType.OnGround)
                        {
                            generate = false;

                            bool last_up = IsAir(p);
                            bool last_down = last_up;

                            int cd = 0;
                            while (true)
                            {
                                cd++;

                                bool up = IsAir(p + new Int3(0, 0, 1));
                                bool down = IsAir(p + new Int3(0, 0, -1));

                                bool in_up = false;
                                bool in_down = false;
                                if (p.z-d_zmin-cd > cellminz)
                                {
                                    in_down = true;
                                    //Console.WriteLine(cd);
                                    if (last_down && !down)
                                    {
                                        p.z -= cd;
                                        generate = true;
                                        break;
                                    }
                                }
                                if (p.z + d_zmax + cd < cellmaxz)
                                {
                                    in_up = true;
                                    //Console.WriteLine(cd);
                                    if (!last_up && up)
                                    {
                                        p.z += cd;
                                        generate = true;
                                        break;
                                    }
                                }


                                if (!in_up && !in_down)
                                {
                                    break;
                                }

                                last_up = up;
                                last_down = down;
                            }
                        }

                        Int3 p_cp = Globals.WBP_to_ChunkPos(p);
                        BiomeType chunk_biome = GetChunkBiometype(p_cp);

                        if (generate && structure.CanGenerateInBiome(chunk_biome))
                        {
                            //Console.WriteLine("Added: " + p.x + ", " + p.y + ", " + p.z);
                            points.Add(new GenPoint(p, cellpos, new Int3(cellminx, cellminy, cellminz),new Int3(area_min_x,area_min_y,area_min_z)));
                        }
                    }
                }
            }
            //Console.WriteLine("POINTS RAN");
            return points;
        }

        public List<StructureGenPos> GetChunkStructures(Int3 chunk_pos, int structure_index = 0)
        {
            List<StructureGenPos> final_points = new List<StructureGenPos>();

            StructuresType t = (StructuresType)structure_index;
            Structure structure = Globals.StructureDefinitions[t];
            int w_xmax = structure.width - structure.anchor_pos.x;
            int h_ymax = structure.height - structure.anchor_pos.y;
            int d_zmax = structure.depth - structure.anchor_pos.z;

            int w_xmin = structure.anchor_pos.x;
            int h_ymin = structure.anchor_pos.y;
            int d_zmin = structure.anchor_pos.z;

            List<GenPoint> points = GetStructurePoints(chunk_pos, t);

            Int3 cp_wp = chunk_pos * Globals.chunk_size;
            int cminx = cp_wp.x;
            int cminy = cp_wp.y;
            int cminz = cp_wp.z;
            int cmaxx = cp_wp.x + Globals.chunk_size.x;
            int cmaxy = cp_wp.y + Globals.chunk_size.y;
            int cmaxz = cp_wp.z + Globals.chunk_size.z;

            if (structure_index == (int)StructuresType.Count-1)
            {
                //Console.WriteLine(t + " -> " + ":) " + points.Count);
                for (int i = 0; i< points.Count; i++)
                {
                    Int3 p = points[i].pos;
                    final_points.Add(new StructureGenPos(p,t));
                }
            }
            else
            {
                //Console.WriteLine(t + " -> " + (t + 1));
                List<StructureGenPos> points2 = GetChunkStructures(chunk_pos, structure_index + 1);
                for (int i = 0; i < points.Count; i++)
                {
                    Int3 p = points[i].pos;
                    int pminx = p.x - w_xmin;
                    int pminy = p.y - h_ymin;
                    int pminz = p.z - d_zmin;

                    int pmaxx = p.x + w_xmax;
                    int pmaxy = p.y + h_ymax;
                    int pmaxz = p.z + d_zmax;

                    bool gen = true;
                    for (int i2 = 0; i2 < points2.Count; i2++)
                    {
                        StructureGenPos p2 = points2[i2];
                        Structure structure2 = Globals.StructureDefinitions[p2.type];
                        int w_xmax2 = structure2.width - structure2.anchor_pos.x;
                        int h_ymax2 = structure2.height - structure2.anchor_pos.y;
                        int d_zmax2 = structure2.depth - structure2.anchor_pos.z;

                        int w_xmin2 = structure2.anchor_pos.x;
                        int h_ymin2 = structure2.anchor_pos.y;
                        int d_zmin2 = structure2.anchor_pos.z;

                        int p2minx = p2.position.x - w_xmin2;
                        int p2miny = p2.position.y - h_ymin2;
                        int p2minz = p2.position.z - d_zmin2;

                        int p2maxx = p2.position.x + w_xmax2;
                        int p2maxy = p2.position.y + h_ymax2;
                        int p2maxz = p2.position.z + d_zmax2;

                        int area_min_x = cp_wp.x - w_xmin2;
                        int area_min_y = cp_wp.y - h_ymin2;
                        int area_min_z = cp_wp.z - d_zmin2;
                        
                        int area_max_x = (cp_wp.x + Globals.chunk_size.x) + w_xmax2;
                        int area_max_y = (cp_wp.y + Globals.chunk_size.y) + h_ymax2;
                        int area_max_z = (cp_wp.z + Globals.chunk_size.z) + d_zmax2;

                        if (pminx < area_max_x && pmaxx > area_min_x && pminy<area_max_y && pmaxy > area_min_y && pminz < area_max_z && pmaxz > area_min_z)
                        {
                            if (pminx < p2maxx && pmaxx > p2minx && pminy < p2maxy && pmaxy > p2miny && pminz < p2maxz && pmaxz > p2minz)
                            {
                                gen = false;
                                break;
                            }
                        }
                    }

                    if (gen)
                    {
                        final_points.Add(new StructureGenPos(p, t));
                    }
                }

                final_points.AddRange(points2);
            }
            
            
            if (structure_index == 0) {
                List<StructureGenPos> final_final_points = new List<StructureGenPos>();

                for (int i = 0; i<final_points.Count; i++)
                {
                    StructureGenPos p2 = final_points[i];
                    Structure structure2 = Globals.StructureDefinitions[p2.type];
                    int w_xmax2 = structure2.width - structure2.anchor_pos.x;
                    int h_ymax2 = structure2.height - structure2.anchor_pos.y;
                    int d_zmax2 = structure2.depth - structure2.anchor_pos.z;

                    int w_xmin2 = structure2.anchor_pos.x;
                    int h_ymin2 = structure2.anchor_pos.y;
                    int d_zmin2 = structure2.anchor_pos.z;

                    int p2minx = p2.position.x - w_xmin2;
                    int p2miny = p2.position.y - h_ymin2;
                    int p2minz = p2.position.z - d_zmin2;

                    int p2maxx = p2.position.x + w_xmax2;
                    int p2maxy = p2.position.y + h_ymax2;
                    int p2maxz = p2.position.z + d_zmax2;

                    if (p2minx < cmaxx && p2maxx > cminx && p2miny < cmaxy && p2maxy > cminy && p2minz < cmaxz && p2maxz > cminz)
                    {
                        final_final_points.Add(p2);

                    }
                    

                }
                //Console.WriteLine("RAN");
                return final_final_points;
            }
            
            //Console.WriteLine("RAN");
            return final_points;
        }
    }

    public class WorldGenerator_Normal : WorldGenerator
    {
        private FastNoiseLite cave_gen_fnl;
        //private FastNoiseLite spaghetti_cave_gen_fnl;
        public WorldGenerator_Normal(WorldInfo worldinfo) : base(worldinfo)
        {
            cave_gen_fnl = new FastNoiseLite(worldinfo.world_seed);
            cave_gen_fnl.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            cave_gen_fnl.SetFractalType(FastNoiseLite.FractalType.FBm);
            cave_gen_fnl.SetFractalOctaves(3);
            cave_gen_fnl.SetFrequency(0.006f);
            //spaghetti_cave_gen_fnl = new FastNoiseLite(worldinfo.world_seed + 5);
            //spaghetti_cave_gen_fnl.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            //spaghetti_cave_gen_fnl.SetFractalType(FastNoiseLite.FractalType.Ridged);
            //spaghetti_cave_gen_fnl.SetFractalOctaves(1);
            //spaghetti_cave_gen_fnl.SetFrequency(0.01f);

            AddBiome(BiomeType.Mushroom);
            //AddBiome(new Biome(BiomeType.Mushroom, BlockType.BrownStoneBlock, new TempRange(-1f,1f), worldinfo.world_seed, 0.7f, 0.01f));
        }
        public override bool IsAir(Int3 blockpos)
        {
            //return blockpos.z != 0;
            return cave_gen_fnl.GetNoise(blockpos.x, blockpos.y, blockpos.z) > 0.4f;
        }
    }
}

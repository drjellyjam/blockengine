using Raylib_cs;
using static Raylib_cs.Raylib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace blockengine
{
    public static class TextureHandler
    {
        static int atlas_size = 512;
        private static bool fully_loaded = false;

        static public RenderTexture2D block_atlas;
        static public RenderTexture2D block_atlas_emissive;
        static private Dictionary<string, UV> texture_uvs = new Dictionary<string, UV>();
        static private Dictionary<string, Texture2D> sprites = new Dictionary<string, Texture2D>();
        //static private Dictionary<string, UV> texture_parts = new Dictionary<string, UV>();
        static public void CreateAtlasTextures()
        {
            int atlas_size_tiles = atlas_size / 16;
            block_atlas = LoadRenderTexture(atlas_size, atlas_size);
            block_atlas_emissive = LoadRenderTexture(atlas_size, atlas_size);
            bool emissive_exists = Directory.Exists("Assets/Textures/Emissive/");
            string[] block_texture_files = Directory.GetFiles("Assets/Textures/");


            //col map
            bool[][] atlas_colmap = new bool[atlas_size_tiles][];
            for (int i = 0; i< atlas_size_tiles; i++)
            {
                atlas_colmap[i] = new bool[32];
                for (int j = 0; j< atlas_size_tiles; j++)
                {
                    atlas_colmap[i][j] = false;
                }
            }

            bool check_colmap_space(int x, int y, int wid, int heit)
            {
                for (int tilex = 0; tilex <= wid; tilex++)
                {
                    for (int tiley = 0; tiley <= heit; tiley++)
                    {
                        int _x = x + tilex;
                        int _y = y + tiley;
                        if (_x < 0 || _x >= atlas_size_tiles || _y < 0 || _y >= atlas_size_tiles)
                        {
                            return false;
                        }
                        bool spot = atlas_colmap[_x][_y];
                        if (spot)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            void set_colmap_space(int x, int y, int wid, int heit)
            {
                for (int tilex = 0; tilex < wid; tilex++)
                {
                    for (int tiley = 0; tiley < heit; tiley++)
                    {
                        int _x = x + tilex;
                        int _y = y + tiley;
                        if (_x >= 0 && _x < atlas_size_tiles && _y >= 0 && _y < atlas_size_tiles)
                        {
                            atlas_colmap[_x][_y] = true;
                        }
                    }
                }
            }

            int atlas_x = 0;
            int atlas_y = 0;

            //File.Exists("Assets/Textures/Emissive/");

            BeginTextureMode(block_atlas_emissive);
            ClearBackground(Raylib_cs.Color.Black);
            EndTextureMode();

            foreach (string texturedir in block_texture_files)
            {
                Texture2D blocktexture = LoadTexture(texturedir);
                string name = Path.GetFileName(texturedir);

                //render texture to render texture

                int atlas_t_wid = (int)MathF.Ceiling(blocktexture.Width / 16f);
                int atlas_t_heit = (int)MathF.Ceiling(blocktexture.Height / 16f);

                Console.WriteLine(atlas_t_wid);
                Console.WriteLine(atlas_t_heit);

                //get next unfiled pos
                while (!check_colmap_space(atlas_x, atlas_y, atlas_t_wid, atlas_t_heit))
                {
                    atlas_x += 1;
                    if (atlas_x >= atlas_size_tiles)
                    {
                        atlas_x = 0;
                        atlas_y++;
                    }

                    if (atlas_y >= atlas_size_tiles)
                    {
                        Console.WriteLine("No more space in texture page MAKE IT BIGGER!!!");
                        break;
                    }
                }

                int real_atlas_x = atlas_x * 16;
                int real_atlas_y = atlas_y * 16;

                BeginTextureMode(block_atlas);
                DrawTexture(blocktexture, real_atlas_x, real_atlas_y, Raylib_cs.Color.White);
                EndTextureMode();
                UnloadTexture(blocktexture);
                if (emissive_exists)
                {
                    string emissive_name = name.Insert(name.Length - 4, "_emissive");
                    string emissive_dir = "Assets/Textures/Emissive/" + emissive_name;
                    if (File.Exists(emissive_dir))
                    {
                        Texture2D emissive_blocktexture = LoadTexture(emissive_dir);
                        BeginTextureMode(block_atlas_emissive);
                        DrawTexture(emissive_blocktexture, real_atlas_x, real_atlas_y, Raylib_cs.Color.White);
                        EndTextureMode();
                        UnloadTexture(emissive_blocktexture);

                        Console.WriteLine("Loaded Emissive texture: " + emissive_name);
                    }
                }

                UV texture_uv = new UV(
                    atlas_x,
                    atlas_y + atlas_t_heit, //xmax
                    atlas_x + atlas_t_wid,
                    atlas_y // xmin
                );
                texture_uv.XMAX /= atlas_size_tiles;
                texture_uv.YMAX /= atlas_size_tiles;
                texture_uv.XMIN /= atlas_size_tiles;
                texture_uv.YMIN /= atlas_size_tiles;
                texture_uv.YMIN = 1 - texture_uv.YMIN;
                texture_uv.YMAX = 1 - texture_uv.YMAX;

                texture_uvs.Add(texturedir, texture_uv);
                set_colmap_space(atlas_x, atlas_y, atlas_t_wid,atlas_t_heit);

                Console.WriteLine("Loaded texture: " + name);
            }

            string[] sprite_texture_files = Directory.GetFiles("Assets/Textures/Sprites/");

            foreach (string spritefile in sprite_texture_files)
            {
                sprites.Add(spritefile, Raylib.LoadTexture(spritefile));
            }

            fully_loaded = true;
        }
        static public UV GetTextureUV(string texture_dir)
        {
            if (!fully_loaded)
            {
                return new UV(0, 0, 1, 1);
            }
            if (texture_uvs.ContainsKey(texture_dir))
            {
                return texture_uvs[texture_dir];
            }
            return texture_uvs["Assets/Textures/missing.png"];
        }

        static public Texture2D GetSprite(string sprite_dir)
        {
            if (sprites.ContainsKey(sprite_dir))
            {
                return sprites[sprite_dir];
            }
            return sprites["Assets/Textures/Sprites/missing.png"];
        }
        static public void Cleanup()
        {
            UnloadRenderTexture(block_atlas);
        }
    }
}

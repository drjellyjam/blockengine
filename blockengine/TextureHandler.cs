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

        static public RenderTexture2D block_atlas;

        static private Dictionary<string, UV> texture_uvs = new Dictionary<string, UV>();
        //static private Dictionary<string, UV> texture_parts = new Dictionary<string, UV>();
        static public void CreateAtlasTextures()
        {
            block_atlas = LoadRenderTexture(atlas_size, atlas_size);
            string[] block_texture_files = Directory.GetFiles("Assets/Textures/");

            int atlas_x = 0;
            int atlas_y = 0;

            
            foreach (string texturedir in block_texture_files)
            {
                Texture2D blocktexture = LoadTexture(texturedir);

                //render texture to render texture
                int real_atlas_x = atlas_x * 16;
                int real_atlas_y = atlas_y * 16;

                BeginTextureMode(block_atlas);
                DrawTexture(blocktexture, real_atlas_x, real_atlas_y, Raylib_cs.Color.White);
                EndTextureMode();
                UnloadTexture(blocktexture);

                UV texture_uv = new UV(
                    0f+atlas_x,31f-atlas_y,1f + atlas_x, 32f - atlas_y
                );
                texture_uv.XMAX /= 32f;
                texture_uv.YMAX /= 32f;
                texture_uv.XMIN /= 32f;
                texture_uv.YMIN /= 32f;
                texture_uvs.Add(texturedir, texture_uv);

                Console.WriteLine("Loaded texture: " + texturedir);

                atlas_x += 1;
                if (atlas_x == atlas_size/16)
                {
                    atlas_x = 0;
                    atlas_y += 1;
                }
            }
            
        }
        static public UV GetTextureUV(string texture_dir)
        {
            if (texture_uvs.ContainsKey(texture_dir))
            {
                return texture_uvs[texture_dir];
            }
            return texture_uvs["Assets/Textures/missing.png"];
        }
        static public void Cleanup()
        {
            UnloadRenderTexture(block_atlas);
        }
    }
}

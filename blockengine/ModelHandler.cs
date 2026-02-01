using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;
using Raylib_cs;
using blockengine.Items;

namespace blockengine
{
    public static class ModelHandler
    {
        private static Dictionary<string, ParsedOBJ> models = new Dictionary<string, ParsedOBJ>();
        private static Material model_block_material;
        private static int shader_uniform_uv_top;
        private static int shader_uniform_uv_bottom;
        private static int shader_uniform_uv_forward;
        private static int shader_uniform_uv_backward;
        private static int shader_uniform_uv_left;
        private static int shader_uniform_uv_right;
        private static int shader_uniform_single_texture;
        private static int shader_uniform_atlastexture;
        private static int shader_uniform_atlastexture_emissive;
        public static bool fully_loaded = false;

        public static void DrawItem(ItemType itemtype,Matrix4x4 transform)
        {
            if (!fully_loaded) { return; }

            Item item = Globals.ItemDefinitions[itemtype];

            if (item.renderype == ItemRenderType.BlockRender)
            {
                DrawBlockType(item.blockrender, transform);
                return;
            }
            DrawOBJ(item.texture, item.viewmodel, transform);
        }

        public static void DrawOBJ(string texturedir,string modeldir,Matrix4x4 transform)
        {
            if (!fully_loaded) { return; }
            ParsedOBJ model = models[modeldir];

            Raylib.BeginShaderMode(model_block_material.Shader);
            UV ttop = TextureHandler.GetTextureUV(texturedir);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_top, ttop.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_single_texture, true, ShaderUniformDataType.Int);
            Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture, TextureHandler.block_atlas.Texture);
            Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture_emissive, TextureHandler.block_atlas_emissive.Texture);
            Raylib.DrawMesh(model.mesh, model_block_material, transform);
            Raylib.EndShaderMode();

        }
        public static void DrawBlock(BlockFaces<string> face_textures, Matrix4x4 transform)
        {
            if (!fully_loaded) { return; }
            ParsedOBJ model = models["Assets/Models/block.obj"];

            Raylib.BeginShaderMode(model_block_material.Shader);
            UV ttop = TextureHandler.GetTextureUV(face_textures.TOP);
            UV tbottom = TextureHandler.GetTextureUV(face_textures.BOTTOM);
            UV tforward = TextureHandler.GetTextureUV(face_textures.FORWARD);
            UV tbackward = TextureHandler.GetTextureUV(face_textures.BACKWARD);
            UV tright = TextureHandler.GetTextureUV(face_textures.RIGHT);
            UV tleft = TextureHandler.GetTextureUV(face_textures.LEFT);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_top, ttop.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_bottom, tbottom.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_forward, tforward.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_backward, tbackward.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_right, tright.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_left, tleft.ToVector4(), ShaderUniformDataType.Vec4);
            Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture, TextureHandler.block_atlas.Texture);
            Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture_emissive, TextureHandler.block_atlas_emissive.Texture);
            Raylib.DrawMesh(model.mesh, model_block_material, transform);
            Raylib.EndShaderMode();
        }
        public static void DrawBlockType(BlockType blocktype, Matrix4x4 transform)
        {
            if (!fully_loaded) { return; }
            Block block = Globals.BlockDefinitions[blocktype];
            if (block.BlockModel == null)
            {
                //Render block

                var face_textures = block.FaceTextureIds;
                ParsedOBJ model = models["Assets/Models/block.obj"];

                Raylib.BeginShaderMode(model_block_material.Shader);
                UV ttop = TextureHandler.GetTextureUV(face_textures.TOP);
                UV tbottom = TextureHandler.GetTextureUV(face_textures.BOTTOM);
                UV tforward = TextureHandler.GetTextureUV(face_textures.FORWARD);
                UV tbackward = TextureHandler.GetTextureUV(face_textures.BACKWARD);
                UV tright = TextureHandler.GetTextureUV(face_textures.RIGHT);
                UV tleft = TextureHandler.GetTextureUV(face_textures.LEFT);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_top, ttop.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_bottom, tbottom.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_forward, tforward.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_backward, tbackward.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_right, tright.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_left, tleft.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture, TextureHandler.block_atlas.Texture);
                Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture_emissive, TextureHandler.block_atlas_emissive.Texture);
                Raylib.DrawMesh(model.mesh, ModelHandler.model_block_material, transform);
                Raylib.EndShaderMode();
            }
            else
            {
                ParsedOBJ model = models[block.BlockModel.model_file];
                Raylib.BeginShaderMode(model_block_material.Shader);
                UV ttop = TextureHandler.GetTextureUV(block.BlockModel.model_texture);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_uv_top, ttop.ToVector4(), ShaderUniformDataType.Vec4);
                Raylib.SetShaderValue(model_block_material.Shader, shader_uniform_single_texture, true, ShaderUniformDataType.Int);
                Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture, TextureHandler.block_atlas.Texture);
                Raylib.SetShaderValueTexture(model_block_material.Shader, shader_uniform_atlastexture_emissive, TextureHandler.block_atlas_emissive.Texture);
                Raylib.DrawMesh(model.mesh, ModelHandler.model_block_material, transform);
                Raylib.EndShaderMode();
            }
        }

        public static ParsedOBJ? GetModel(string modeldir)
        {
            if (models.ContainsKey(modeldir)) { return models[modeldir]; }

            return null;
        }

        public static void LoadModels()
        {
            //load block model shader

            model_block_material = Raylib.LoadMaterialDefault();
            Shader block_model_shader = Raylib.LoadShader("Assets/Shaders/objblock_shader.vs", "Assets/Shaders/objblock_shader.fs");
            shader_uniform_uv_top = Raylib.GetShaderLocation(block_model_shader, "uv_top");
            shader_uniform_uv_bottom = Raylib.GetShaderLocation(block_model_shader, "uv_bottom");
            shader_uniform_uv_forward = Raylib.GetShaderLocation(block_model_shader, "uv_forward");
            shader_uniform_uv_backward = Raylib.GetShaderLocation(block_model_shader, "uv_backward");
            shader_uniform_uv_left = Raylib.GetShaderLocation(block_model_shader, "uv_left");
            shader_uniform_uv_right = Raylib.GetShaderLocation(block_model_shader, "uv_right");
            shader_uniform_single_texture = Raylib.GetShaderLocation(block_model_shader, "single_texture");
            shader_uniform_atlastexture = Raylib.GetShaderLocation(block_model_shader, "atlastexture");
            shader_uniform_atlastexture_emissive = Raylib.GetShaderLocation(block_model_shader, "atlastexture_emissive");
            model_block_material.Shader = block_model_shader;

            ///load models

            string[] model_files = Directory.GetFiles("Assets/Models/");

            foreach (string modeldir in model_files)
            {
                string[] lines = File.ReadAllLines(modeldir);
                ParsedOBJ parsed = new ParsedOBJ();

                foreach (string line in lines)
                {
                    string[] parts = line.Split(" ");
                    //Console.WriteLine(line);
                    switch (parts[0])
                    {
                        case "v": //POSITION
                            float x = Convert.ToSingle(parts[1]);
                            float y = Convert.ToSingle(parts[2]);
                            float z = Convert.ToSingle(parts[3]);
                            parsed.AddVertex(new Vector3(x, y, z));

                            
                            break;

                        case "vn": //NORMAL
                            float xn = Convert.ToSingle(parts[1]);
                            float yn = Convert.ToSingle(parts[2]);
                            float zn = Convert.ToSingle(parts[3]);
                            parsed.AddNormal(new Vector3(xn, yn, zn));
                            
                            break;

                        case "vt": //UV
                            float xuv = Convert.ToSingle(parts[1]);
                            float yuv = Convert.ToSingle(parts[2]);
                            parsed.AddUV(new Vector2(xuv, yuv));

                            
                            break;

                        case "f": //FACE
                            for (int i = 1; i<=3; i++)
                            {
                                string vertex = parts[i];
                                //Console.WriteLine(vertex);
                                string[] vertex_parts = vertex.Split("/");
                                
                                //Console.WriteLine(vertex_parts[0] + "/" + vertex_parts[1] + "/" + vertex_parts[2]);

                                int v = int.Parse(vertex_parts[0])-1;
                                int vt = int.Parse(vertex_parts[1])-1;
                                int vn = int.Parse(vertex_parts[2])-1;

                                parsed.AddFace(new Int3(v, vt, vn));
                            }
                            break;
                    }
                }

                parsed.UploadMesh();
                models[modeldir] = parsed;
                fully_loaded = true;
                Console.WriteLine("Parsed: " + modeldir + " (" + parsed.face_count + ")");
            }
        }
    }
}

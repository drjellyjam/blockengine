using blockengine;
using blockengine.Entitys;
using Raylib_cs;
using System.Numerics;
using System.Security.Cryptography;

namespace BlockEngine;
class Program
{
    public static void Main()
    {
        bool hacky_fix_draw_glitch = true;
        bool draw_debug = false;

        if (!Directory.Exists("Saves"))
        {
            Directory.CreateDirectory("Saves");
        }

        Raylib.InitWindow(1280, 720, "Block Engine");

        World world = new World(new WorldInfo("test world"));
        world.AddChunk(new Int3(0, 0, 0));
        //world.Generate();
        //world.Build();

        Player player = new Player(world);
        player.Position = new Vector3(8,8, 2);

        int max_frames = 5;
        int bi = 0;

        TextureHandler.CreateAtlasTextures();

        Task.Run(() =>
        {
            while (true)
            {
                world.GenerateAroundFlood(player.Position);
            }
        });

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F3))
            {
                draw_debug = !draw_debug;
            }

            //update

            player.Update();

            bi++;
            if (bi >= max_frames)
            {
                bi = 0;
                world.UploadChunkQueue();
            }

            //drawing

            Raylib.BeginDrawing();

            //behind 3d

            Raylib.ClearBackground(Color.Gray);

            //3d

            Raylib.BeginMode3D(player.cam);

            if (hacky_fix_draw_glitch)
            {
                hacky_fix_draw_glitch = false;
                Raylib.DrawCube(Vector3.Zero, 1, 1, 1, Color.White);
            }

            world.Draw(player.cam,3, draw_debug);
            player.Draw();

            Raylib.EndMode3D();

            //ui

            Raylib.DrawFPS(10, 10);

            var pos = new Int3((int)player.Position.X, (int)player.Position.Y, (int)player.Position.Z);
            var chunkpos = world.GetChunkBlockPosWorldPos(pos);
            var chunk_block_pos = world.GetChunkPosWorldPos(pos);
            Raylib.DrawText("(" + chunkpos.x + ", " + chunkpos.y + ", " + chunkpos.z + ")", 10, 64, 24, Color.White);
            Raylib.DrawText("(" + chunk_block_pos.x + ", " + chunk_block_pos.y + ", " + chunk_block_pos.z + ")", 10, 128, 24, Color.White);
            Raylib.DrawText(world.GetLoadedChunksCount().ToString(), 10, 128+64, 24, Color.Red);

            //Raylib.DrawTexture(TextureHandler.block_atlas.Texture, 0, 0, Color.Red);

            Raylib.EndDrawing();
        }
        TextureHandler.Cleanup();
        world.Cleanup();

        Raylib.CloseWindow();
    }
}
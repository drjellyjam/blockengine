using blockengine;
using blockengine.Entitys;
using blockengine.Items;
using Raylib_cs;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;

namespace BlockEngine;
class Program
{
    private static List<Int3> list = new List<Int3>();
    private static void flood(List<Int3> poslist,int d = 4,bool print = false)
    {
        if (d <= 0)
        {
            return;
        }

        var _newlist = new List<Int3>();
        foreach (Int3 pos in poslist)
        {
            if (!list.Contains(pos))
            {
                list.Add(pos);
                if (print)
                {
                    Console.WriteLine("new Int3(" + pos.x + "," + pos.y + "," + pos.z + "),");
                }
                foreach (Int3 norm in Globals.around_positions)
                {
                    var _npos = pos + norm;
                    if (!list.Contains(_npos))
                    {
                        _newlist.Add(_npos);

                    }
                }
            }
        }
        flood(_newlist, d - 1, print);
    }
    public static void Main()
    {
        //Console.WriteLine("precalculated flood positions");
        //flood(new List<Int3>(){ new Int3(0,0,0) },2);
        //Console.WriteLine("");
        //Console.WriteLine("Done " + list.Count);

        var pseudo = new PseudoRandom();
        for (int i = 0; i<(8*8)*8; i++)
        {
            Int3 pos = Globals.IndexToPosition(i, new Int3(8, 8, 8));
            Console.WriteLine(pseudo.GetTileInt(pos.x,pos.y,pos.z, 10));
        }
        
        
        
        bool hacky_fix_draw_glitch = true;
        bool draw_debug = false;

        if (!Directory.Exists("Saves"))
        {
            Directory.CreateDirectory("Saves");
        }


        Raylib.SetTraceLogLevel(TraceLogLevel.Error);
        Raylib.InitWindow(1280, 720, "Block Engine");
        Raylib.SetTargetFPS(120);

        ModelHandler.LoadModels();
        TextureHandler.CreateAtlasTextures();

        World world = new World(new WorldInfo("test world",333)); //Raylib.GetRandomValue(-99999,99999)
        world.ChangeFogColor(Color.Black);
        world.AddEntity(new PlayerEntity(world, "Player", Vector3.Zero),true);

        float max_frames = 0.025f;
        float bi = max_frames;

        
        Task.Run(() =>
        {
            while (!Raylib.WindowShouldClose())
            {
                world.GenerateArea();
            }
        });


        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F3))
            {
                draw_debug = !draw_debug;
            }

            //update

            world.UpdateWorld();

            bi -= 1 * Raylib.GetFrameTime();
            if (bi <= 0) {
                bi = max_frames;

                world.UploadChunks();
            }

            //drawing

            Raylib.BeginDrawing();

            //behind 3d

            Raylib.ClearBackground(world.fog_color);

            //3d

            Raylib.BeginMode3D(world.cam);

            if (hacky_fix_draw_glitch)
            {
                hacky_fix_draw_glitch = false;
                Raylib.DrawCube(Vector3.Zero, 1, 1, 1, Color.White);
            }

            world.Draw();

            Raylib.EndMode3D();

            //ui

            Entity? player = world.GetFocusEntity();
            if (player != null)
            {
                player.DrawGui();
            }

            Raylib.DrawFPS(10, 10);
            world.DrawDebugGUI();

            //Raylib.DrawTexture(TextureHandler.block_atlas.Texture, 0, 0, Color.Red);

            Raylib.EndDrawing();
        }

        lock (world)
        {
            world.Cleanup();
        }
        TextureHandler.Cleanup();

        Raylib.CloseWindow();
    }
}
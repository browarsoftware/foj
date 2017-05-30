/**
    Field Of Judgement, Copyright (C) 2014 Tomasz Hachaj, Ph.D
    The purpose of this application is to demnostrate how to use
    Gesture Descrption Language (GDL) as natural user interface for
    computer games and other systems.

    The sorce code is under MIT licence. 
    All dll-s are under licenses provided by theirs copyright holders.

    The MIT License (MIT)
    Copyright (c) 2014 Tomasz Hachaj, Ph.D
  
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimplexNoise;
using FieldOfJudgement.Enviroment;
using Jitter.LinearMath;

namespace FieldOfJudgement
{
    public class GameMap
    {
        //public float[][] heightmap = null;
        public float mapWidth = 0;
        public float mapHeight = 0;
        int bacgroundMeshSize = 20;
        public RenderableRigidBody Ground = null;
        public float[][] bacground = null;
        private float[][] map = null;
        private bool blocksOptimizatonMode = false;
        public int maxBlocksCount = int.MaxValue;

        public void BuildMap(FieldOfJudgementGame program)
        {
            byte[] noiseSeed;
            int necessaryBlockCount = 0;
            noiseSeed = new byte[512];        
            program.random.NextBytes(noiseSeed);
            Noise.perm = noiseSeed;

            float groundHeight = 1000;
            //float groundHeight = program.gsize;

            mapHeight = program.gsize;
            mapWidth = program.gsize;

            program.boundingBox = new FieldBorder[4];

            //float wallHeiht = 20f;
            Ground = new Ground(new JVector(0, 0, -groundHeight / 2.0f),
                new JVector(mapWidth, mapHeight, groundHeight));
            program.world.AddBody(Ground);


            FieldBorder fb = new FieldBorder(new JVector(-mapWidth, 0, 0), new JVector(mapWidth, mapHeight, groundHeight));
            program.world.AddBody(fb);
            program.boundingBox[0] = fb;

            fb = new FieldBorder(new JVector(mapWidth, 0, 0), new JVector(mapWidth, mapHeight, groundHeight));
            program.world.AddBody(fb);
            program.boundingBox[1] = fb;

            fb = new FieldBorder(new JVector(0, -mapWidth, 0), new JVector(mapWidth, mapHeight, groundHeight));
            program.world.AddBody(fb);
            program.boundingBox[2] = fb;

            fb = new FieldBorder(new JVector(0, mapWidth, 0), new JVector(mapWidth, mapHeight, groundHeight));
            program.world.AddBody(fb);
            program.boundingBox[3] = fb;
            ///////////////////////////////

            necessaryBlockCount = program.world.RigidBodies.Count + 2;
            
            bacground = new float[bacgroundMeshSize][];
            map = new float[(int)mapHeight][];
            for (int a = 0; a < mapHeight; a++)
                map[a] = new float[(int)mapWidth];

            float middlePointX = bacgroundMeshSize / 2f;
            float middlePointY = bacgroundMeshSize / 2f;
            float multiplayer = 4f * ((float)mapWidth / (float)bacgroundMeshSize);
            float border = (float)mapWidth / 2.0f / multiplayer;
            //bool found = false;

            for (int a = 0; a < bacground.Length; a++)
            {
                bacground[a] = new float[bacgroundMeshSize];
                for (int b = 0; b < bacground[a].Length; b++)
                {
                    bacground[a][b] = SimplexNoise.Noise.Generate((float)(a) / (float)bacground.Length, (float)(b) / (float)bacground[a].Length) * 20f;
                    /*if (middlePointX - a > -(float)gsize / 2.0f && middlePointX - a < (float)gsize / 2.0f)
                    {
                        if (middlePointY - b > -(float)gsize / 2.0f && middlePointY - b < (float)gsize / 2.0f)
                        {
                            //bacground[a][b] = -0.01f;
                        }
                    }*/
                    //found = false;
                    if (middlePointX - a > -border - 1 && middlePointX - a < border)
                    {
                        if (middlePointY - b > -border - 1 && middlePointY - b < border)
                        {
                            //found = true;
                            bacground[a][b] = -0.01f;
                        }
                    }
                    if (middlePointX - a > -border - 2 && middlePointX - a < border + 1)
                    {
                        if (middlePointY - b > -border - 2 && middlePointY - b < border + 1)
                        {
                            //found = true;
                            bacground[a][b] = -0.01f;
                        }
                    }
                }
            }
            ///////////////////////////////

            if (blocksOptimizatonMode)
            {
                for (int a = 1; a < mapWidth -1; a++)
                {
                    for (int b = 1; b < mapHeight -1; b++)
                    {
                        map[a][b] = 1;
                        program.world.AddBody(new StoneWall(new JVector(-mapHeight / 2.0f + a,
                                                                            -mapWidth / 2.0f + b,
                                                                            (float)0 + 0.5f),
                                        new JVector(1, 1, 1)));
                    }
                }
                return;
            }

            int sizex = 5;
            int sizey = 5;
            int sum = 0;
            bool endLoop = false;
            for (int d = 0; d < 3 && !endLoop; d++)
            {
                int offsetx = program.random.Next((int)mapWidth - sizex);
                int offsety = program.random.Next((int)mapHeight - sizey);

                //GameMap gm = new GameMap((int)sizex, (int)sizey, program.random.Next(1, 8));
                for (int a = 0; a < sizex && !endLoop; a++)
                    for (int b = 0; b < sizey && !endLoop; b++)
                    {
                        int value = (int)(SimplexNoise.Noise.Generate((float)(a) / (float)sizex, (float)(b) / (float)sizey) * Math.Min(sizex, sizey));
                        map[(int)(a + offsetx)][(int)(offsety + b)] += value;
                        //value = 0;
                        /*if (value > 0)
                        {
                            for (int c = 0; c < value; c++)
                            {
                                program.world.AddBody(new BrickWall(new JVector(-mapHeight / 2.0f + a + offsetx,
                                                                        -mapWidth / 2.0f + offsety + b,
                                                                        (float)c + 0.5f),
                                    new JVector(1, 1, 1)));
                            }
                        }*/

                    }
            }
            for (int a = 0; a < map.Length; a++)
                for (int b = 0; b < map[a].Length; b++)
                {
                    int value = (int)map[a][b];
                    int prevRandom = 0;

                    if (value > 0)
                    {
                        if (sum + value > maxBlocksCount - necessaryBlockCount)
                        {
                            value = maxBlocksCount - necessaryBlockCount - sum;
                            endLoop = true;
                        }
                        sum += value;
                        for (int c = 0; c < value; c++)
                        {
                            int rand = 0;
                            if (value == 0)
                            {
                                rand = program.random.Next(6);
                            }
                            else
                            {
                                rand = program.random.Next(6);
                                if (rand < prevRandom)
                                {
                                    rand = prevRandom;
                                }
                                prevRandom = rand;
                            }

                            if (rand == 0)
                            {
                                program.world.AddBody(new StoneWall(new JVector(-mapHeight / 2.0f + a,
                                                                                -mapWidth / 2.0f + b,
                                                                                (float)c + 0.5f),
                                            new JVector(1, 1, 1)));
                            }
                            if (rand == 1 || rand == 2 || rand == 3)
                            {
                                program.world.AddBody(new BrickWall(new JVector(-mapHeight / 2.0f + a,
                                                                                -mapWidth / 2.0f + b,
                                                                                (float)c + 0.5f),
                                            new JVector(1, 1, 1)));
                            }
                            if (rand == 4)
                            {
                                program.world.AddBody(new MedievalPatternWall(new JVector(-mapHeight / 2.0f + a,
                                                                                -mapWidth / 2.0f + b,
                                                                                (float)c + 0.5f),
                                            new JVector(1, 1, 1)));
                            }
                            if (rand == 5)
                            {
                                program.world.AddBody(new WoodenCrate(new JVector(-mapHeight / 2.0f + a,
                                                                                -mapWidth / 2.0f + b,
                                                                                (float)c + 0.5f),
                                            new JVector(1, 1, 1)));
                            }

                            /*if (rand == 2)
                            {
                                program.world.AddBody(new Pillar(new JVector(-mapHeight / 2.0f + a,
                                                                                -mapWidth / 2.0f + b,
                                                                                (float)c + 0.5f),
                                            new JVector(1, 1, 1)));*/
                        }
                    }
                }

            //tu dodać sprawdzanie
            if (sum >= maxBlocksCount - necessaryBlockCount)
            {
                return;
            }
            int max = 8;
            if (max + sum >= maxBlocksCount - necessaryBlockCount)
            {
                max = maxBlocksCount - necessaryBlockCount - sum;
            }
            
            //int pillarsCount = program.random.Next(1, max);
            //sum += pillarsCount;

            int reflectingCount = program.random.Next(1, max);
            sum += reflectingCount;
            while (reflectingCount > 0)
            {
                float x = program.random.Next((int)mapWidth - 4) + 2;
                float y = program.random.Next((int)mapHeight - 4) + 2;
                if (map[(int)x][(int)y] == 0)
                {
                    map[(int)x][(int)y] = 1;
                    reflectingCount--;
                    program.world.AddBody(new ReflectingWall(new JVector(-mapHeight / 2.0f + (float)x,
                                                                            -mapWidth / 2.0f + (float)y,
                                                                                (float)0.5f),
                                                                        new JVector(1, 1, 1)));
                }
            }
            int left = maxBlocksCount - necessaryBlockCount - sum;
            while (left > 0)
            {
                float x = program.random.Next((int)mapWidth - 4) + 2;
                float y = program.random.Next((int)mapHeight - 4) + 2;
                if (map[(int)x][(int)y] == 0)
                {
                    map[(int)x][(int)y] = 1;
                    //pillarsCount--;
                    int pillarHeight = program.random.Next(3,6);
                    if (left - pillarHeight < 0)
                        pillarHeight = left;
                    left -= pillarHeight;
                    for (int a = 0; a < pillarHeight; a++)
                    program.world.AddBody(new Pillar(new JVector(-mapHeight / 2.0f + (float)x,
                                                                            -mapWidth / 2.0f + (float)y,
                                                                                (float)0.5f + (float)a),
                                                                        new JVector(1, 1, 1)));
                }
            }
            int end = 0;
            end++;
        }
        public GameMap(bool blocksOptimizatonMode = false)
        {
            this.blocksOptimizatonMode = blocksOptimizatonMode;
            /*byte[] noiseSeed;
            noiseSeed = new byte[512];
            Random rand = new Random();
            rand.NextBytes(noiseSeed);
            

            Noise.perm = noiseSeed;

            heightmap = new float[height][];
            for (int a = 0; a < heightmap.Length; a++)
            {
                heightmap[a] = new float[width];
                for (int b = 0; b < heightmap[a].Length; b++)
                {
                    float cval = Noise.Generate((float)a / (float)heightmap.Length,
                        (float)b / (float)heightmap[a].Length) * difference;
                    heightmap[a][b] = cval;
                    //heightmap[a][b] = 0;
                }
            }*/
        }
    }
}

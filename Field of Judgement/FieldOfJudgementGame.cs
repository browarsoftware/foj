/*  
    Field Of Judgement, Copyright (C) 2014 Tomasz Hachaj, Ph.D
    The source code is under MIT license. 
    All dlls are under licenses provided by theirs copyright holders.

    This file contains also a code taken from:
    Open Dynamics Engine, Copyright (C) 2001,2002 Russell L. Smith.
    All rights reserved.  Email: russ@q12.org   Web: http://www.q12.org
    Open Dynamics Engine 4J, Copyright (C) 2007-2010 Tilmann Zäschke
    All rights reserved.  Email: ode4j@gmx.de   Web: http://www.ode4j.org
    CsODE, Copyright (C) 2011 Miguel Angel Guirado López.
    All rights reserved.  Email: bitiopiasite@gmail.com
   


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
using Jitter.LinearMath;
using Jitter.Collision;
using Jitter;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using System.Collections;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Threading;
using FieldOfJudgement.Magic;
using System.Drawing;
using FieldOfJudgement.Enviroment;
using SimplexNoise;
using System.Reflection;
using System.IO;
using FieldOfJudgement.Sound;
using System.Xml.Serialization;

namespace FieldOfJudgement
{
    public class FieldOfJudgementGame : DrawStuffOtk, IDisposable
    {
        /*
        http://jitter-physics.com/phpBB3/viewtopic.php?f=2&t=48
        http://jitter-physics.com/phpBB3/viewtopic.php?f=2&t=473
        http://jitter-physics.com/phpBB3/viewtopic.php?f=2&t=352
        http://jitter-physics.com/phpBB3/viewtopic.php?f=2&t=230
        http://jitter-physics.com/phpBB3/viewforum.php?f=2&start=125
        */
        public World world;
        private bool initFrame = true;

        private const string title = "Field of Judgement";
        public static double eps = 0.0001;
        public static float BulletRadius = 0.75f;
        public static float BulletMass = 10.0f;
        public static float BlockMass = 10;
        public CollisionSystemBrute collisionTester = new CollisionSystemBrute();
        public bool goreEffects = true;

        //Sensor has to be run in separate thraed, because GameWindow
        //catches all eavents from kinect device
        private void SensorThread()
        {
            sensorHost = new SensorHost(this);
        }

        protected override void OnUnload(EventArgs e)
        {
            if (sensorHost != null)
                sensorHost.Dispose();
            for (int a = 0; a < texture.Length; a++)
                if (a != (int)DS_TEXTURE_NUMBER.DS_NONE)
                {
                    if (texture[a] != null)
                        texture[a].finalize();
                }

            statsTexture.finalize();
            depthViewR.finalize();
            depthViewL.finalize();
            myTextWriter.finalize();
            for (int a = 0; a < players.Length; a++)
            {
                players[a].model.finalize();
            }
            Music.StopMusic(SoundType.IntoTheCube);
            Audio.Dispose();
            listnum = 0;
        }

        public void PlaySound(SoundType soundType)
        {
            Audio.PlaySound(soundType);
        }

        public FieldOfJudgementGame(int resolutionWidth, int resolutionHeight, bool blocksOptimizatonMode = false)
            : base(resolutionWidth, resolutionHeight)
        {
            this.blocksOptimizatonMode = blocksOptimizatonMode;
            depthViewR = new TextureLoader(this,0);
            depthViewL = new TextureLoader(this,1);
            statsTexture = new TextureLoader(this, 0);

            Assembly a = Assembly.GetExecutingAssembly();
            Stream readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.stats.png");
            statsTexture.LoadTextureFromBitmap(new Bitmap(readerStream));

            ForceField.textureHandler = DS_TEXTURE_NUMBER.DS_FORCEFIELD;
            MagicMissile.textureHandler = DS_TEXTURE_NUMBER.DS_MAGICMISSLE;
            Fireabll.textureHandler = DS_TEXTURE_NUMBER.DS_FIREBALL;


            Thread thread = new Thread(new ThreadStart(SensorThread));
            thread.Start();


            world = new World(new CollisionSystemPersistentSAP());
            world.Gravity = new JVector(0, 0, -10);
            keyboardHost = new KeyboardHost();

            dsSetSphereQuality(4);
            collisionTester.CollisionDetected += new CollisionDetectedHandler(CollisionDetected);

            this.VSync = OpenTK.VSyncMode.Off;
            this.Title = title;

            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);

            goreEffects = StartClass.gameConf.IsGoreEffects;
            IsVerticalSplit = StartClass.gameConf.IsVerticalSplit;


            gameMap = new GameMap(blocksOptimizatonMode);
            gameMap.maxBlocksCount = StartClass.gameConf.MaxBoxCount;
            gameMap.BuildMap(this);

            players = new Player[2];
            players[0] = new Player(world, new JVector(10, 10, 6), new JVector(1,0,1), this);
            players[0].shotKey = (int)OpenTK.Input.Key.Space;
            players[0].rightKey = (int)OpenTK.Input.Key.Right;
            players[0].leftKey = (int)OpenTK.Input.Key.Left;
            players[0].forwardKey = (int)OpenTK.Input.Key.Up;
            players[0].backrwardKey = (int)OpenTK.Input.Key.Down;
            players[0].jumpKey = (int)OpenTK.Input.Key.X;
            players[0].Id = 0;

            players[0].model = LoadModel();
            players[0].RespownPlayer(gameMap);

            players[1] = new Player(world, new JVector(5, 5, 6), new JVector(0, 1, 1), this);
            players[1].Id = 1;
            players[1].model = LoadModel();
            players[1].RespownPlayer(gameMap);
            players[0].shotKey = (int)OpenTK.Input.Key.LControl;

            GoFullscreen(StartClass.gameConf.IsFullScrean);

            myTextWriter = new MyTextWriter(this);
            redScreen = new RedScreen(this);
        }

        private void GoFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                this.WindowBorder = OpenTK.WindowBorder.Hidden;
                this.WindowState = OpenTK.WindowState.Fullscreen;
            }
            else
            {
                this.WindowBorder = OpenTK.WindowBorder.Resizable;
                this.WindowState = OpenTK.WindowState.Normal;
            }
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Up) keyboardHost.forward = 0;
            if (e.Key == OpenTK.Input.Key.Left)
                keyboardHost.left = 0;
            if (e.Key == OpenTK.Input.Key.Right)
                keyboardHost.right = 0;

            if (e.Key == OpenTK.Input.Key.PageUp)
                keyboardHost.up = 0;
            if (e.Key == OpenTK.Input.Key.PageDown)
                keyboardHost.down = 0;

            if (e.Key == OpenTK.Input.Key.Up) keyboardHost.forward = 0;
            if (e.Key == OpenTK.Input.Key.Down) keyboardHost.backrward = 0;

            for (int a = 0; a < players.Length; a++)
            {
                if ((int)e.Key == players[a].leftKey) players[a].left = 0;
                if ((int)e.Key == players[a].rightKey) players[a].right = 0;
                if ((int)e.Key == players[a].backrwardKey) players[a].backrward = 0;
                if ((int)e.Key == players[a].forwardKey) players[a].forward = 0;

                if ((int)e.Key == players[a].lookDownKey) players[a].lookDown = 0;
                if ((int)e.Key == players[a].lookUpKey) players[a].lookUp = 0;

                if ((int)e.Key == players[a].jumpKey) players[a].jump = 0;
            }
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Up) keyboardHost.forward = 1;
            if (e.Key == OpenTK.Input.Key.Down) keyboardHost.backrward = 1;
            if (e.Key == OpenTK.Input.Key.Left)
                keyboardHost.left = 1;
            if (e.Key == OpenTK.Input.Key.Right)
                keyboardHost.right = 1;

            if (e.Key == OpenTK.Input.Key.PageUp)
                keyboardHost.up = 1;
            if (e.Key == OpenTK.Input.Key.PageDown)
                keyboardHost.down = 1;
            JVector targetVelocity = JVector.Zero;

            for (int a = 0; a < players.Length; a++)
            {
                if ((int)e.Key == players[a].leftKey) players[a].left = 3;
                if ((int)e.Key == players[a].rightKey) players[a].right = 3;
                if ((int)e.Key == players[a].backrwardKey) players[a].backrward = 5;
                if ((int)e.Key == players[a].forwardKey) players[a].forward = 5;
                if ((int)e.Key == players[a].jumpKey) players[a].jump = 1;
                if ((int)e.Key == players[a].lookDownKey) players[a].lookDown = 1;
                if ((int)e.Key == players[a].lookUpKey) players[a].lookUp = 1;
                if ((int)e.Key == players[a].shotKey) ShootSphere(players[a], 0);
            }
            
            if (e.Key == OpenTK.Input.Key.Escape)
            {
                this.Close();
            }
        }

        public void ShootSphere(Player player, int type)
        {
            MagicBullet []mb = null;
            //type = 0;
            if (type == 0)
            {
                mb = SpellFactory.CreateFireball(player, GameTime);
                if (mb != null)
                    Audio.PlaySound(SoundType.Fireball);
                else
                    Audio.PlaySound(SoundType.NoMana);
            }
            if (type == 1)
            {
                mb = SpellFactory.CreateMagicMissle(player, GameTime);
                if (mb != null)
                    Audio.PlaySound(SoundType.MagicMissle);
                else
                    Audio.PlaySound(SoundType.NoMana);
            }
            if (type == 2)
            {
                mb = SpellFactory.CreateForceField(player, GameTime);
                if (mb != null)
                {
                    player.IsInsideForceField = true;
                    player.IsInsideForceFieldPosition = player.Position;
                    player.IsInsideForceFieldOrientation = player.Orientation;
                    Audio.PlaySound(SoundType.ForceField);
                }
                else
                    Audio.PlaySound(SoundType.NoMana);
            }

            if (mb != null)
                for (int a = 0; a < mb.Length; a++)
                    world.AddBody(mb[a]);
        }

        protected override void OnBeginRender(double elapsedTime, int playerId)
        {
            GameTime += elapsedTime;
            if (initFrame)
            {
                dsSetViewpoint(new float[] { 18, 10, 1.5f }, new float[] { 190, -10, 0 });
                initFrame = false;
            }

            RenderAll(playerId, elapsedTime);

            base.OnBeginRender(elapsedTime);
        }

        public ArrayList rbToRemove = new ArrayList();

        private void SpellProcessCollision(MagicBullet spell, Player player)
        {
            //can be hit by a spell only ones
            if (player.alradyHitMe.Contains(spell))
                return;
            if (spell.VanishAfterCollisionWithPlayer)
                rbToRemove.Add(spell);
            if (spell.DoNotHurtOwner && spell.OwnerId == player.Id)
                return;
            player.alradyHitMe.Add(spell);
            float damages = spell.Power - player.armour;
            if (damages > 0 && player.Health > 0)
                player.Health -= damages;

            if (player.Health <= 0 && !player.killed)
            {
                player.killed = true;
                if (player.Id != spell.OwnerId)
                    players[spell.OwnerId].Frags++;
                else
                    players[spell.OwnerId].Frags--;
                Audio.PlaySound(SoundType.WomanDeath);
            }
            else
                Audio.PlaySound(SoundType.WomanHit);
        }

        public void CollisionDetected(RigidBody body1, RigidBody body2, JVector point1, JVector point2, JVector normal, float penetration)
        {
            if (body1 is FieldBorder && body2 is FieldBorder)
                return;
            if (body1 is MagicBullet)
            {
                if (((MagicBullet)body1).VanishAfterCollisionWithAnyObject)
                    rbToRemove.Add(body1);
                for (int a = 0; a < players.Length; a++)
                {
                    if (body2 is Player)
                    {
                        SpellProcessCollision((MagicBullet)body1, (Player)body2);
                    }
                }
                if (body2 == boundingBox[0]
                    || body2 == boundingBox[1]
                    || body2 == boundingBox[2]
                    || body2 == boundingBox[3])
                {
                    rbToRemove.Add(body1);
                    Audio.PlaySound(SoundType.SpellHitBorder);
                    //return;
                }
            }
            if (body2 is MagicBullet)
            {
                if (((MagicBullet)body1).VanishAfterCollisionWithAnyObject)
                    rbToRemove.Add(body2);
                for (int a = 0; a < players.Length; a++)
                {
                    if (body1 is Player)
                    {
                        SpellProcessCollision((MagicBullet)body1, (Player)body2);
                    }
                }
                if (body1 == boundingBox[0]
                    || body1 == boundingBox[1]
                    || body1 == boundingBox[2]
                    || body1 == boundingBox[3])
                {
                    rbToRemove.Add(body2);
                    Audio.PlaySound(SoundType.SpellHitBorder);
                    //return;
                }
            }
            
            for (int a = 0; a < boundingBox.Length; a++)
            {
                if (body1 == boundingBox[a] || body2 == boundingBox[a])
                {
                    boundingBox[a].collision = true;
                    return;
                }
            }          
        }
        public double GameTime = 0;
        float specialIterator = 0;

        public object RigidBodiesLock = new Object();
        bool first = true;
        double specialIteratorElapsedTime = 0;

        private void RenderAll(int playerId, double elapsedTime)
        {
            specialIteratorElapsedTime += elapsedTime;
            if (specialIteratorElapsedTime > 0.01)
            {
                specialIterator++;
                specialIteratorElapsedTime = 0;
            }
            if (!blocksOptimizatonMode)
                rbToRemove.Clear();

            foreach (RenderableRigidBody body in world.RigidBodies)
            {
                if (body.Tag is bool) continue;

                collisionTester.Detect(body, boundingBox[0]);
                collisionTester.Detect(body, boundingBox[1]);
                collisionTester.Detect(body, boundingBox[2]);
                collisionTester.Detect(body, boundingBox[3]);
                if (body is Player)
                {
                    //nie rysować samego siebie :-)
                    if (players[playerId] != body)
                    {
                        for (int a = 0; a < players.Length; a++)
                        if (players[a] == body)
                        {
                            if (players[a].Health > 0)
                            {
                                dsSetTexture(DS_TEXTURE_NUMBER.DS_GRASS);
                                players[a].Render(elapsedTime, this, specialIterator);
                            }
                            else
                            {
                                rbToRemove.Add(body);
                            }
                        }
                    }
                }
                else
                {
                    if (body.GetTexture() != DS_TEXTURE_NUMBER.DS_NONE)
                        dsSetTexture(body.GetTexture());
                    body.Render(elapsedTime, this, specialIterator);
                }

            }

            while (rbToRemove.Count > 0)
            {
                if (rbToRemove[0] is MagicBullet)
                {
                    MagicBullet mb = (MagicBullet)rbToRemove[0];
                    if (!mb.spellEnded)
                    {
                        mb.EndSpell(players[mb.OwnerId]);
                        mb.spellEnded = true;
                    }
                }
                world.RemoveBody((RigidBody)rbToRemove[0]);
                if (rbToRemove[0] is Player)
                {
                    ((Player)rbToRemove[0]).RespownPlayer(gameMap);
                }
                if (players[0].alradyHitMe.Contains(rbToRemove[0]))
                {
                    players[0].alradyHitMe.Remove(rbToRemove[0]);
                }
                if (players[1].alradyHitMe.Contains(rbToRemove[0]))
                {
                    players[1].alradyHitMe.Remove(rbToRemove[0]);
                }
                rbToRemove.RemoveAt(0);
            }
        }

        float accTime = 0.0f;
        double avgStatistic = 0;
        int avgToXSec = 10;
        int sumToAvg = 0;
        int howManyStep = 0;
        bool startOpt = true;

        bool removeX(int X)
        {
            foreach (RigidBody body in world.RigidBodies)
            {
                if (body is StoneWall && !rbToRemove.Contains(body))
                {
                    X--;
                    rbToRemove.Add(body);
                    if (X == 0)
                        break;
                }
            }
            if (X == 0)
                return true;
            else
                return false;
        }
        bool addX(int X)
        {
            for (int a = 0; a < X; a++)
            {
                int xx = random.Next(1, (int)(gameMap.mapHeight - 1 + 0.01));
                int yy = random.Next(1, (int)(gameMap.mapHeight - 1 + 0.01));
                world.AddBody(new StoneWall(new JVector(-gameMap.mapHeight / 2.0f + xx,
                                                                            -gameMap.mapWidth / 2.0f + yy,
                                                                            (float)0 + 0.5f),
                                        new JVector(1, 1, 1)));
            }
            return true;
        }

        protected override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            accTime += 1.0f / (float)RenderFrequency;

            if (accTime > 1.0f)
            {
                this.Title = title + " " + RenderFrequency.ToString("##.#") + " fps";
                accTime = 0.0f;
                if (blocksOptimizatonMode)
                {
                    if (startOpt)
                    {
                        howManyStep = world.RigidBodies.Count / 2;
                        startOpt = true;
                    }
                    rbToRemove.Clear();
                    this.Title += " block count " + world.RigidBodies.Count;
                    if (sumToAvg < avgToXSec)
                    {
                        avgStatistic += RenderFrequency;
                        sumToAvg++;
                    }
                    else
                    {
                        avgStatistic /= avgToXSec;
                        sumToAvg = 1;
                        avgStatistic = RenderFrequency; 
                        bool found = false;
                        if (avgStatistic < 150 || avgStatistic > 180)
                        {
                            if (avgStatistic < 150)
                            {
                                found = removeX(howManyStep);
                            }
                            else
                            {
                                found = addX(howManyStep);
                            }
                            howManyStep /= 2;
                            }
                        if (!found)
                        {
                            blocksOptimizatonMode = false;
                            GameConf conf = new GameConf();
                            conf.MaxBoxCount = (int)(0.8 * world.RigidBodies.Count);
                            conf.IsFullScrean = StartClass.gameConf.IsFullScrean;
                            conf.IsGoreEffects = StartClass.gameConf.IsGoreEffects;
                            conf.IsVerticalSplit = StartClass.gameConf.IsVerticalSplit;
                            conf.IsAudioEnabled = StartClass.gameConf.IsAudioEnabled;

                            GameConf.SaveToFile(conf, "conf.xml");
                            this.Exit();
                        }
                    }
                }
            }

            /*if (IsStartScreen)
            {
                return;
            }*/

            float step = 1.0f / (float)RenderFrequency;
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //TUTUTUTU
            if (step > 1.0f / 140.0f) step = 1.0f / 140.0f;

            
            //casting spells
            for (int a = 0; a < players.Length; a++)
                if (players[a].castSpell >= 0)
                {
                    ShootSphere(players[a], players[a].castSpell);
                    players[a].castSpell = -1;
                }
            //TUTUTUTUTUTUTU
            if (first)
            {
                foreach (RigidBody body in world.RigidBodies)
                {
                    if (!body.IsStatic)
                    {
                        body.IsActive = true;
                        
                        //body.AngularVelocity = JVector.Zero;
                        //body.LinearVelocity = JVector.Zero;
                    }
                }
                float oldMV = world.ContactSettings.MinimumVelocity;
                //float oldAP = world.ContactSettings.MinimumVelocity;

                world.ContactSettings.MinimumVelocity = 0;
                //world.ContactSettings.AllowedPenetration = 1;
                //world.SetInactivityThreshold(0, 0, 0);
                world.Step(0.0001f, false);

                foreach (RigidBody body in world.RigidBodies)
                {
                    if (!body.IsStatic)
                    {
                        body.AngularVelocity = JVector.Zero;
                        body.LinearVelocity = JVector.Zero;
                    }
                }
                world.ContactSettings.MinimumVelocity = oldMV;
                //world.ContactSettings.AllowedPenetration = oldAP;
                first = false;
            }
            else
            {
                //lock (RigidBodiesLock)
                {

                    world.Step(step, true);
                    for (int a = 0; a < players.Length; a++)
                    {
                        if (players[a].IsInsideForceField)
                        {
                            players[a].Position = players[a].IsInsideForceFieldPosition;
                            players[a].Orientation = players[a].IsInsideForceFieldOrientation;
                            players[a].LinearVelocity = JVector.Zero;
                            players[a].AngularVelocity = JVector.Zero;
                            players[a].IsActive = false;
                        }
                    }
                }
            }

            base.OnUpdateFrame(e);
        }



















































    }
}

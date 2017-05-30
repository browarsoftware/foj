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
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Jitter.LinearMath;
using FieldOfJudgement.Magic;
using Jitter.Dynamics;
using FieldOfJudgement.Enviroment;
using FieldOfJudgement.Sound;

namespace FieldOfJudgement
{
    /**
     * One of two main classes of games, inherit from OpenTK's GameWindow.
     * Remember, that GameWindow class might disturb message pumping of COM components! ;-)
     * This class has most of the rendering methods.
     */
    public class DrawStuffOtk : GameWindow
    {
        public bool blocksOptimizatonMode = true;

        public MyTextWriter myTextWriter = null;
        public RedScreen redScreen = null;
        public bool IsVerticalSplit = true;
        public KeyboardHost keyboardHost;
        public Player[] players = null;
        Color clearColor = Color.FromArgb(100, 100, 120);

        MouseState current, previous;
        private const double M_PI = Math.PI;

        private const double DEG_TO_RAD = Math.PI / 180.0;
        // light vector. LIGHTZ is implicitly 1
        private const float LIGHTX = 1.0f;
        private const float LIGHTY = 0.4f;

        // ground color for when there's no texture
        private const float GROUND_R = 0.5f;
        private const float GROUND_G = 0.5f;
        private const float GROUND_B = 0.3f;

        // ground and sky
        private const float SHADOW_INTENSITY = 0.65f;

        private const float ground_scale = 1.0f / 1.0f;	// ground texture scale (1/size)
        private const float ground_ofsx = 0.5f;		// offset of ground texture
        private const float ground_ofsy = 0.5f;
        private const float sky_scale = 1.0f / 8.0f;	// sky texture scale (1/size)
        private const float sky_height = 1.0f;		// sky height above viewpoint

        protected double fieldOfView = 90.0f;
        protected double nearClipDistance = 0.1f;
        protected double farClipDistance = 5000.0f;

        public Random random = new Random();
        private float offset = 0.0f;

        // the current state:
        //    0 = uninitialized
        //    1 = dsSimulationLoop() called
        //    2 = dsDrawFrame() called
        private int current_state = 0;

        // textures and shadows
        public bool use_textures = true;		// 1 if textures to be drawn
        public bool use_shadows = true;		// 1 if shadows to be drawn
        private Texture sky_texture = null;
        protected Texture ground_texture = null;
        public Texture[] texture = null;

        public float[] color = { 0, 0, 0, 0 };	// current r,g,b,alpha color
        private DS_TEXTURE_NUMBER tnum = DS_TEXTURE_NUMBER.DS_NONE;

        private float[] s_params_SDM = null;
        private float[] t_params_SDM = null;
        private float[] q_params_SDM;
        private float[] r_params_SDM;

        private float[] light_ambient2;
        private float[] light_diffuse2;
        private float[] light_specular2;

        private float[] light_position = new float[] { LIGHTX, LIGHTY, 1.0f, 0.0f };
        private float[] light_ambient = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
        private float[] light_diffuse = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] light_specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };

        private readonly float[] s_params_SSDM;
        private readonly float[] t_params_SSDM;
        

        // current camera position and orientation
        private float[] view_xyz = new float[3];	// position x,y,z
        private float[] view_hpr = new float[3];	// heading, pitch, roll (degrees)

        #region Constructores

        #region public GameBase()

        /// <summary>Constructs a new GameWindow with sensible default attributes.</summary>
        public DrawStuffOtk()
            : this(640, 480, GraphicsMode.Default, "Field of Judgement", 0, DisplayDevice.Default) { }

        #endregion

        #region public GameBase(int width, int height)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param Name="width">The width of the GameWindow in pixels.</param>
        /// <param Name="height">The height of the GameWindow in pixels.</param>
        public DrawStuffOtk(int width, int height)
            : this(width, height, GraphicsMode.Default, "Field of Judgement", 0, DisplayDevice.Default) { }

        #endregion

        #region public GameBase(int width, int height, GraphicsMode mode)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param Name="width">The width of the GameWindow in pixels.</param>
        /// <param Name="height">The height of the GameWindow in pixels.</param>
        /// <param Name="mode">The OpenTK.Graphic.GraphicsMode of the GameWindow.</param>
        public DrawStuffOtk(int width, int height, GraphicsMode mode)
            : this(width, height, mode, "Field of Judgement", 0, DisplayDevice.Default) { }

        #endregion

        #region public GameBase(int width, int height, GraphicsMode mode, string title)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param Name="width">The width of the GameWindow in pixels.</param>
        /// <param Name="height">The height of the GameWindow in pixels.</param>
        /// <param Name="mode">The OpenTK.Graphic.GraphicsMode of the GameWindow.</param>
        /// <param Name="title">The title of the GameWindow.</param>
        public DrawStuffOtk(int width, int height, GraphicsMode mode, string title)
            : this(width, height, mode, title, 0, DisplayDevice.Default) { }

        #endregion

        #region public GameBase(int width, int height, GraphicsMode mode, string title, GameWindowFlags options)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param Name="width">The width of the GameWindow in pixels.</param>
        /// <param Name="height">The height of the GameWindow in pixels.</param>
        /// <param Name="mode">The OpenTK.Graphic.GraphicsMode of the GameWindow.</param>
        /// <param Name="title">The title of the GameWindow.</param>
        /// <param Name="options">GameWindow options regarding window appearance and behavior.</param>
        public DrawStuffOtk(int width, int height, GraphicsMode mode, string title, GameWindowFlags options)
            : this(width, height, mode, title, options, DisplayDevice.Default) { }

        #endregion

        #region public GameBase(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param Name="width">The width of the GameWindow in pixels.</param>
        /// <param Name="height">The height of the GameWindow in pixels.</param>
        /// <param Name="mode">The OpenTK.Graphic.GraphicsMode of the GameWindow.</param>
        /// <param Name="title">The title of the GameWindow.</param>
        /// <param Name="options">GameWindow options regarding window appearance and behavior.</param>
        /// <param Name="device">The OpenTK.Graphic.DisplayDevice to construct the GameWindow in.</param>
        public DrawStuffOtk(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)
            : this(width, height, mode, title, options, device, 1, 0, GraphicsContextFlags.Default)
        { }

        #endregion

        #region public GameBase(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags)

        /// <summary>Constructs a new GameWindow with the specified attributes.</summary>
        /// <param Name="width">The width of the GameWindow in pixels.</param>
        /// <param Name="height">The height of the GameWindow in pixels.</param>
        /// <param Name="mode">The OpenTK.Graphic.GraphicsMode of the GameWindow.</param>
        /// <param Name="title">The title of the GameWindow.</param>
        /// <param Name="options">GameWindow options regarding window appearance and behavior.</param>
        /// <param Name="device">The OpenTK.Graphic.DisplayDevice to construct the GameWindow in.</param>
        /// <param Name="major">The major version for the OpenGL GraphicsContext.</param>
        /// <param Name="minor">The minor version for the OpenGL GraphicsContext.</param>
        /// <param Name="flags">The GraphicsContextFlags version for the OpenGL GraphicsContext.</param>
        public DrawStuffOtk(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device,
            int major, int minor, GraphicsContextFlags flags)
            : this(width, height, mode, title, options, device, major, minor, flags, null)
        { }

        #endregion

        public DrawStuffOtk(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device,
                        int major, int minor, GraphicsContextFlags flags, IGraphicsContext sharedContext)
            : base(width, height, mode, title, options, device, major, minor, flags, sharedContext)
        {

            //s_params_SDM = new float[] { 1f, 0.0f, 0.0f, 0 };
            //t_params_SDM = new float[] { 0f, 1f, 0.0f, 0f };
            //q_params_SDM = new float[] { 0.0f, 0.0f, 1.0f, 0 };
            //r_params_SDM = new float[] { 0f, 0.0f, 0.0f, 1f };
            s_params_SDM = new float[] { 1.0f, 1.0f, 0.0f, 1 };
            t_params_SDM = new float[] { 0.817f, -0.817f, 0.817f, 1 };
            //t_params_SDM = new float[] { 0f, 1f, 1f, 1 };

            s_params_SSDM = new float[] { ground_scale, 0, 0, ground_ofsx };
            t_params_SSDM = new float[] { 0, ground_scale, 0, ground_ofsy };

            Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);

            keyboardHost = new KeyboardHost();

            offset = (float)random.NextDouble();
        }
        #endregion Constructores

        bool buttonDownLeft = false;
        bool buttonDownMiddle = false;
        bool buttonDownRight = false;

        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (e.Button == OpenTK.Input.MouseButton.Left)
                buttonDownLeft = true;
            else if (e.Button == OpenTK.Input.MouseButton.Middle)
                buttonDownMiddle = true;
            else if (e.Button == OpenTK.Input.MouseButton.Right)
                buttonDownRight = true;
        }
        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (e.Button == OpenTK.Input.MouseButton.Left)
                buttonDownLeft = false;
            else if (e.Button == OpenTK.Input.MouseButton.Middle)
                buttonDownMiddle = false;
            else if (e.Button == OpenTK.Input.MouseButton.Right)
                buttonDownRight = false;
        }

        protected void gluPerspective(double fovy, double aspect, double zNear, double zFar)
        {
            double xmin, xmax, ymin, ymax;

            ymax = zNear * Math.Tan(fovy * Math.PI / 360.0);
            ymin = -ymax;

            xmin = ymin * aspect;
            xmax = ymax * aspect;

            GL.Frustum(xmin, xmax, ymin, ymax, zNear, zFar);
        }

        #region OnXXX of GameWindow

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            current_state = 1;
            initMotionModel();

            Stream readerStream;
            texture = new Texture[Enum.GetNames(typeof(DS_TEXTURE_NUMBER)).Length + 1];
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                String[] nombres = a.GetManifestResourceNames(); 

                //http://mertol.deviantart.com/gallery/
                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.lava.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_LAVA] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.tree.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_TREE] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.medieval.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MEDIEVAL] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.medieval2.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MEDIEVAL2] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.medieval3.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MEDIEVAL3] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.medieval4.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MEDIEVAL4] = new Texture(readerStream);


                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.white.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_WHITE] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.wood.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_WOOD] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.sky.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_SKY] = new Texture(readerStream, true);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.checkered.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_CHECKERED] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.grass.jpeg");
                texture[(int)DS_TEXTURE_NUMBER.DS_GRASS] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.reflectingwall.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_REFLECTINGWALL] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.flowers.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_FLOWERS] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.mossfloor.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MOSSFLOOR] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.mossfloor2.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MOSSFLOOR2] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.brick.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_BRICKWALL] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.stonewall.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_STONEWALL] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.forcefield.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_FORCEFIELD] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.fireball.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_FIREBALL] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.fireball_core.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_FIREBALL_CORE] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.magicmissle.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_MAGICMISSLE] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.flesh.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_FLESH] = new Texture(readerStream);

                readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.start_screen.png");
                texture[(int)DS_TEXTURE_NUMBER.DS_STARTSCREEN] = new Texture(readerStream);
            }
            catch
            {
                throw new Exception("Error accessing resources!");
            }
            ground_texture = texture[(int)DS_TEXTURE_NUMBER.DS_MOSSFLOOR];
            sky_texture = texture[(int)DS_TEXTURE_NUMBER.DS_SKY];

            Audio.LoadSounds();
            Music.StartMusic(SoundType.SavageGround);
        }

        public PlayerModel LoadModel()
        {
            /*
             * String path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            String[] dir = Directory.GetDirectories(path + "\\Models");
            PlayerModels = new PlayerModel[dir.Length];
            for (int a = 0; a < dir.Length; a++)
            {
                PlayerModels[a] = new PlayerModel();
                PlayerModels[a].chest.LoadTextures("chest", dir[a], texture[(int)DS_TEXTURE_NUMBER.DS_CHECKERED]);
            }
             */
            PlayerModel pm = new PlayerModel();
            pm.chest.LoadTextures("chest", 
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.belly.LoadTextures("belly",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.hips.LoadTextures("hips",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");
            //RIGHT HAND
            pm.rightShoulder.LoadTextures("rightShoulder",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.rightElbow.LoadTextures("rightElbow",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.rightHand.LoadTextures("rightHand",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");
            //LEFT HAND
            pm.leftShoulder.LoadTextures("leftShoulder",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.leftElbow.LoadTextures("leftElbow",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.leftHand.LoadTextures("leftHand",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");
            //RIGHT LEG
            pm.rightHip.LoadTextures("rightHip",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.rightKnee.LoadTextures("rightKnee",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.rightFoot.LoadTextures("rightFoot",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");
            //LEFT LEG
            pm.leftHip.LoadTextures("leftHip",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.leftKnee.LoadTextures("leftKnee",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.leftFoot.LoadTextures("leftFoot",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            //HEAD ETC.
            pm.head.LoadTextures("head",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.neck.LoadTextures("neck",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");
                 //FieldOfJudgement.Resources.Models.checkered.png
            pm.hat.LoadTextures("hat",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");

            pm.hatTop.LoadTextures("hatTop",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.Sorceress.hatTop.png");

            pm.auraPosition.LoadTextures("aura",
                "FieldOfJudgement.Resources.Models.Sorceress",
                "FieldOfJudgement.Resources.Models.checkered.png");
            return pm;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (Width != 0 && Height != 0)
            {
                double aspectRatio = Width / (double)Height;
                //if (IsSplitScreen)
                {
                    if (IsVerticalSplit)
                    {
                        GL.Viewport(0, 0, Width, Height / 2);
                        //GL.Viewport(0, 0, Width, Height);
                        aspectRatio = (Width / 2) / (double)Height;
                    }
                    else
                    {
                        GL.Viewport(0, 0, Width, Height / 2);
                        //GL.Viewport(0, 0, Width, Height);
                        aspectRatio = Width / (double)(Height / 2);
                    }
                }
                /*else
                {
                    GL.Viewport(0, 0, Width, Height);
                }*/
                
                // Set projection
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                gluPerspective(fieldOfView, aspectRatio, nearClipDistance, farClipDistance);
            }
        }

        //MOJE
        public TextureLoader depthViewR = null;
        public TextureLoader depthViewL = null;
        public TextureLoader statsTexture = null;
        public static SensorHost sensorHost = null;
        
        protected void RenderStartScreen()
        {

        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            updateMouseState();
            move(e.Time);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                      
            dsDrawFrame(Width, Height, false, 0);
            OnBeginRender(e.Time, 0);
            if (players[0].redScreenDamages > 0)
                redScreen.Draw(players[0].redScreenDamages, IsVerticalSplit, true);
               
            OnEndRender();
            dsDrawFrame(Width, Height, false, 1);
            OnBeginRender(e.Time, 1);
            if (players[1].redScreenDamages > 0)
                redScreen.Draw(players[1].redScreenDamages, IsVerticalSplit, false);
            OnEndRender();

            float scaleParameter = 0.05f;
            int newWidth = (int)((double)myTextWriter.textureWidth * (double)scaleParameter);
            int newHeight = (int)((double)newWidth / (double)statsTexture.originalWidth * (double)statsTexture.originalHeight);

            if (IsVerticalSplit)
            {
                if (players[0].dirtyRecord || players[1].dirtyRecord)
                {
                    players[0].dirtyRecord = false;
                    players[1].dirtyRecord = false;
                    myTextWriter.Clear();
                    myTextWriter.AddLine("" + players[0].Frags, new PointF(newWidth, +(newHeight / 15.0f)), Brushes.White);
                    myTextWriter.AddLine("" + players[0].Health + "/" + players[0].maxHealth, new PointF(newWidth, newHeight / 3.0f + (newHeight / 15.0f)), Brushes.White);
                    myTextWriter.AddLine("" + players[0].Mana + "/" + players[0].maxMana, new PointF(newWidth, 2.0f * newHeight / 3.0f + (newHeight / 15.0f)), Brushes.White);

                    newWidth += myTextWriter.textureWidth / 2;
                    myTextWriter.AddLine("" + players[1].Frags, new PointF(newWidth, +(newHeight / 15.0f)), Brushes.White);
                    myTextWriter.AddLine("" + players[1].Health + "/" + players[0].maxHealth, new PointF(newWidth, newHeight / 3.0f + (newHeight / 15.0f)), Brushes.White);
                    myTextWriter.AddLine("" + players[1].Mana + "/" + players[0].maxMana, new PointF(newWidth, 2.0f * newHeight / 3.0f + (newHeight / 15.0f)), Brushes.White);
                }
                myTextWriter.Draw();

                if (sensorHost != null)
                {
                    if (sensorHost.Sensor != null)
                    {
                        depthViewR.LoadTexture();
                        depthViewL.LoadTexture();
                        depthViewR.Draw();
                        depthViewL.Draw();
                    }
                }
                statsTexture.DrawImageWithTransparency(scaleParameter, 0, 0);
                statsTexture.DrawImageWithTransparency(scaleParameter, ClientSize.Width / 2.0f , 0);
            }
            else
            {
                if (players[0].dirtyRecord || players[1].dirtyRecord)
                {
                    players[0].dirtyRecord = false;
                    players[1].dirtyRecord = false;
                    myTextWriter.Clear();
                    myTextWriter.AddLine("" + players[0].Frags, new PointF(newWidth, +(newHeight / 15.0f)), Brushes.White);
                    myTextWriter.AddLine("" + players[0].Health + "/" + players[0].maxHealth, new PointF(newWidth, newHeight / 3.0f + (newHeight / 15.0f)), Brushes.White);
                    myTextWriter.AddLine("" + players[0].Mana + "/" + players[0].maxMana, new PointF(newWidth, 2.0f * newHeight / 3.0f + (newHeight / 15.0f)), Brushes.White);
                    
                    int pHeight = myTextWriter.textureHeight / 2;
                    myTextWriter.AddLine("" + players[1].Frags, new PointF(newWidth, +(newHeight / 15.0f) + pHeight) , Brushes.White);
                    myTextWriter.AddLine("" + players[1].Health + "/" + players[0].maxHealth, new PointF(newWidth, newHeight / 3.0f + (newHeight / 15.0f) + pHeight), Brushes.White);
                    myTextWriter.AddLine("" + players[1].Mana + "/" + players[0].maxMana, new PointF(newWidth, 2.0f * newHeight / 3.0f + (newHeight / 15.0f) + pHeight), Brushes.White);
                }
                myTextWriter.Draw();

                if (sensorHost != null)
                {
                    if (sensorHost.Sensor != null)
                    {
                        depthViewR.LoadTexture();
                        depthViewL.LoadTexture();
                        depthViewR.DrawHorizontal();
                        depthViewL.DrawHorizontal();
                    }
                }
                statsTexture.DrawImageWithTransparency(scaleParameter, 0, 0);
                statsTexture.DrawImageWithTransparency(scaleParameter, 0, ClientSize.Height / 2.0f);
            }
            SwapBuffers();
        }
        /// <summary>
        /// Virtual pura
        /// </summary>
        protected virtual void OnBeginRender(double elapsedTime) { }

        protected virtual void OnBeginRender(double elapsedTime, int playerId) { }
        /// <summary>
        /// Virtual pura
        /// </summary>
        protected virtual void OnEndRender() { }

        #endregion OnXXX of GameWindow

        private void normalizeVector3(float[] v)//[3])
        {
            float len = v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
            if (len <= 0.0f)
            {
                v[0] = 1;
                v[1] = 0;
                v[2] = 0;
            }
            else
            {
                len = 1.0f / (float)Math.Sqrt(len);
                v[0] *= len;
                v[1] *= len;
                v[2] *= len;
            }
        }
        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);
        }
        private void updateMouseState()
        {
            int dx = 0;
            int dy = 0;
            int dw = 0;

            current = OpenTK.Input.Mouse.GetState();
            if (current != previous)
            {
                // Mouse state has changed
                dx = current.X - previous.X;
                dy = current.Y - previous.Y;
                dw = current.Wheel - previous.Wheel;
            }
            previous = current;


            // get out if no movement
            if (dx == dy && dx == 0 && dw == 0)
            {
                return;
            }

            //LWJGL: 0=left 1=right 2=middle
            //GL: 0=left 1=middle 2=right

            int mode = 0;
            if (buttonDownLeft)
                mode |= 1;
            else if (buttonDownMiddle)
                mode |= 2;
            else if (buttonDownRight)
                mode |= 4;
            if (mode != 0)
            {
                //LWJGL has inverted dy wrt C++/GL
                dsMotion(mode, dx, dy);
            }
        }
        protected void dsSetClearColor(byte red, byte green, byte blue)
        {
            clearColor = Color.FromArgb(red, green, blue);
        }
        void dsMotion(int mode, int deltax, int deltay)
        {
            float side = 0.01f * (float)deltax;
            float fwd = (mode == 4) ? (0.01f * (float)deltay) : 0.0f;
            float s = (float)Math.Sin(view_hpr[0] * DEG_TO_RAD);
            float c = (float)Math.Cos(view_hpr[0] * DEG_TO_RAD);

            if (mode == 1)
            {
                view_hpr[0] += (float)(deltax) * -0.5f;
                view_hpr[1] += (float)(deltay) * -0.5f;
            }
            else
            {
                view_xyz[0] += -s * side + c * fwd;
                view_xyz[1] += c * side + s * fwd;
                if (mode == 2 || mode == 5)
                    view_xyz[2] += 0.01f * (float)(deltay);
            }
            wrapCameraAngles();
        }

        
        private void move(double time)
        {
            for (int a = 0; a < players.Length; a++)
            {
                players[a].UpdateCamera(time);
                players[a].UpdateKeyboardInput();
            }         
        }


        private void wrapCameraAngles()
        {
            for (int i = 0; i < 3; i++)
            {
                while (view_hpr[i] > 180) view_hpr[i] -= 360;
                while (view_hpr[i] < -180) view_hpr[i] += 360;
            }
        }

        void dsDrawFrame(int width, int height, bool pause, int playerId)
        {
            if (current_state < 1)
                throw new Exception("internal error");
            current_state = 2;
            
            // setup stuff
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);

            GL.Disable(EnableCap.TextureGenQ);
            GL.Disable(EnableCap.TextureGenR);


            GL.ShadeModel(ShadingModel.Flat);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            
            // setup viewport
            if (playerId == 0)
            {
                if (IsVerticalSplit)
                {
                    GL.Viewport(0, 0, width / 2, height);
                }
                else
                {
                    GL.Viewport(0, height / 2, width, height / 2);
                }
            }
            else
            {
                if (IsVerticalSplit)
                {
                    GL.Viewport(width / 2, 0, width / 2, height);
                }
                else
                {
                    GL.Viewport(0, 0, width, height / 2);
                }
            }
            view_hpr = players[playerId].view_hpr;
            view_xyz = players[playerId].view_xyz;
            view_xyz[2] += players[playerId].HeadPosition;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float vnear = 0.1f;
            float vfar = 1000.0f;
            //float k = 0.8f;     // view scale, 1 = +/- 45 degrees
            //float k = 1.047f;     // view scale, 1 = +/- 60 degrees
            //float k = 1.57f; // view scale, 1 = +/- 90 degrees
            float k = (float)Math.Tan(fieldOfView * Math.PI / 360.0);
            //float k = 1.745f;
            if (width >= height)
            {
                float k2 = (float)height / (float)width;
                if (IsVerticalSplit)
                    k2 = (float)height / (float)(width/2.0);
                else
                    k2 = (float)(height / 2) / (float)width;
                GL.Frustum(-vnear * k, vnear * k, -vnear * k * k2, vnear * k * k2, vnear, vfar);
            }
            else
            {
                float k2 = (float)width / (float)height;
                if (IsVerticalSplit)
                {
                    k2 = (float)(width / 2.0) / (float)height;
                }
                else
                {
                    k2 = (float)width / (float)(height / 2);
                }
                GL.Frustum(-vnear * k * k2, vnear * k * k2, -vnear * k, vnear * k, vnear, vfar);
            }

            // setup lights. it makes a difference whether this is done in the
            GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, light_specular);
            GL.Color3(1.0f, 1.0f, 1.0f);

            // clear the window
            GL.ClearColor(clearColor.R / 255f, clearColor.G / 255f, clearColor.B / 255f, 0);

            // snapshot camera position (in MS Windows it is changed by the GUI thread)
            float[] view2_xyz = (float[])view_xyz.Clone();
            float[] view2_hpr = (float[])view_hpr.Clone();

            // go to GL_MODELVIEW matrix mode and set the camera
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            setCamera(view2_xyz[0], view2_xyz[1], view2_xyz[2],
                    view2_hpr[0], view2_hpr[1], view2_hpr[2]);

            // set the light position (for some reason we have to do this in model view.
            GL.Light(LightName.Light0, LightParameter.Position, light_position);

            // draw the background (ground, sky etc)
            drawSky(view2_xyz);
            drawGround();
            RenderHeightmap(gameMap.bacground);

            // draw the little markers on the ground

            // leave openGL in a known state - flat shaded white, no textures
            GL.Enable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);
            GL.ShadeModel(ShadingModel.Flat);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Color3(1.0f, 1.0f, 1.0f);
            setColor(1.0f, 1.0f, 1.0f, 1.0f);

            // draw the rest of the objects. set drawing state first.
            color[0] = 1.0f;
            color[1] = 1.0f;
            color[2] = 1.0f;
            color[3] = 1.0f;
            tnum = DS_TEXTURE_NUMBER.DS_NONE;
        }

        
        private void RenderHeightmap(float[][] heightmap)
        {
            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogMode, (int)FogMode.Linear); //FogStart and FogEnd params apply to the Linear fog mode

            GL.Fog(FogParameter.FogColor, new[] { clearColor.R / 255f, clearColor.G / 255f, clearColor.B / 255f });
            GL.Fog(FogParameter.FogStart, 1.0f); //start fog at 30% of zfar
            GL.Fog(FogParameter.FogEnd, gsize); //moving to full fog a quarter chunk before the zfar lets the distance transition out smoother

            float middlePointX = (float)((float)heightmap.Length / 2f);
            float middlePointY = (float)((float)heightmap[0].Length / 2f);
            float multiplayer = 4f * ((float)gsize / (float)heightmap.Length);
            int RenderSteps = 1;
            
            if (use_textures)
            {
                GL.Enable(EnableCap.Texture2D);
                texture[(int)DS_TEXTURE_NUMBER.DS_LAVA].bind(false);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Color3(GROUND_R, GROUND_G, GROUND_B);
            }

            GL.Begin(BeginMode.Quads);
            for (int bx = 0; bx < (heightmap.Length - RenderSteps); bx += RenderSteps)
            {
                for (int bz = 0; bz < (heightmap[bx].Length - RenderSteps); bz += RenderSteps)
                {
                    GL.Color3(255f, 255f, 255f);
                    // 0,0
                    GL.TexCoord2(-ground_scale * multiplayer,
                            -ground_scale * multiplayer);
                    GL.Vertex3((bx - middlePointX) * multiplayer,
                        (bz - middlePointY) * multiplayer,
                    heightmap[bx][bz]);
                    // 1,0
                    GL.TexCoord2(ground_scale * multiplayer,
                        -ground_scale * multiplayer);
                    GL.Vertex3((bx + RenderSteps - middlePointX) * multiplayer,
                        (bz - middlePointY) * multiplayer,
                        heightmap[bx + RenderSteps][bz]);

                    // 1,1
                    GL.TexCoord2(ground_scale * multiplayer,
                            ground_scale * multiplayer);
                    GL.Vertex3((bx + RenderSteps - middlePointX) * multiplayer,
                        (bz + RenderSteps - middlePointY) * multiplayer,
                        heightmap[bx + RenderSteps][bz + RenderSteps]);
                    // 0,1
                    GL.TexCoord2(-ground_scale * multiplayer,
                            ground_scale * multiplayer);
                    GL.Vertex3((bx - middlePointX) * multiplayer,
                        (bz + RenderSteps - middlePointY) * multiplayer,
                        heightmap[bx][bz + RenderSteps]);
                }
            }
            GL.End();
        }

        public float gsize = 25f;

        private void drawGround()
        {
            GL.Disable(EnableCap.Lighting);
            GL.ShadeModel(ShadingModel.Flat);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            if (use_textures)
            {
                GL.Enable(EnableCap.Texture2D);
                ground_texture.bind(false);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Color3(GROUND_R, GROUND_G, GROUND_B);
            }

            
            float offset = 0; // -0.001f; ... polygon offsetting doesn't work well

            GL.Begin(BeginMode.Quads);
            GL.Normal3(0, 0, 1.0f); // GL.Normal3(0, 0, 1) esto no funciona
            GL.TexCoord2(-gsize * ground_scale + ground_ofsx,
                         -gsize * ground_scale + ground_ofsy);
            GL.Vertex3(-gsize / 2.0f, -gsize / 2.0f, offset);
            GL.TexCoord2(gsize * ground_scale + ground_ofsx,
                        -gsize * ground_scale + ground_ofsy);
            GL.Vertex3(gsize / 2.0f, -gsize / 2.0f, offset);
            GL.TexCoord2(gsize * ground_scale + ground_ofsx,
                         gsize * ground_scale + ground_ofsy);
            GL.Vertex3(gsize / 2.0f, gsize / 2.0f, offset);
            GL.TexCoord2(-gsize * ground_scale + ground_ofsx,
                         gsize * ground_scale + ground_ofsy);
            GL.Vertex3(-gsize / 2.0f, gsize / 2.0f, offset);
            GL.End();

            GL.Disable(EnableCap.Fog);
        }
        public GameMap gameMap = null;
        public FieldBorder[] boundingBox = null;

        private void drawSky(float[] view_xyz)
        {
            GL.Disable(EnableCap.Lighting);
            if (use_textures)
            {
                GL.Enable(EnableCap.Texture2D);
                sky_texture.bind(false);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Color3(0f, 0.5f, 1.0f);
            }

            // make sure sky depth is as far back as possible
            GL.ShadeModel(ShadingModel.Flat);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthRange(1.0, 1.0);

            float ssize = 1000.0f;

            float x = ssize * sky_scale;
            float z = view_xyz[2] + sky_height;

            GL.Begin(BeginMode.Quads);
            GL.Normal3(0, 0, -1.0f);
            GL.TexCoord2(-x + offset, -x + offset);
            GL.Vertex3(-ssize + view_xyz[0], -ssize + view_xyz[1], z);
            GL.TexCoord2(-x + offset, x + offset);
            GL.Vertex3(-ssize + view_xyz[0], ssize + view_xyz[1], z);
            GL.TexCoord2(x + offset, x + offset);
            GL.Vertex3(ssize + view_xyz[0], ssize + view_xyz[1], z);
            GL.TexCoord2(x + offset, -x + offset);
            GL.Vertex3(ssize + view_xyz[0], -ssize + view_xyz[1], z);
            GL.End();

            offset = offset + 0.00005f * (float)(60.0d / RenderFrequency);

            GL.DepthFunc(DepthFunction.Less);
            GL.DepthRange(0, 1.0);
        }
        private void setCamera(float x, float y, float z, float h, float p, float r)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Rotate(90, 0, 0, 1);
            GL.Rotate(90, 0, 1, 0);
            GL.Rotate(r, 1, 0, 0);
            GL.Rotate(p, 0, 1, 0);
            GL.Rotate(-h, 0, 0, 1);
            GL.Translate(-x, -y, -z);
        }

        // initialize the above variables

        private void initMotionModel()
        {
            view_xyz[0] = 2;
            view_xyz[1] = 0;
            view_xyz[2] = 1;

            view_hpr[0] = 180;
            view_hpr[1] = 0;
            view_hpr[2] = 0;
        }

        protected void dsSetSphereQuality(int n)
        {
            sphere_quality = n;
        }
        protected void dsSetCapsuleQuality(int n)
        {
            capped_cylinder_quality = n;
        }
        public void dsSetTexture(DS_TEXTURE_NUMBER texture_number)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            tnum = texture_number;
        }
        protected void dsSetColor(double red, double green, double blue)
        {
            dsSetColor((float)red, (float)green, (float)blue);
        }
        public void dsSetColor(float red, float green, float blue)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            color[0] = red;
            color[1] = green;
            color[2] = blue;
            color[3] = 1;
        }
        public void dsSetColorAlpha(float red, float green, float blue, float alpha)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            color[0] = red;
            color[1] = green;
            color[2] = blue;
            color[3] = alpha;
        }

        protected void dsGetViewPoint(out JVector position, out JVector angles)
        {
            position.X = view_xyz[0];
            position.Y = view_xyz[1];
            position.Z = view_xyz[2];

            angles.X = view_hpr[0];
            angles.Y = view_hpr[1];
            angles.Z = view_hpr[2];
        }

        protected void dsSetViewpoint(float[] xyz, float[] hpr)
        {
            if (current_state < 1)
                Console.WriteLine("dsSetViewpoint() called before simulation started");
            if (xyz != null)
            {
                view_xyz[0] = xyz[0];
                view_xyz[1] = xyz[1];
                view_xyz[2] = xyz[2];
            }
            if (hpr != null)
            {
                view_hpr[0] = hpr[0];
                view_hpr[1] = hpr[1];
                view_hpr[2] = hpr[2];
                wrapCameraAngles();
            }
        }
        protected void dsDrawLine(JVector _pos1, JVector _pos2)
        {
            float[] pos1 = Conversion.ToFloat(_pos1);
            float[] pos2 = Conversion.ToFloat(_pos2);
            dsDrawLine(pos1, pos2);
        }
        public void dsDrawLine(float[] pos1, float[] pos2)
        {
            setupDrawingMode();
            GL.Color3(color[0], color[1], color[2]);
            GL.Disable(EnableCap.Lighting);
            GL.LineWidth(2);
            GL.ShadeModel(ShadingModel.Flat);
            GL.Begin(BeginMode.Lines);
            GL.Vertex3(pos1[0], pos1[1], pos1[2]);
            GL.Vertex3(pos2[0], pos2[1], pos2[2]);
            GL.End();
        }
        public void dsDrawSphere(JVector pos, JMatrix R, float radius, double elapsedTime, MagicBullet magicBullet)
        {
            float[] pos2 = Conversion.ToFloat(pos);
            float[] R2 = Conversion.ToFloat(R);
            if (magicBullet is Fireabll)
            {
                Fireabll bl = (Fireabll)magicBullet;
                dsSetTexture(DS_TEXTURE_NUMBER.DS_FIREBALL_CORE);
                dsDrawSphere(pos2, R2, radius, elapsedTime, magicBullet);
                //core
                dsSetTexture(DS_TEXTURE_NUMBER.DS_FIREBALL);
                bl.elapsed += elapsedTime;              

                for (int a = 0; a < bl.previousPosition.Length; a++)
                {
                    dsDrawSphere(Conversion.ToFloat(bl.previousPosition[a]), R2, radius * 1.1f, elapsedTime, magicBullet);
                    dsSetColorAlpha(bl.basicColor.X,
                                bl.basicColor.Y,
                                bl.basicColor.Z,
                                (float)(bl.basicColor.W / (a + 1.1)));
                }

                if (bl.elapsed > bl.elapsedMax)
                {
                    bl.elapsed = 0;
                    for (int a = bl.previousPosition.Length - 1; a > 0; a--)
                    {
                        bl.previousPosition[a] = bl.previousPosition[a - 1];
                    }
                    bl.previousPosition[0] = bl.Position;
                }
            }
            else
                dsDrawSphere(pos2, R2, radius, elapsedTime, magicBullet);
        }
        protected void dsDrawSphere(float[] pos, float[] R, float radius, double elapsedTime, MagicBullet magicBullet)
        {
            if (current_state != 2)
                Console.WriteLine("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.Enable(EnableCap.Normalize);
            GL.ShadeModel(ShadingModel.Smooth);
            setTransform(pos, R);
            GL.Scale(radius, radius, radius);

            ////////////////////disable culling
            GL.Disable(EnableCap.CullFace);
            ////////////////////disable culling
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            drawSphere(elapsedTime, magicBullet);
            GL.PopMatrix();
            GL.Disable(EnableCap.Normalize);

            // draw shadows
            if (use_shadows)
            {
                GL.DepthRange(0.0, 1.0);
                GL.Disable(EnableCap.Lighting);
                if (use_textures)
                {
                    ground_texture.bind(true);
                    GL.Enable(EnableCap.Texture2D);
                    GL.Disable(EnableCap.TextureGenS);
                    GL.Disable(EnableCap.TextureGenT);
                    //TUTUUTUTUTUT
                    GL.Disable(EnableCap.TextureGenQ);
                    GL.Disable(EnableCap.TextureGenR);


                    GL.Color3(SHADOW_INTENSITY, SHADOW_INTENSITY, SHADOW_INTENSITY);
                }
                else
                {
                    GL.Disable(EnableCap.Texture2D);
                    GL.Color3(GROUND_R * SHADOW_INTENSITY, GROUND_G * SHADOW_INTENSITY,
                            GROUND_B * SHADOW_INTENSITY);
                }
                GL.ShadeModel(ShadingModel.Flat);
                GL.DepthRange(0, 0.9999);
                drawSphereShadow(pos[0], pos[1], pos[2], radius);
                ////////////////////disable culling
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Ccw);
                ////////////////////disable culling
            }
        }
        protected void dsDrawTriangle(JVector pos, JMatrix R, float[] vAll, int v0, int v1,
                                       int v2, bool solid)
        {
            if (current_state != 2)
                Console.WriteLine("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Flat);
            setTransform(pos, R);
            drawTriangle(vAll, v0, v1, v2, solid);
            GL.PopMatrix();
        }
        public void dsDrawTriangle(JVector pos, JMatrix R,
                                    JVector v0, JVector v1,
                                    JVector v2, bool solid)
        {
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Flat);
            setTransform(pos, R);
            drawTriangle(v0, v1, v2, solid);
            GL.PopMatrix();
        }
        private void drawTriangle(JVector v0, JVector v1,
                                   JVector v2, bool solid)
        {
            float[] u = new float[3], v = new float[3], normal = new float[3];
            u[0] = (float)(v1.X - v0.X);
            u[1] = (float)(v1.Y - v0.Y);
            u[2] = (float)(v1.Z - v0.Z);
            v[0] = (float)(v2.X - v0.X);
            v[1] = (float)(v2.Y - v0.Y);
            v[2] = (float)(v2.Z - v0.Z);

            //OdeMath.dCROSS(normal, CsODE.OP.EQ, u, v);
            normal[0] = u[1] * v[2] - u[2] - v[1];
            normal[1] = u[2] * v[0] - u[0] - v[2];
            normal[2] = u[0] * v[1] - u[1] - v[0];

            normalizeVector3(normal);

            GL.Begin(solid ? BeginMode.Triangles : BeginMode.LineStrip);
            GL.Normal3(normal[0], normal[1], normal[2]);
            GL.Vertex3(v0.X, v0.Y, v0.Z);
            GL.Vertex3(v1.X, v1.Y, v1.Z);
            GL.Vertex3(v2.X, v2.Y, v2.Z);
            GL.End();
        }
        private void drawTriangle(float[] vAll, int v0, int v1, int v2, bool solid)
        {
            float[] u = new float[3], v = new float[3], normal = new float[3];
            u[0] = vAll[v1] - vAll[v0];
            u[1] = vAll[v1 + 1] - vAll[v0 + 1];
            u[2] = vAll[v1 + 2] - vAll[v0 + 2];
            v[0] = vAll[v2] - vAll[v0];
            v[1] = vAll[v2 + 1] - vAll[v0 + 1];
            v[2] = vAll[v2 + 2] - vAll[v0 + 2];
            dCROSS(normal, OP.EQ, u, v);
            normalizeVector3(normal);

            GL.Begin(solid ? BeginMode.Triangles : BeginMode.LineStrip);
            GL.Normal3(normal[0], normal[1], normal[2]);
            GL.Vertex3(vAll[v0], vAll[v0 + 1], vAll[v0 + 2]);
            GL.Vertex3(vAll[v1], vAll[v1 + 1], vAll[v1 + 2]);
            GL.Vertex3(vAll[v2], vAll[v2 + 1], vAll[v2 + 2]);
            GL.End();
        }
        enum OP
        {
            ADD, SUB, MUL, DIV, EQ, /** += */ ADD_EQ, /** =- */ EQ_SUB,
            /** *= */
            MUL_EQ, /** -= */ SUB_EQ
        }
        static void dCROSS(float[] a, OP op, float[] b, float[] c)
        {
            if (op == OP.EQ)
            {
                a[0] = ((b)[1] * (c)[2] - (b)[2] * (c)[1]);
                a[1] = ((b)[2] * (c)[0] - (b)[0] * (c)[2]);
                a[2] = ((b)[0] * (c)[1] - (b)[1] * (c)[0]);
            }
            else if (op == OP.ADD_EQ)
            {
                a[0] += ((b)[1] * (c)[2] - (b)[2] * (c)[1]);
                a[1] += ((b)[2] * (c)[0] - (b)[0] * (c)[2]);
                a[2] += ((b)[0] * (c)[1] - (b)[1] * (c)[0]);
            }
            else if (op == OP.SUB_EQ)
            {
                a[0] -= ((b)[1] * (c)[2] - (b)[2] * (c)[1]);
                a[1] -= ((b)[2] * (c)[0] - (b)[0] * (c)[2]);
                a[2] -= ((b)[0] * (c)[1] - (b)[1] * (c)[0]);
            }
            else
            {
                throw new InvalidOperationException(op.ToString());
            }
        }
        private void setTransform(JVector pos, JMatrix R)
        {
            //GLdouble
            double[] matrix = new double[16];
            matrix[0] = R.M11;
            matrix[1] = R.M21;
            matrix[2] = R.M31;
            matrix[3] = 0;
            matrix[4] = R.M12;
            matrix[5] = R.M22;
            matrix[6] = R.M32;
            matrix[7] = 0;
            matrix[8] = R.M13;
            matrix[9] = R.M23;
            matrix[10] = R.M33;
            matrix[11] = 0;
            matrix[12] = pos.X;
            matrix[13] = pos.Y;
            matrix[14] = pos.Z;
            matrix[15] = 1;
            GL.PushMatrix();
            GL.MultMatrix(matrix);
        }
        private static bool init = false;
        private static float len2, len1, scale;
        private void drawSphereShadow(float px, float py, float pz, float radius)
        {
            // calculate shadow constants based on light vector
            if (!init)
            {
                len2 = LIGHTX * LIGHTX + LIGHTY * LIGHTY;
                len1 = 1.0f / (float)Math.Sqrt(len2);
                scale = (float)Math.Sqrt(len2 + 1);
                init = true;
            }

            // map sphere center to ground plane based on light vector
            px -= LIGHTX * pz;
            py -= LIGHTY * pz;

            float kx = 0.96592582628907f;
            float ky = 0.25881904510252f;
            float x = radius, y = 0;

            GL.Begin(BeginMode.TriangleFan);
            for (int i = 0; i < 24; i++)
            {
                // for all points on circle, scale to elongated rotated shadow and draw
                float x2 = (LIGHTX * x * scale - LIGHTY * y) * len1 + px;
                float y2 = (LIGHTY * x * scale + LIGHTX * y) * len1 + py;
                GL.TexCoord2(x2 * ground_scale + ground_ofsx, y2 * ground_scale + ground_ofsy);
                GL.Vertex3(x2, y2, 0);

                // rotate [x,y] vector
                float xtmp = kx * x - ky * y;
                y = ky * x + kx * y;
                x = xtmp;
            }
            GL.End();
        }
        protected static int listnum = 0; //GLunint TZ
        private const float ICX = 0.525731112119133606f;
        private const float ICZ = 0.850650808352039932f;
        private readonly float[][] idata = new float[][]
        {
		    new float[]{-ICX, 0, ICZ},
		    new float[]{ICX, 0, ICZ},
		    new float[]{-ICX, 0, -ICZ},
		    new float[]{ICX, 0, -ICZ},
		    new float[]{0, ICZ, ICX},
		    new float[]{0, ICZ, -ICX},
		    new float[]{0, -ICZ, ICX},
		    new float[]{0, -ICZ, -ICX},
		    new float[]{ICZ, ICX, 0},
		    new float[]{-ICZ, ICX, 0},
		    new float[]{ICZ, -ICX, 0},
		    new float[]{-ICZ, -ICX, 0}
	    };
        private readonly int[][] index = new int[][]
        {
		    new int[]{0, 4, 1}, new int[]{0, 9, 4},
		    new int[]{9, 5, 4},	  new int[]{4, 5, 8},
		    new int[]{4, 8, 1},	  new int[]{8, 10, 1},
		    new int[]{8, 3, 10},   new int[]{5, 3, 8},
		    new int[]{5, 2, 3},	  new int[]{2, 7, 3},
		    new int[]{7, 10, 3},   new int[]{7, 6, 10},
		    new int[]{7, 11, 6},   new int[]{11, 0, 6},
		    new int[]{0, 1, 6},	  new int[]{6, 1, 10},
		    new int[]{9, 0, 11},   new int[]{9, 11, 2},
		    new int[]{9, 2, 5},	 new int[] {7, 2, 11},
	    };
        private int sphere_quality = 1;

        private void drawSphere(double elapsedTime, MagicBullet magicBullet)
        {
            ///////////////////
            if (magicBullet != null)
            {
                GL.Enable(EnableCap.TextureGenS);
                GL.Enable(EnableCap.TextureGenT);

                GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, (int)All.ObjectLinear);
                GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, (int)All.ObjectLinear);

                GL.TexGen(TextureCoordName.S, TextureGenParameter.ObjectPlane, this.s_params_SDM);
                GL.TexGen(TextureCoordName.T, TextureGenParameter.ObjectPlane, this.t_params_SDM);


                if (magicBullet.disortTextureFactor1Offset != 0)
                {
                    if (magicBullet.disortTextureFactor1OffsetUp)
                        magicBullet.disortTextureFactor1 += (float)(magicBullet.disortTextureFactor1Offset * elapsedTime);
                    else
                        magicBullet.disortTextureFactor1 -= (float)(magicBullet.disortTextureFactor1Offset * elapsedTime);
                    if (magicBullet.disortTextureFactor1 > 1)
                    {
                        magicBullet.disortTextureFactor1 = 1;
                        magicBullet.disortTextureFactor1OffsetUp = false;
                    }
                    if (magicBullet.disortTextureFactor1 < -1)
                    {
                        magicBullet.disortTextureFactor1 = -1;
                        magicBullet.disortTextureFactor1OffsetUp = true;
                    }

                }

                if (magicBullet.disortTextureFactor2Offset != 0)
                {
                    if (magicBullet.disortTextureFactor2OffsetUp)
                        magicBullet.disortTextureFactor2 += (float)(magicBullet.disortTextureFactor2Offset * elapsedTime);
                    else
                        magicBullet.disortTextureFactor2 -= (float)(magicBullet.disortTextureFactor2Offset * elapsedTime);
                    if (magicBullet.disortTextureFactor2 > 1)
                    {
                        magicBullet.disortTextureFactor2 = 1;
                        magicBullet.disortTextureFactor2OffsetUp = false;
                    }
                    if (magicBullet.disortTextureFactor2 < -1)
                    {
                        magicBullet.disortTextureFactor2 = -1;
                        magicBullet.disortTextureFactor2OffsetUp = true;
                    }
                }


                if (magicBullet.disortTextureFactor3Offset != 0)
                {
                    if (magicBullet.disortTextureFactor3OffsetUp)
                        magicBullet.disortTextureFactor3 += (float)(magicBullet.disortTextureFactor3Offset * elapsedTime);
                    else
                        magicBullet.disortTextureFactor3 -= (float)(magicBullet.disortTextureFactor3Offset * elapsedTime);
                    if (magicBullet.disortTextureFactor3 > 1)
                    {
                        magicBullet.disortTextureFactor3 = 1;
                        magicBullet.disortTextureFactor3OffsetUp = false;
                    }
                    if (magicBullet.disortTextureFactor3 < -1)
                    {
                        magicBullet.disortTextureFactor3 = -1;
                        magicBullet.disortTextureFactor2OffsetUp = true;
                    }
                }

                float[] t_params_SDM = new float[] { magicBullet.disortTextureFactor1,
                    magicBullet.disortTextureFactor2, 
                    -magicBullet.disortTextureFactor3, 
                    magicBullet.disortTextureFactor3 };
                GL.TexGen(TextureCoordName.T, TextureGenParameter.ObjectPlane, t_params_SDM);
            }
            ////////////////////
            // icosahedron data for an icosahedron of radius 1.0
            //		# define ICX 0.525731112119133606f
            //		# define ICZ 0.850650808352039932f
            if (listnum == 0)
            {
                listnum = GL.GenLists(1);
                GL.NewList(listnum, ListMode.Compile);
                GL.Begin(BeginMode.Triangles);
                for (int i = 0; i < 20; i++)
                {
                    //				drawPatch (&idata[index[i][2]][0],&idata[index[i][1]][0],
                    //						&idata[index[i][0]][0],sphere_quality);
                    drawPatch(idata[index[i][2]], idata[index[i][1]],
                            idata[index[i][0]], sphere_quality);
                }
                GL.End();
                GL.EndList();
            }
            GL.CallList(listnum);
        }
        // This is recursively subdivides a triangular area (vertices p1,p2,p3) into
        // smaller triangles, and then draws the triangles. All triangle vertices are
        // normalized to a distance of 1.0 from the origin (p1,p2,p3 are assumed
        // to be already normalized). Note this is not super-fast because it draws
        // triangles rather than triangle strips.

        //	static void drawPatch (float p1[3], float p2[3], float p3[3], int level)
        private void drawPatch(float[] p1, float[] p2, float[] p3, int level)
        {
            int i;
            if (level > 0)
            {
                float[] q1 = new float[3], q2 = new float[3], q3 = new float[3];		 // sub-vertices
                for (i = 0; i < 3; i++)
                {
                    q1[i] = 0.5f * (p1[i] + p2[i]);
                    q2[i] = 0.5f * (p2[i] + p3[i]);
                    q3[i] = 0.5f * (p3[i] + p1[i]);
                }
                float length1 = (float)(1.0 / Math.Sqrt(q1[0] * q1[0] + q1[1] * q1[1] + q1[2] * q1[2]));
                float length2 = (float)(1.0 / Math.Sqrt(q2[0] * q2[0] + q2[1] * q2[1] + q2[2] * q2[2]));
                float length3 = (float)(1.0 / Math.Sqrt(q3[0] * q3[0] + q3[1] * q3[1] + q3[2] * q3[2]));
                for (i = 0; i < 3; i++)
                {
                    q1[i] *= length1;
                    q2[i] *= length2;
                    q3[i] *= length3;
                }
                drawPatch(p1, q1, q3, level - 1);
                drawPatch(q1, p2, q2, level - 1);
                drawPatch(q1, q2, q3, level - 1);
                drawPatch(q3, q2, p3, level - 1);
            }
            else
            {
                GL.Normal3(p1[0], p1[1], p1[2]);
                GL.Vertex3(p1[0], p1[1], p1[2]);
                GL.Normal3(p2[0], p2[1], p2[2]);
                GL.Vertex3(p2[0], p2[1], p2[2]);
                GL.Normal3(p3[0], p3[1], p3[2]);
                GL.Vertex3(p3[0], p3[1], p3[2]);
            }
        }
        public void dsDrawConvex(JVector pos, JMatrix R,
                                double[] _planes, int _planecount,
                                double[] _points,
                                int _pointcount,
                                int[] _polygons)
        {
            if (current_state != 2)
                Console.WriteLine("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Flat);
            setTransform(pos, R);
            drawConvexD(_planes, _planecount, _points, _pointcount, _polygons);
            GL.PopMatrix();
            if (use_shadows)
            {
                GL.DepthRange(0, 1);
                setShadowDrawingMode();
                setShadowTransform();
                setTransform(pos, R);
                drawConvexD(_planes, _planecount, _points, _pointcount, _polygons);
                GL.PopMatrix();
                GL.PopMatrix();
            }
        }
        private void drawConvexD(double[] _planes, int _planecount,
                                double[] _points,
                                int _pointcount,
                                int[] _polygons)
        {
            //unsigned 
            int polyindex = 0;
            for (int i = 0; i < _planecount; ++i)
            {
                //unsigned 
                int pointcount = _polygons[polyindex];
                polyindex++;
                GL.Begin(BeginMode.Polygon);
                GL.Normal3(_planes[(i * 4) + 0],
                        _planes[(i * 4) + 1],
                        _planes[(i * 4) + 2]);
                for (int j = 0; j < pointcount; ++j)
                {
                    GL.Vertex3(_points[_polygons[polyindex] * 3],
                            _points[(_polygons[polyindex] * 3) + 1],
                            _points[(_polygons[polyindex] * 3) + 2]);
                    polyindex++;
                }
                GL.End();
            }
        }
        protected void dsDrawCapsule(JVector pos, JMatrix R,
            float length, float radius)
        {
            float[] pos2 = Conversion.ToFloat(pos);
            float[] R2 = Conversion.ToFloat(R);
            dsDrawCapsule(pos2, R2, length, radius);
        }
        protected void dsDrawCapsule(float[] pos, float[] R,
            float length, float radius)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Smooth);
            setTransform(pos, R);
            drawCapsule(length, radius);
            GL.PopMatrix();

            if (use_shadows)
            {
                GL.DepthRange(0, 1);
                setShadowDrawingMode();
                setShadowTransform();
                setTransform(pos, R);
                drawCapsule(length, radius);
                GL.PopMatrix();
                GL.PopMatrix();
            }
        }
        private int capped_cylinder_quality = 3;

        private void drawCapsule(float l, float r)
        {
            int i, j;
            float tmp, nx, ny, nz, start_nx, start_ny, a, ca, sa;
            // number of sides to the cylinder (divisible by 4):
            int n = capped_cylinder_quality * 4;

            l *= 0.5f;
            a = (float)((M_PI * 2.0) / (float)n);
            sa = (float)Math.Sin(a);
            ca = (float)Math.Cos(a);

            // draw cylinder body
            ny = 1; nz = 0;		  // normal vector = (0,ny,nz)
            GL.Begin(BeginMode.TriangleStrip);
            for (i = 0; i <= n; i++)
            {
                GL.Normal3(ny, nz, 0);
                GL.Vertex3(ny * r, nz * r, l);
                GL.Normal3(ny, nz, 0);
                GL.Vertex3(ny * r, nz * r, -l);
                // rotate ny,nz
                tmp = ca * ny - sa * nz;
                nz = sa * ny + ca * nz;
                ny = tmp;
            }
            GL.End();

            // draw first cylinder cap
            start_nx = 0;
            start_ny = 1;
            for (j = 0; j < (n / 4); j++)
            {
                // get start_n2 = rotated start_n
                float start_nx2 = ca * start_nx + sa * start_ny;
                float start_ny2 = -sa * start_nx + ca * start_ny;
                // get n=start_n and n2=start_n2
                nx = start_nx; ny = start_ny; nz = 0;
                float nx2 = start_nx2, ny2 = start_ny2, nz2 = 0;
                GL.Begin(BeginMode.TriangleStrip);
                for (i = 0; i <= n; i++)
                {
                    GL.Normal3(ny2, nz2, nx2);
                    GL.Vertex3(ny2 * r, nz2 * r, l + nx2 * r);
                    GL.Normal3(ny, nz, nx);
                    GL.Vertex3(ny * r, nz * r, l + nx * r);
                    // rotate n,n2
                    tmp = ca * ny - sa * nz;
                    nz = sa * ny + ca * nz;
                    ny = tmp;
                    tmp = ca * ny2 - sa * nz2;
                    nz2 = sa * ny2 + ca * nz2;
                    ny2 = tmp;
                }
                GL.End();
                start_nx = start_nx2;
                start_ny = start_ny2;
            }

            // draw second cylinder cap
            start_nx = 0;
            start_ny = 1;
            for (j = 0; j < (n / 4); j++)
            {
                // get start_n2 = rotated start_n
                float start_nx2 = ca * start_nx - sa * start_ny;
                float start_ny2 = sa * start_nx + ca * start_ny;
                // get n=start_n and n2=start_n2
                nx = start_nx; ny = start_ny; nz = 0;
                float nx2 = start_nx2, ny2 = start_ny2, nz2 = 0;
                GL.Begin(BeginMode.TriangleStrip);
                for (i = 0; i <= n; i++)
                {
                    GL.Normal3(ny, nz, nx);
                    GL.Vertex3(ny * r, nz * r, -l + nx * r);
                    GL.Normal3(ny2, nz2, nx2);
                    GL.Vertex3(ny2 * r, nz2 * r, -l + nx2 * r);
                    // rotate n,n2
                    tmp = ca * ny - sa * nz;
                    nz = sa * ny + ca * nz;
                    ny = tmp;
                    tmp = ca * ny2 - sa * nz2;
                    nz2 = sa * ny2 + ca * nz2;
                    ny2 = tmp;
                }
                GL.End();
                start_nx = start_nx2;
                start_ny = start_ny2;
            }
        }
        public void dsDrawCylinder(JVector pos, JMatrix R, float length, float radius)
        {
            float[] pos2 = Conversion.ToFloat(pos);
            float[] R2 = Conversion.ToFloat(R);
            dsDrawCylinder(pos2, R2, length, radius);
        }

        public void dsDrawCylinder(float[] pos, float[] R, float length, float radius)
        {
            if (current_state != 2)
                Console.WriteLine("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Smooth);
            setTransform(pos, R);
            drawCylinder(length, radius, 0);
            GL.PopMatrix();

            if (use_shadows)
            {
                GL.DepthRange(0, 1.0);
                setShadowDrawingMode();
                setShadowTransform();
                setTransform(pos, R);
                drawCylinder(length, radius, 0);
                GL.PopMatrix();
                GL.PopMatrix();
            }
        }
        void drawCylinder(float l, float r, float zoffset)
        {
            int i;
            float tmp, ny, nz, a, ca, sa;
            int n = 24;	// number of sides to the cylinder (divisible by 4)

            l *= 0.5f;
            a = (float)(M_PI * 2.0 / (float)n);
            sa = (float)Math.Sin(a);
            ca = (float)Math.Cos(a);

            // draw cylinder body
            ny = 1; nz = 0;		  // normal vector = (0,ny,nz)
            GL.Begin(BeginMode.TriangleStrip);
            for (i = 0; i <= n; i++)
            {
                GL.Normal3(ny, nz, 0);
                GL.Vertex3(ny * r, nz * r, l + zoffset);
                GL.Normal3(ny, nz, 0);
                GL.Vertex3(ny * r, nz * r, -l + zoffset);
                // rotate ny,nz
                tmp = ca * ny - sa * nz;
                nz = sa * ny + ca * nz;
                ny = tmp;
            }
            GL.End();

            // draw top cap
            GL.ShadeModel(ShadingModel.Flat);
            ny = 1; nz = 0;		  // normal vector = (0,ny,nz)
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0, 0, 1.0f);
            GL.Vertex3(0, 0, l + zoffset);
            for (i = 0; i <= n; i++)
            {
                if (i == 1 || i == n / 2 + 1)
                    setColor(color[0] * 0.75f, color[1] * 0.75f, color[2] * 0.75f, color[3]);
                GL.Normal3(0, 0, 1.0f);
                GL.Vertex3(ny * r, nz * r, l + zoffset);
                if (i == 1 || i == n / 2 + 1)
                    setColor(color[0], color[1], color[2], color[3]);

                // rotate ny,nz
                tmp = ca * ny - sa * nz;
                nz = sa * ny + ca * nz;
                ny = tmp;
            }
            GL.End();

            // draw bottom cap
            ny = 1; nz = 0;		  // normal vector = (0,ny,nz)
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0, 0, -1.0f);
            GL.Vertex3(0, 0, -l + zoffset);
            for (i = 0; i <= n; i++)
            {
                if (i == 1 || i == n / 2 + 1)
                    setColor(color[0] * 0.75f, color[1] * 0.75f, color[2] * 0.75f, color[3]);
                GL.Normal3(0, 0, -1.0f);
                GL.Vertex3(ny * r, nz * r, -l + zoffset);
                if (i == 1 || i == n / 2 + 1)
                    setColor(color[0], color[1], color[2], color[3]);

                // rotate ny,nz
                tmp = ca * ny + sa * nz;
                nz = -sa * ny + ca * nz;
                ny = tmp;
            }
            GL.End();
        }

        private void drawBoxTexuturedShadows(float[] sides)
        {
            float lx = sides[0] * 0.5f;
            float ly = sides[1] * 0.5f;
            float lz = sides[2] * 0.5f;

            // sides
            GL.Begin(BeginMode.TriangleStrip);
            GL.Normal3(-1.0f, 0, 0); // GL.Normal3(-1, 0, 0) no funciona
            GL.Vertex3(-lx, -ly, -lz);
            GL.Vertex3(-lx, -ly, lz);
            GL.Vertex3(-lx, ly, -lz);
            GL.Vertex3(-lx, ly, lz);
            GL.Normal3(0, 1.0f, 0); // GL.Normal3(0, 1, 0) no funciona
            GL.Vertex3(lx, ly, -lz);
            GL.Vertex3(lx, ly, lz);
            GL.Normal3(1.0f, 0, 0); // GL.Normal3(1, 0, 0) no funciona
            GL.Vertex3(lx, -ly, -lz);
            GL.Vertex3(lx, -ly, lz);
            GL.Normal3(0, -1.0f, 0); // GL.Normal3(0, -1, 0) no funciona
            GL.Vertex3(-lx, -ly, -lz);
            GL.Vertex3(-lx, -ly, lz);
            GL.End();

            // top face
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0, 0, 1.0f); // GL.Normal3(0, 0, 1) no funciona
            GL.Vertex3(-lx, -ly, lz);
            GL.Vertex3(lx, -ly, lz);
            GL.Vertex3(lx, ly, lz);
            GL.Vertex3(-lx, ly, lz);
            GL.End();

            // bottom face
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0, 0, -1.0f); // GL.Normal3(0, 0, -1) no funciona
            GL.Vertex3(-lx, -ly, -lz);
            GL.Vertex3(-lx, ly, -lz);
            GL.Vertex3(lx, ly, -lz);
            GL.Vertex3(lx, -ly, -lz);
            GL.End();
        }
        public bool use_lighting = true;

        private void drawBoxTexuturedOK(float[] sides, Texture[] textures = null, bool repeat = true)
        {
            float lx = sides[0] * 0.5f;
            float ly = sides[1] * 0.5f;
            float lz = sides[2] * 0.5f;
            setupDrawingMode(new float[] { 1.0f, 0f, 0.0f, 0 }, new float[] { 1, 0, 0, 0 });
            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.ShadeModel(ShadingModel.Flat);

            if (use_lighting == false)
                GL.Disable(EnableCap.Lighting);
            else
                GL.Enable(EnableCap.Lighting);
            //FRONT
            if (textures != null)
                textures[3].bind(true);
            GL.Begin(BeginMode.TriangleFan);

            GL.Normal3(-1.0f, 0, 0); // GL.Normal3(-1, 0, 0) no funciona

            if (repeat)
            {
                // set texture parameters - will these also be bound to the texture???
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Decal);
                GL.TexCoord2(sides[0], 0);  GL.Vertex3(-lx, -ly, -lz);
                GL.TexCoord2(sides[0], sides[0]); GL.Vertex3(-lx, -ly, lz);
                GL.TexCoord2(0.0f, sides[0]); GL.Vertex3(-lx, ly, lz);
                GL.TexCoord2(0.0f, 0);  GL.Vertex3(-lx, ly, -lz); 

            }
            else
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, -lz);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-lx, -ly, lz);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-lx, ly, lz);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-lx, ly, -lz);
            }
            GL.End();
            //BACK
            if (textures != null)
                textures[2].bind(true);
            GL.Begin(BeginMode.TriangleFan);
            
            GL.Normal3(1.0f, 0, 0); // GL.Normal3(-1, 0, 0) no funciona
            if (repeat)
            {
                GL.TexCoord2(0.0f, sides[0]); GL.Vertex3(lx, -ly, lz);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(lx, -ly, -lz);
                GL.TexCoord2(sides[0], 0.0f); GL.Vertex3(lx, ly, -lz);
                GL.TexCoord2(sides[0], sides[0]); GL.Vertex3(lx, ly, lz);
            }
            else
            {
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(lx, -ly, lz);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(lx, -ly, -lz);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(lx, ly, -lz);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(lx, ly, lz);
            }
            GL.End();
            //RIGHT
            if (textures != null)
                textures[5].bind(true);
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0.0f, -1.0, 0); // GL.Normal3(-1, 0, 0) no funciona
            if (repeat)
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, -lz);
                GL.TexCoord2(sides[0], 0.0f); GL.Vertex3(lx, -ly, -lz);
                GL.TexCoord2(sides[0], sides[0]); GL.Vertex3(lx, -ly, lz);
                GL.TexCoord2(0.0f, sides[0]); GL.Vertex3(-lx, -ly, lz);
            }
            else
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, -lz);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(lx, -ly, -lz);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(lx, -ly, lz);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-lx, -ly, lz);
            }
            GL.End();
            //LEFT
            if (textures != null)
                textures[4].bind(true);
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0.0f, 1.0, 0); // GL.Normal3(-1, 0, 0) no funciona
            if (repeat)
            {
                GL.TexCoord2(sides[0], sides[0]); GL.Vertex3(-lx, ly, lz);
                GL.TexCoord2(0, sides[0]); GL.Vertex3(lx, ly, lz);
                GL.TexCoord2(0, 0.0f); GL.Vertex3(lx, ly, -lz);
                GL.TexCoord2(sides[0], 0.0f); GL.Vertex3(-lx, ly, -lz);
            }
            else
            {
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-lx, ly, lz);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(lx, ly, lz);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(lx, ly, -lz);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, ly, -lz);
            }
            GL.End();

            //TOP
            if (textures != null)
                textures[0].bind(true);
            
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0, 0, 1.0f); // GL.Normal3(0, 0, 1) no funciona
            if (repeat)
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, lz);
                GL.TexCoord2(sides[0], 0.0f); GL.Vertex3(lx, -ly, lz);
                GL.TexCoord2(sides[0], sides[0]); GL.Vertex3(lx, ly, lz);
                GL.TexCoord2(0.0f, sides[0]); GL.Vertex3(-lx, ly, lz);
            }
            else
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, lz);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(lx, -ly, lz);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(lx, ly, lz);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-lx, ly, lz);
            }
            GL.End();

            //BOTTOM
            if (textures != null)
                textures[1].bind(true);
            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(0, 0, -1.0f); // GL.Normal3(0, 0, -1) no funciona
            if (repeat)
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, -lz);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-lx, ly, -lz);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(lx, ly, -lz);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(lx, -ly, -lz);
            }
            else
            {
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-lx, -ly, -lz);
                GL.TexCoord2(0.0f, sides[0]); GL.Vertex3(-lx, ly, -lz);
                GL.TexCoord2(sides[0], sides[0]); GL.Vertex3(lx, ly, -lz);
                GL.TexCoord2(sides[0], 0.0f); GL.Vertex3(lx, -ly, -lz);
            }
            GL.End();
        }

        public void dsDrawModel(JVector pos, JMatrix R, JVector sides)
        {
            float[] pos2 = Conversion.ToFloat(pos);
            float[] R2 = Conversion.ToFloat(R);
            float[] fsides = Conversion.ToFloat(sides);
            dsDrawModel(pos2, R2, fsides);
        }

        /// /////////////////////////////////////

        public void dsDrawModel(Matrix4 matrix, JVector sides, Texture[] textures = null)
        {
            float[] fsides = Conversion.ToFloat(sides);
            dsDrawModel(matrix, fsides, textures);
        }

        protected void dsDrawModel(Matrix4 matrix, float[] sides, Texture[] textures = null)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            //textures exists but probably tnum == DS_TEXTURE_NUMBER.DS_NONE
            if (textures != null)
                setupDrawingMode(true);
            else
                setupDrawingMode();
            GL.ShadeModel(ShadingModel.Flat);
            setTransform(matrix);
            drawBoxTexuturedOK(sides, textures);
            GL.PopMatrix();
            if (use_shadows)
            {
                GL.DepthRange(0, 1.0);
                setShadowDrawingMode();
                setShadowTransform();
                setTransform(matrix);
                drawBoxTexuturedShadows(sides);
                GL.PopMatrix();
                GL.PopMatrix();
            }
        }

        /// /////////////////////////////////////


        protected void dsDrawModel(float[] pos, float[] R, float[] sides)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Flat);
            setTransform(pos, R);
            drawBoxTexuturedOK(sides);
            GL.PopMatrix();
            if (use_shadows)
            {
                GL.DepthRange(0, 1.0);
                setShadowDrawingMode();
                setShadowTransform();
                setTransform(pos, R);
                drawBoxTexuturedShadows(sides);
                GL.PopMatrix();
                GL.PopMatrix();
            }
        }

        public void dsDrawBox(JVector pos, JMatrix R, JVector sides)
        {
            float[] pos2 = Conversion.ToFloat(pos);
            float[] R2 = Conversion.ToFloat(R);
            float[] fsides = Conversion.ToFloat(sides);
            dsDrawBox(pos2, R2, fsides);
        }
        protected void dsDrawBox(float[] pos, float[] R, float[] sides)
        {
            if (current_state != 2)
                throw new Exception("drawing function called outside simulation loop");
            setupDrawingMode();
            GL.ShadeModel(ShadingModel.Flat);
            setTransform(pos, R);
            drawBoxTexuturedOK(sides);
            GL.PopMatrix();
            if (use_shadows)
            {
                GL.DepthRange(0, 1.0);
                setShadowDrawingMode();
                setShadowTransform();
                setTransform(pos, R);
                drawBoxTexuturedShadows(sides);
                GL.PopMatrix();
                GL.PopMatrix();
            }
        }
        private void setShadowDrawingMode()
        {
            GL.Disable(EnableCap.Lighting);
            if (use_textures)
            {
                GL.Enable(EnableCap.Texture2D);
                ground_texture.bind(true);
                GL.Color3(SHADOW_INTENSITY, SHADOW_INTENSITY, SHADOW_INTENSITY);
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.TextureGenS);
                GL.Enable(EnableCap.TextureGenT);
                GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, (int)All.EyeLinear);
                GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, (int)All.EyeLinear);
                GL.TexGen(TextureCoordName.S, TextureGenParameter.EyePlane, s_params_SSDM);
                GL.TexGen(TextureCoordName.T, TextureGenParameter.EyePlane, t_params_SSDM);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Color3(GROUND_R * SHADOW_INTENSITY, GROUND_G * SHADOW_INTENSITY, GROUND_B * SHADOW_INTENSITY);
            }
            GL.DepthRange(0, 0.9999);
        }
        private void setShadowTransform()
        {
            //GLfloat
            float[] matrix = new float[16];
            for (int i = 0; i < 16; i++) matrix[i] = 0;
            matrix[0] = 1.0f;
            matrix[5] = 1.0f;
            matrix[8] = -LIGHTX;
            matrix[9] = -LIGHTY;
            matrix[15] = 1.0f;

            GL.PushMatrix();
            GL.MultMatrix(matrix);
        }
        public void setupDrawingMode(bool forceTextures = false)
        {
            GL.Enable(EnableCap.Lighting);
            if (tnum != DS_TEXTURE_NUMBER.DS_NONE || forceTextures)
            {
                if (use_textures)
                {
                    GL.Enable(EnableCap.Texture2D);
                    if (tnum != DS_TEXTURE_NUMBER.DS_NONE)
                        texture[(int)tnum].bind(true);
                    
                    GL.Enable(EnableCap.TextureGenS);
                    GL.Enable(EnableCap.TextureGenT);
                    
                    GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, (int)All.ObjectLinear);
                    GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, (int)All.ObjectLinear);

                    GL.TexGen(TextureCoordName.S, TextureGenParameter.ObjectPlane, s_params_SDM);
                    GL.TexGen(TextureCoordName.T, TextureGenParameter.ObjectPlane, t_params_SDM);
                }
                else
                {
                    GL.Disable(EnableCap.Texture2D);
                }
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
            }
            setColor(color[0], color[1], color[2], color[3]);

            if (color[3] < 1.0f)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        private void setupDrawingMode(float[] s_params_SDM, float[] t_params_SDM)
        {
            GL.Enable(EnableCap.Lighting);
            if (tnum != DS_TEXTURE_NUMBER.DS_NONE)
            {
                if (use_textures)
                {
                    GL.Enable(EnableCap.Texture2D);
                    texture[(int)tnum].bind(true);
                    GL.Enable(EnableCap.TextureGenS);
                    GL.Enable(EnableCap.TextureGenT);

                    GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, (int)All.ObjectLinear);
                    GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, (int)All.ObjectLinear);

                    GL.TexGen(TextureCoordName.S, TextureGenParameter.ObjectPlane, s_params_SDM);
                    GL.TexGen(TextureCoordName.T, TextureGenParameter.ObjectPlane, t_params_SDM);
                }
                else
                {
                    GL.Disable(EnableCap.Texture2D);
                }
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
            }
            setColor(color[0], color[1], color[2], color[3]);

            if (color[3] < 1.0f)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        private void setColor(float r, float g, float b, float alpha)
        {
            light_ambient2 = new float[] { r * 0.3f, g * 0.3f, b * 0.3f, alpha };
            light_diffuse2 = new float[] { r * 0.7f, g * 0.7f, b * 0.7f, alpha };
            light_specular2 = new float[] { r * 0.2f, g * 0.2f, b * 0.2f, alpha };

            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, light_ambient2);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, light_diffuse2);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, light_specular2);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 5.0f);
        }



        public void setTransform(Matrix4 mat)
        {
            float[] matrix = new float[16];
            matrix[0] = mat.Row0.X;
            matrix[1] = mat.Row0.Y;
            matrix[2] = mat.Row0.Z;
            matrix[3] = mat.Row0.W;
            matrix[4] = mat.Row1.X;
            matrix[5] = mat.Row1.Y;
            matrix[6] = mat.Row1.Z;
            matrix[7] = mat.Row1.W;
            matrix[8] = mat.Row2.X;
            matrix[9] = mat.Row2.Y;
            matrix[10] = mat.Row2.Z;
            matrix[11] = mat.Row2.W;
            matrix[12] = mat.Row3.X;
            matrix[13] = mat.Row3.Y;
            matrix[14] = mat.Row3.Z;
            matrix[15] = mat.Row3.W;

            GL.PushMatrix();
            GL.MultMatrix(matrix);
        }

        public void setTransform(float[] pos, float[] R)
        {
            //GLfloat
            float[] matrix = new float[16];
            matrix[0] = R[0];
            matrix[1] = R[4];
            matrix[2] = R[8];
            matrix[3] = 0;
            matrix[4] = R[1];
            matrix[5] = R[5];
            matrix[6] = R[9];
            matrix[7] = 0;
            matrix[8] = R[2];
            matrix[9] = R[6];
            matrix[10] = R[10];
            matrix[11] = 0;
            matrix[12] = pos[0];
            matrix[13] = pos[1];
            matrix[14] = pos[2];
            matrix[15] = 1;
            GL.PushMatrix();
            GL.MultMatrix(matrix);
        }

    }

}

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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Reflection;
using FieldOfJudgement.Sound;

namespace FieldOfJudgement
{
    /**
     * This class displays the start screen and credtis screen.
     */
    class StartScreen : GameWindow
    {
        public StartScreen(int width, int height)
            : this(width, height, GraphicsMode.Default, "Field of Judgement", 0, DisplayDevice.Default)
        { 
        }   
        Texture startScreenTexture = null;
        Texture creditsTexture = null;
        public StartScreen (int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)
            : base(width, height, mode, title, options, device)
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);

            Assembly a = Assembly.GetExecutingAssembly();
            Stream readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.start_screen.png");
            startScreenTexture = new Texture(readerStream);
            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.credits.PNG");
            creditsTexture = new Texture(readerStream);

            Audio.LoadSounds();
            Music.StartMusic(SoundType.IntoTheCube);
            GoFullscreen(StartClass.gameConf.IsFullScrean);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Draw(e.Time);
            SwapBuffers();
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Space)
            {
                StartClass.StartGame = true;
                this.Exit();
            }
            if (e.Key == OpenTK.Input.Key.Escape)
            {
                StartClass.CloseGame = true;
                this.Exit();
            }
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnDisposed(e);
            Music.StopMusic(SoundType.IntoTheCube);
            Audio.Dispose();
            startScreenTexture.finalize();
            creditsTexture.finalize();
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


        double elapsed = 0;
        public void Draw(double time)
        {
            elapsed += time;
            GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.Disable(EnableCap.DepthTest);

            GL.Ortho(0, ClientSize.Width, ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Lighting);
            GL.Color4(1, 0, 0, 0.5f);
            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (elapsed < 20)
            {
                GL.BindTexture(TextureTarget.Texture2D, startScreenTexture.name);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, creditsTexture.name);
                if (elapsed > 40)
                    elapsed = 0;
            }
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Replace);
            // Draw a textured quad
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex3(0, 0, 0.5f);
            GL.TexCoord2(0, 0); GL.Vertex3(0, ClientSize.Height, 0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(ClientSize.Width, ClientSize.Height, 0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(ClientSize.Width, 0, 0.5f);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}

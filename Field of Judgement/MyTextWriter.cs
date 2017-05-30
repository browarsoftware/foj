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
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using OpenTK;
using System.Drawing.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace FieldOfJudgement
{
    public class MyTextWriter
    {
        private readonly Font TextFont = new Font(FontFamily.GenericSansSerif, 24,FontStyle.Bold);
        FontFamily family = null;
        PrivateFontCollection fonts = null;
        private readonly Bitmap TextBitmap = null;
        private List<PointF> _positions;
        private List<string> _lines;
        private List<Brush> _colours;
        private int _textureId;
        //private Size _clientSize;
        //private FontCollection fontCollection = null;

         // load font family from byte array
        public static FontFamily LoadFontFamily(byte[] buffer, out PrivateFontCollection fontCollection)
        {
            //pin array so we can get its address
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                fontCollection = new PrivateFontCollection();
                fontCollection.AddMemoryFont(ptr, buffer.Length);
                return fontCollection.Families[0];

            }
            finally
            {
                // don't forget to unpin the array!
                handle.Free();
            }
        }

      // load font family from byte array

        public static unsafe FontFamily LoadFontFamilyUnsafe(byte[] buffer, out PrivateFontCollection fontCollection) 
        {
             fixed (byte* ptr = buffer) {
                fontCollection = new PrivateFontCollection();
                fontCollection.AddMemoryFont(new IntPtr(ptr), buffer.Length);
                return fontCollection.Families[0];
              }
        }

        // Load font family from stream
        public static FontFamily LoadFontFamily(Stream stream, out PrivateFontCollection fontCollection) 
        {
          var buffer = new byte[stream.Length];
          stream.Read(buffer, 0, buffer.Length);
          return LoadFontFamily(buffer, out fontCollection);
        }

        public void Update(int ind, string newText)
        {
            if (ind < _lines.Count)
            {
                _lines[ind] = newText;
                UpdateText();
            }
        }

        FieldOfJudgementGame program = null;
        //texture size
        public int textureWidth = 800;
        public int textureHeight = 600;

        public MyTextWriter(FieldOfJudgementGame program)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            String[] nombres = a.GetManifestResourceNames();

            //http://mertol.deviantart.com/gallery/
            Stream s = a.GetManifestResourceStream("FieldOfJudgement.Resources.Fonts.MagicSchoolTwo.ttf");
            family = LoadFontFamily(s, out fonts);
            TextFont = new Font(family, 22.0f);
      
            _positions = new List<PointF>();
            _lines = new List<string>();
            _colours = new List<Brush>();

            this.program = program;
            TextBitmap = new Bitmap(textureWidth, textureHeight);
            _textureId = CreateTexture();
        }

        private int CreateTexture()
        {
            int textureId;
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.ReplaceExt);//Important, or wrong color on some computers
            Bitmap bitmap = TextBitmap;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //BGRA
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            
            GL.Finish();
            bitmap.UnlockBits(data);
            return textureId;
        }

        public void Dispose()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
        }

        public void Clear()
        {
            _lines.Clear();
            _positions.Clear();
            _colours.Clear();
        }

        public void AddLine(string s, PointF pos, Brush col)
        {
            _lines.Add(s);
            _positions.Add(pos);
            _colours.Add(col);
            UpdateText();
        }

        public void UpdateText()
        {
            if (_lines.Count > 0)
            {
                using (Graphics gfx = Graphics.FromImage(TextBitmap))
                {
                    gfx.Clear(Color.FromArgb(0, 184, 56, 11));
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    for (int i = 0; i < _lines.Count; i++)
                        gfx.DrawString(_lines[i], TextFont, _colours[i], _positions[i]);
                }

                System.Drawing.Imaging.BitmapData data = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //BGRA
                GL.BindTexture(TextureTarget.Texture2D, _textureId);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                TextBitmap.UnlockBits(data);
            }
        }

        public void Draw()
        {
            GL.Viewport(0, 0, program.ClientSize.Width, program.ClientSize.Height);
            //GL.Viewport(0, 0, 800, 600);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.Disable(EnableCap.DepthTest);
            //GL.Disable(EnableCap.DepthTest);
            //GL.DepthMask(false);

            GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Lighting);
            GL.Color4(1, 0, 0, 0.5f);
            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.OneMinusDstColor, BlendingFactorDest.OneMinusSrcColor);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,(int)All.Decal);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Replace);
            //GL.Disable(EnableCap.DepthTest);
            //GL.BindTexture(TextureTarget.Texture2D, (int)DS_TEXTURE_NUMBER.DS_CHECKERED+1);


            // Draw a textured quad
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(0, 0, 0.5f);
            GL.TexCoord2(0, 1); GL.Vertex3(0, program.ClientSize.Height, 0.5f);
            GL.TexCoord2(1, 1); GL.Vertex3(program.ClientSize.Width, program.ClientSize.Height, 0.5f);
            GL.TexCoord2(1, 0); GL.Vertex3(program.ClientSize.Width, 0, 0.5f);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void finalize()
        {
            //TextFont.Dispose();
            //fonts.Dispose();
            //family.Dispose();
            //GL.DeleteTextures(0, ref _textureId);
        }

        /*public void Draw()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            
            //GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);


            GL.BindTexture(TextureTarget.Texture2D, (int)DS_TEXTURE_NUMBER.DS_WOOD);

            GL.Disable(EnableCap.Lighting);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-100, 100, -100, 100,-1,1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            //GL.Color3(1, 1, 1);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(20.0f, 20.0f, 0.0f);
            GL.TexCoord2(1, 0); GL.Vertex3(20.0f, -20.0f, 0.0f);
            GL.TexCoord2(1, 1); GL.Vertex3(-20.0f, -20.0f, 0.0f);
            GL.TexCoord2(0, 1); GL.Vertex3(-20.0f, 20.0f, 0.0f);

            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }*/
        /*public void Draw()
        {
            GL.PushMatrix();
            GL.LoadIdentity();
            
            Matrix4 ortho_projection = Matrix4.CreateOrthographicOffCenter(0, _clientSize.Width, _clientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            
            ////////////////TUTUTUTUTU
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Cw);
            //////////////////////////
            
            GL.PushMatrix();//
            GL.LoadMatrix(ref ortho_projection);
            


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.DstAlpha);
            //GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.ConstantColor);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            
            //GL.Disable(EnableCap.DepthTest);
            
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0); GL.Vertex2(TextBitmap.Width, 0);
            GL.TexCoord2(1, 1); GL.Vertex2(TextBitmap.Width, TextBitmap.Height);
            GL.TexCoord2(0, 1); GL.Vertex2(0, TextBitmap.Height);
            GL.End();
            GL.PopMatrix();
            
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }*/
    }
}

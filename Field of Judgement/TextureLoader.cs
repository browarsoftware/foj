/*  
    Field Of Judgement, Copyright (C) 2014 Tomasz Hachaj, Ph.D
    The source code is under MIT license. 
    All dlls are under licenses provided by theirs copyright holders.

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
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using FieldOfJudgement;

namespace FieldOfJudgement
{
    public class TextureLoader
    {
        public bool isLoaded = false;
        public int tex = 0;
        public byte[] pixels = null;
        public bool dirty = true;
        FieldOfJudgementGame program = null;
        public TextureLoader(FieldOfJudgementGame program, int position)
        {
            this.position = position;
            this.program = program;
            pixels = new byte[640 * 480 * 4];
            for (int a = 0; a < 640; a++)
                for (int b = 0; b < 480; b++)
                {
                    pixels[(a * 480 + b) * 4] = 240;
                    pixels[(a * 480 + b) * 4 + 1] = 0;
                    pixels[(a * 480 + b) * 4 + 2] = 0;
                    pixels[(a * 480 + b) * 4 + 3] = 255;
                }
        }
        public int originalWidth = 640;
        public int originalHeight = 480;
        public static int LoadTextureFromFile(Bitmap bitmap)
        {
            int tex = -1;
            if (bitmap == null)
                return -1;

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            return tex;
        }

        public void LoadTextureFromBitmap(Bitmap bitmap)
        {
            originalWidth = bitmap.Width;
            originalHeight = bitmap.Height;

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            if (!isLoaded)
            {
                GL.GenTextures(1, out tex);
                isLoaded = true;
            }
            for (int a = 0; a < bitmap.Width; a++)
                for (int b = 0; b < bitmap.Height; b++)
                    if (bitmap.GetPixel(a,b).A < 1)
                        bitmap.SetPixel(a, b, Color.FromArgb(0, 255, 255, 255));
            GL.BindTexture(TextureTarget.Texture2D, tex);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);



            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
        }

        public int LoadTexture(Bitmap bitmap)
        {
            if (!dirty)
                return 0;
            dirty = false;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            if (!isLoaded)
            {
                GL.GenTextures(1, out tex);
                isLoaded = true;
            }
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 640, 480, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            return tex;
        }

        public int LoadTexture()
        {
            if (!dirty)
                return 0;
            dirty = false;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            if (!isLoaded)
            {
                GL.GenTextures(1, out tex);
                isLoaded = true;
            }
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 640, 480, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            return tex;
        }


        public void DrawImageWithTransparency(float scaleParameter, float widthOffset, float heightOffset)
        {
            GL.Viewport(0, 0, program.Width, program.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);


            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Dither);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Replace);


            GL.DepthMask(false);

            GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();


            GL.Color3(1, 1, 1);



            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.Begin(BeginMode.Quads);

            int newWidth = (int)((double)program.ClientSize.Width * (double)scaleParameter);
            int newHeight = (int)((double)newWidth / (double)originalWidth * (double)originalHeight);

            GL.TexCoord2(0, 1); GL.Vertex2(0 + widthOffset, newHeight + heightOffset);
            GL.TexCoord2(1, 1); GL.Vertex2(newWidth + widthOffset, newHeight + heightOffset);
            GL.TexCoord2(1, 0); GL.Vertex2(newWidth + widthOffset, 0 + heightOffset);
            GL.TexCoord2(0, 0); GL.Vertex2(0 + widthOffset, 0 + heightOffset);
            GL.End();


            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }


        public void DrawImageWithTransparency(float positionWidth, float positionHeight, float sizeWidth, float sizeHeight)
        {
            GL.Viewport(0, 0, program.Width, program.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);


            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Dither);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Replace);

            GL.DepthMask(false);

            GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            
            GL.Color3(1, 1, 1);
            


            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.Begin(BeginMode.Quads);
            //right
            float widthP = program.Width;
            float heightP = program.Height;
            float width = widthP * sizeWidth;
            float height = heightP * sizeHeight;

            GL.TexCoord2(0, 1); GL.Vertex2(positionWidth * widthP - (width / 2.0f), positionHeight * heightP + (height / 2.0f));
            GL.TexCoord2(1, 1); GL.Vertex2(positionWidth * widthP + (width / 2.0f), positionHeight * heightP + (height / 2.0f));
            GL.TexCoord2(1, 0); GL.Vertex2(positionWidth * widthP + (width / 2.0f), positionHeight * heightP - (height / 2.0f));
            GL.TexCoord2(0, 0); GL.Vertex2(positionWidth * widthP - (width / 2.0f), positionHeight * heightP - (height / 2.0f));
           
            GL.End();

            
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }
        public void DrawImageWithTransparency3()
        {
            GL.Viewport(0, 0, program.Width, program.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            
            
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            
            GL.DepthMask(false);

            GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Lighting);
            GL.Color3(1, 1, 1);
            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Blend);


            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Decal);

            // Draw a textured quad
            GL.Begin(BeginMode.Quads);
            //right
            float widthP = program.Width;
            float heightP = program.Height;
            float width = widthP / 5.0f;
            float height = heightP / 5.0f;



            if (position == 0)
            {
                width = widthP - width;
                height = heightP - height;

            }
            if (position == 1)
            {
                width = (widthP / 2.0f) - width;
                height = heightP - height;
                widthP /= 2.0f;
            }


            GL.TexCoord2(1, 0); GL.Vertex2(widthP, height);
            GL.TexCoord2(0, 0); GL.Vertex2(width, height);
            GL.TexCoord2(0, 1); GL.Vertex2(width, heightP);
            GL.TexCoord2(1, 1); GL.Vertex2(widthP, heightP);

            GL.End();

            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void DrawImage()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.PushAttrib(AttribMask.EnableBit);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);

            GL.PopAttrib();
            DrawCube();
        }

        private int position = 0;
        private void DrawCube()
        {
            GL.BindTexture(TextureTarget.Texture2D, tex);
            float width = program.Width / 5.0f;
            float height = program.Height / 5.0f;

            GL.Begin(BeginMode.Quads);
            if (position == 0)
            {
                GL.TexCoord2(1, 0); GL.Vertex2(width, 0);
                GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
                GL.TexCoord2(0, 1); GL.Vertex2(0, height);
                GL.TexCoord2(1, 1); GL.Vertex2(width, height);
            }
            GL.End();
        }

        public void Draw()
        {
            GL.Viewport(0, 0, program.Width, program.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.Disable(EnableCap.DepthTest);
            
            GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Lighting);
            GL.Color3(1, 1, 1);
            GL.Enable(EnableCap.Texture2D);

            GL.Disable(EnableCap.Blend);

            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,(int)All.Decal);

            // Draw a textured quad
            GL.Begin(BeginMode.Quads);
            //right
            float widthP = program.Width;
            float heightP = program.Height;
            float width = widthP / 5.0f;
            float height = heightP / 5.0f;


            
            if (position == 0)
            {
                width = widthP - width;
                height = heightP - height;
                
            }
            if (position == 1)
            {
                width = (widthP / 2.0f) - width;
                height = heightP - height;
                widthP /= 2.0f;
            }

            
            GL.TexCoord2(1, 0); GL.Vertex2(widthP, height);
            GL.TexCoord2(0, 0); GL.Vertex2(width, height);
            GL.TexCoord2(0, 1); GL.Vertex2(width, heightP);
            GL.TexCoord2(1, 1); GL.Vertex2(widthP, heightP);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void DrawHorizontal()
        {
            GL.Viewport(0, 0, program.Width, program.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.Disable(EnableCap.DepthTest);
            GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Lighting);
            GL.Color3(1, 1, 1);
            GL.Enable(EnableCap.Texture2D);

            GL.Disable(EnableCap.Blend);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Decal);

            // Draw a textured quad
            GL.Begin(BeginMode.Quads);
            //right
            float widthP = program.Width;
            float heightP = program.Height;
            float width = widthP / 5.0f;
            float height = heightP / 5.0f;



            if (position == 0)
            {
                width = widthP - width;
                height = heightP - height;
            }
            if (position == 1)
            {
                width = widthP - width;

                height = (heightP / 2.0f) - height;
                heightP /= 2.0f;

                
            }


            GL.TexCoord2(1, 0); GL.Vertex2(widthP, height);
            GL.TexCoord2(0, 0); GL.Vertex2(width, height);
            GL.TexCoord2(0, 1); GL.Vertex2(width, heightP);
            GL.TexCoord2(1, 1); GL.Vertex2(widthP, heightP);
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
            GL.DeleteTextures(0, ref tex);
        }
    }
}

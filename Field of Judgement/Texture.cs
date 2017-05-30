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
using System.IO;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;

namespace FieldOfJudgement
{
    /**
     * Texture handling class, file also has enum for texture's name.
     */
    public class Texture
    {
        public int name;

        public Texture(Stream stm, bool mirrored = false)
        {
            CreateFromBitmap(new Bitmap(stm), mirrored);
        }
        public Texture(String filename, bool mirrored = false)
        {
            CreateFromBitmap(new Bitmap(filename), mirrored);
        }
        public Texture(Bitmap bmp, bool mirrored = false)
        {
            CreateFromBitmap(bmp, mirrored);
        }
        void CreateFromBitmap(Bitmap image, bool mirrored)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            GL.GenTextures(1, out name);
            GL.BindTexture(TextureTarget.Texture2D, name);

            // set pixel unpacking mode
            GL.PixelStore(PixelStoreParameter.UnpackSwapBytes, 0);
            GL.PixelStore(PixelStoreParameter.PackRowLength, 0);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);

            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Requieres OpenGL >= 1.4
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1); // 1 = True
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            if (mirrored)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.MirroredRepeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.MirroredRepeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Decal);

            image = null;
            // Unbind
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }


        internal void finalize()
        {
            GL.DeleteTextures(0, ref name);
        }

        public void bind(bool modulate)
        {
            GL.BindTexture(TextureTarget.Texture2D, name);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                      modulate ? (int)All.Modulate : (int)All.Decal);
        }

    }

    /* texture numbers */
    public enum DS_TEXTURE_NUMBER
    {
        DS_NONE, // = 0,       /* uses the current color instead of a texture */
        DS_WOOD,
        DS_CHECKERED,
        DS_SKY,
        DS_GRASS,
        DS_REFLECTINGWALL,
        DS_FORCEFIELD,
        DS_MAGICMISSLE,
        DS_FIREBALL,
        DS_FIREBALL_CORE,
        DS_BRICKWALL,
        DS_LAVA,
        DS_TREE,
        DS_FLOWERS,
        DS_WHITE,
        DS_MOSSFLOOR,
        DS_MOSSFLOOR2,
        DS_STONEWALL,
        DS_FLESH,
        DS_MEDIEVAL,
        DS_MEDIEVAL2,
        DS_MEDIEVAL3,
        DS_MEDIEVAL4,
        DS_STARTSCREEN
    }
}

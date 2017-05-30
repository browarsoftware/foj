using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace FieldOfJudgement
{
    public class RedScreen
    {
        FieldOfJudgementGame program = null;
        public RedScreen(FieldOfJudgementGame program)
        {
            this.program = program;
        }
        public void Draw(float modulate, bool isVertical, bool isFirst)
        {
            float viewportWidth = 0;
            float viewportHeight = 0;
            float viewportWidthStart = 0;
            float viewportHeightStart = 0;
            if (isVertical)
            {
                if (isFirst)
                {
                    viewportWidth = program.ClientSize.Width / 2.0f;
                    viewportHeight = program.ClientSize.Height;
                    viewportWidthStart = 0;
                    viewportHeightStart = 0;

                }
                else
                {
                    viewportWidthStart = program.ClientSize.Width / 2.0f;
                    viewportHeightStart = 0;
                    viewportWidth = program.ClientSize.Width;
                    viewportHeight = program.ClientSize.Height;
                }
            }
            else
            {
                if (isFirst)
                {
                    viewportWidthStart = 0;
                    viewportHeightStart = program.ClientSize.Height / 2.0f;
                    viewportWidth = program.ClientSize.Width;
                    viewportHeight = program.ClientSize.Height;
                }
                else
                {
                    viewportWidth = program.ClientSize.Width;
                    viewportHeight = program.ClientSize.Height / 2.0f;
                    viewportWidthStart = 0;
                    viewportHeightStart = 0;
                }
            }
            GL.Viewport((int)viewportWidthStart, (int)viewportHeightStart, (int)viewportWidth, (int)viewportHeight);
            //GL.Viewport(0, 0, program.ClientSize.Width, program.ClientSize.Height);
            //GL.Viewport(0, 0, 800, 600);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.Disable(EnableCap.DepthTest);
            //GL.Disable(EnableCap.DepthTest);
            //GL.DepthMask(false);

            GL.Ortho(viewportWidthStart, viewportWidth, viewportHeight, viewportWidthStart, -1, 1);
            //GL.Ortho(0, program.ClientSize.Width, program.ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Disable(EnableCap.Lighting);
            
            GL.Color4(1, 0, 0, modulate);
            //program.dsSetTexture(DS_TEXTURE_NUMBER.DS_WHITE);
            
            
            //program.dsSetColorAlpha(1, 0, 0, modulate);
            
            //program.setupDrawingMode();
            //GL.Color4(1, 0, 0, 0.4f);
            //GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.OneMinusDstColor, BlendingFactorDest.OneMinusSrcColor);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2D, program.texture[(int)DS_TEXTURE_NUMBER.DS_WHITE].name);
            //program.texture[(int)DS_TEXTURE_NUMBER.DS_WHITE].bind(false);
            //GL.BindTexture(TextureTarget.Texture2D, program.texture[(int)DS_TEXTURE_NUMBER.DS_WHITE]);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,(int)All.Decal);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Modulate);
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
    }
}

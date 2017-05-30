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
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using OpenTK.Graphics.OpenGL;
using FieldOfJudgement.Magic;

namespace FieldOfJudgement.Enviroment
{
    /**
     *This class is used in rendering loop of the game. It is rendered as rigid body block
     */ 
    class ReflectingWall : RenderableRigidBody
    {
        MagicBullet mb = null;

        public ReflectingWall(JVector position, JVector size)
            : base(new BoxShape(size))
        {
            textureHandler = DS_TEXTURE_NUMBER.DS_REFLECTINGWALL;
            BoxShape shape = new BoxShape(size);
            this.Position = position;
            IsStatic = true;
            Material.Restitution = 5f;
            IsActive = false;
            Mass = 100;
            mb = new MagicBullet(0,0.5f,0,0,0,false,false, false, 0);
            Random random = new Random((int)this.Position.X);
            mb.disortTextureFactor1Offset = (float)random.NextDouble();
            mb.disortTextureFactor2Offset = (float)random.NextDouble();
            mb.disortTextureFactor3Offset = (float)random.NextDouble();

            mb.disortTextureFactor1OffsetUp = (random.Next(0, 2) > 0) ? true : false;
            mb.disortTextureFactor2OffsetUp = (random.Next(0, 2) > 0) ? true : false;
            mb.disortTextureFactor3OffsetUp = (random.Next(0, 2) > 0) ? true : false;
        }
        public override void Render(double time, FieldOfJudgementGame program, float iterator)
        {
            program.dsSetColor(1f, 1f, 1);
            bool oldLighting = program.use_lighting;
            program.use_lighting = false;
            
            program.dsDrawSphere(Position, JMatrix.Identity, 0.5f, time, mb);

            program.use_lighting = oldLighting;
            float helpSize = 0.3f;
            float halfSize = 0.5f - (helpSize / 2.0f);
            JVector size = new JVector(helpSize, helpSize, helpSize);

            program.dsSetTexture(DS_TEXTURE_NUMBER.DS_FLOWERS);
            JVector position = new JVector(halfSize, halfSize, halfSize);
            program.dsDrawBox(Position + position, Orientation, size);
            position = new JVector(halfSize, -halfSize, halfSize);
            program.dsDrawBox(Position + position, Orientation, size);
            position = new JVector(halfSize, - halfSize, -halfSize);
            program.dsDrawBox(Position + position, Orientation, size);
            position = new JVector(halfSize, halfSize, - halfSize);
            program.dsDrawBox(Position + position, Orientation, size);

            position = new JVector(-halfSize, halfSize, halfSize);
            program.dsDrawBox(Position + position, Orientation, size);

            position = new JVector(-halfSize, -halfSize, halfSize);
            program.dsDrawBox(Position + position, Orientation, size);

            position = new JVector(-halfSize, -halfSize, -halfSize);
            program.dsDrawBox(Position + position, Orientation, size);

            position = new JVector(-halfSize, halfSize, -halfSize);
            program.dsDrawBox(Position + position, Orientation, size);
        }
    }
}

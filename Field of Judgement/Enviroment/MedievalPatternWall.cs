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
using FieldOfJudgement.Sound;

namespace FieldOfJudgement.Enviroment
{
    /**
     *This class is used in rendering loop of the game. It is rendered as rigid body block
     */ 
    class MedievalPatternWall : RenderableRigidBody
    {
        public MedievalPatternWall(JVector position, JVector size)
            : base(new BoxShape(size))
        {
            Random random = new Random((int)(position.X + position.Y + position.Z));
            int r = random.Next(4);
            if (r == 0)
                textureHandler = DS_TEXTURE_NUMBER.DS_MEDIEVAL;
            if (r == 1)
                textureHandler = DS_TEXTURE_NUMBER.DS_MEDIEVAL2;
            if (r == 2)
                textureHandler = DS_TEXTURE_NUMBER.DS_MEDIEVAL3;
            if (r == 3)
                textureHandler = DS_TEXTURE_NUMBER.DS_MEDIEVAL4;
            BoxShape shape = new BoxShape(size);
            this.Position = position;
            IsStatic = false;
            Material.Restitution = 0;
            LinearVelocity = JVector.Zero;
            IsActive = false;
            Mass = 10;
        }
        public override void Render(double time, FieldOfJudgementGame program, float iterator)
        {
            if (LinearVelocity.Length() > 2f)
            {
                if (WasStatic == true)
                {
                    program.PlaySound(SoundType.Hit);
                }
                WasStatic = false;
            }
            else
                WasStatic = true;
            float helpIterator = iterator % 512;
            if (helpIterator % 512 > 256)
            {
                helpIterator = 512 - helpIterator;
            }
            if (IsActive) program.dsSetColor(1, 1 - (helpIterator / 511f), 1 - (helpIterator / 511f));
            else program.dsSetColor(1f, 1f, 1);
            program.dsDrawBox(Position, Orientation, ((BoxShape)Shape).Size);
        }
    }
}

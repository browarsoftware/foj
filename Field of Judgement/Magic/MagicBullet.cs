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
using Jitter.Dynamics;
using OpenTK;
using Jitter.Collision.Shapes;
using FieldOfJudgement.Enviroment;

namespace FieldOfJudgement.Magic
{
    /**
     *This class is used in rendering loop of the game. Classes from Magic namespace inherit from this class.
     */
    public class MagicBullet : RenderableRigidBody
    {
        public int OwnerId = 0;
        public float Power = 100;
        public float MinimalSpeed = 0.01f;
        public bool VanishAfterCollisionWithAnyObject = false;
        public bool VanishAfterCollisionWithPlayer = false;
        public bool DoNotHurtOwner = false;
        public double LastingTimeMiliseconds = 0;
        public float manaCost = 0;
        public Vector4 basicColor = new Vector4(1, 1, 1, 1);

        public bool usePlayerColorFactor = false;
        public float changeColorFactor = 0;
        public bool changeColorFactorUp = true;

        public float disortTextureFactor1 = 0.1f;
        public float disortTextureFactor2 = 0.1f;
        public float disortTextureFactor3 = 0.1f;

        public float disortTextureFactor1Offset = -1;
        public float disortTextureFactor2Offset = -1;
        public float disortTextureFactor3Offset = -1;

        public bool disortTextureFactor1OffsetUp = true;
        public bool disortTextureFactor2OffsetUp = true;
        public bool disortTextureFactor3OffsetUp = true;
        public bool spellEnded = false;

        public MagicBullet(int ownerId, float radius, float mass, float power, 
            float minimalSpeed, bool vanishAfterCollisionWithAnyObject,
            bool vanishAfterCollisionWithPlayer, bool doNotHurtOwner,
            double lastingTimeMiliseconds)
            : base(new SphereShape(radius))
        {
            mass = FieldOfJudgementGame.BulletMass;
            this.OwnerId = ownerId;
            this.Power = power;
            this.MinimalSpeed = minimalSpeed;
            this.VanishAfterCollisionWithAnyObject = vanishAfterCollisionWithAnyObject;
            this.VanishAfterCollisionWithPlayer = vanishAfterCollisionWithPlayer;
            this.DoNotHurtOwner = doNotHurtOwner;
            this.LastingTimeMiliseconds = lastingTimeMiliseconds;
        }

        public virtual void EndSpell(Player player)
        {
        }

        public override void Render(double time, FieldOfJudgementGame program, float iterator)
        {
            program.dsSetTexture(GetTexture());
            if (LinearVelocity.Length() < MinimalSpeed && MinimalSpeed >= 0)
                program.rbToRemove.Add(this);

            if (changeColorFactor != 0)
            {
                int r = program.random.Next(3);
                if (r == 0)
                {
                    if (changeColorFactorUp)
                    {
                        basicColor.X += (float)(changeColorFactor * elapsedTime);
                    }
                    else
                    {
                        basicColor.X -= (float)(changeColorFactor * elapsedTime);
                    }
                    if (basicColor.X > 1)
                    {
                        changeColorFactorUp = false;
                        basicColor.X = 1;
                    }
                    if (basicColor.X < 0)
                    {
                        changeColorFactorUp = true;
                        basicColor.X = 0;
                    }
                                    
                                    
                }
                if (r == 1)
                {
                    if (changeColorFactorUp)
                    {
                        basicColor.Y += (float)(changeColorFactor * elapsedTime);
                    }
                    else
                    {
                        basicColor.Y -= (float)(changeColorFactor * elapsedTime);
                    }
                    if (basicColor.Y > 1)
                    {
                        changeColorFactorUp = false;
                        basicColor.Y = 1;
                    }
                    if (basicColor.Y < 0)
                    {
                        changeColorFactorUp = true;
                        basicColor.Y = 0;
                    }
                }
                if (r == 2)
                {
                    if (changeColorFactorUp)
                    {
                        basicColor.Z += (float)(changeColorFactor * elapsedTime);
                    }
                    else
                    {
                        basicColor.Z -= (float)(changeColorFactor * elapsedTime);
                    }
                    if (basicColor.Z > 1)
                    {
                        changeColorFactorUp = false;
                        basicColor.Z = 1;
                    }
                    if (basicColor.Z < 0)
                    {
                        changeColorFactorUp = true;
                        basicColor.Z = 0;
                    }
                }
            }

            program.dsSetColorAlpha(basicColor.X,
                basicColor.Y,
                basicColor.Z,
                basicColor.W);

            LastingTimeMiliseconds -= time;
            if (LastingTimeMiliseconds < 0)
            {
                program.rbToRemove.Add(this);
                if (!spellEnded)
                {
                    EndSpell(program.players[OwnerId]);
                    spellEnded = true;
                }
            }

            if (!IsActive)
            {
                program.rbToRemove.Add(this);
            }
            if (LinearVelocity.Length() < 0)
                program.rbToRemove.Add(this);

            if (program.collisionTester.RaisePassedBroadphase(this, program.gameMap.Ground))
            {
                program.collisionTester.Detect(this, program.gameMap.Ground);
            }

            for (int a = 0; a < program.players.Length; a++)
            {
                if (program.collisionTester.RaisePassedBroadphase(this, program.players[a]))
                {
                    program.collisionTester.Detect(this, program.players[a]);
                }
            }

            bool oldLighting = program.use_lighting;
            program.use_lighting = false;
            program.dsDrawSphere(Position, Orientation, ((SphereShape)Shape).Radius - 0.1f, time, this);
            program.use_lighting = oldLighting;
        }
    }
}

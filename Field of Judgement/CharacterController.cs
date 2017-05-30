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
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Dynamics.Constraints;
using FieldOfJudgement.Magic;

namespace FieldOfJudgement
{
    /**
     *Overrides standard rigid body control of Jitter Physics. It used for players controlling.
     */
    public class CharacterController : Constraint
    {
        private const float JumpVelocity = 0.5f;

        private float feetPosition;

        public FieldOfJudgementGame program = null;

        public CharacterController(World world, RigidBody body, FieldOfJudgementGame program)
            : base(body, null)
        {
            this.World = world;
            this.program = program;
            // determine the position of the feets of the character
            // this can be done by supportmapping in the down direction.
            // (furthest point away in the down direction)
            JVector vec = new JVector(0,0,-1);
            JVector result = JVector.Zero;

            // Note: the following works just for normal shapes, for multishapes (compound for example)
            // you have to loop through all sub-shapes -> easy.
            body.Shape.SupportMapping(ref vec, out result);

            // feet position is now the distance between body.Position and the feets
            // Note: the following '*' is the dot product.
            //feetPosition = result * JVector.Down;
            feetPosition = result * new JVector(0, 0, -1);
        }

        public World World { private set; get; }
        public JVector TargetVelocity { get; set; }
        public bool TryJump { get; set; }
        public RigidBody BodyWalkingOn { get; set; }

        private JVector deltaVelocity = JVector.Zero;
        private bool shouldIJump = false;

        public override void PrepareForIteration(float timestep)
        {
            // send a ray from our feet position down.
            // if we collide with something which is 0.05f units below our feets remember this!

            RigidBody resultingBody = null;
            JVector normal; float frac;

            bool result = World.CollisionSystem.Raycast(Body1.Position + new JVector(0, 0, -1) * (feetPosition - 0.1f), new JVector(0, 0, -1), RaycastCallback,
                out resultingBody, out normal, out frac);

            BodyWalkingOn = (result && frac <= 0.2f) ? resultingBody : null;
            shouldIJump = (result && frac <= 0.2f && Body1.LinearVelocity.Z < JumpVelocity && TryJump);

        }

        private bool RaycastCallback(RigidBody body, JVector normal, float fraction)
        {
            // prevent the ray to collide with ourself!
            return (body != this.Body1);
        }

        public float rotate = 0;
        public float moveDirection = 0;

        

        public JVector Direction = new JVector();

        private JVector getDirections3(JMatrix orientation)
        {
            JVector transformed = JVector.Normalize(JVector.Transform(new JVector(1, 0, 0), orientation));
            float angleZ = (float)Math.Acos(JVector.Dot(new JVector(1, 0, 0), transformed)) * 2.0f;
            int signCross = Math.Sign(JVector.Cross(new JVector(1, 0, 0), transformed).Z);
            int signDot = Math.Sign(angleZ);
            if (signCross > 0)
            {
            }
            else if (signCross < 0)
            {
                angleZ *= -1.0f;
            }
            JVector returnVector = new JVector(0, 0, angleZ);
            return returnVector;
        }
        public bool resetAngular = false;
        public override void Iterate()
        {
            float deltaAngular = 2f * rotate -Body1.AngularVelocity.Z;
            if (deltaAngular != 0.0f)
            {
                Body1.AddTorque(new JVector(0, 0, deltaAngular));
            }
            //the object is not rolling now
            Body1.AngularVelocity = new JVector(0, 0, Body1.AngularVelocity.Z);
            if (resetAngular)
                Body1.AngularVelocity = JVector.Zero;
            Direction = getDirections3(Body1.Orientation);


            deltaVelocity = TargetVelocity - Body1.LinearVelocity;
            if (BodyWalkingOn == null) deltaVelocity *= 0.0001f;
            else deltaVelocity *= 0.02f;

            if (deltaVelocity.LengthSquared() != 0.0f)
            {
                // activate it, in case it fall asleep :)
                Body1.IsActive = true;
                Body1.ApplyImpulse(deltaVelocity * Body1.Mass);
            }
            if (shouldIJump)
            {
                Body1.IsActive = true;
                Body1.ApplyImpulse(JumpVelocity * new JVector(0,0,1) * Body1.Mass * 5f);

                System.Diagnostics.Debug.WriteLine("JUMP! " + DateTime.Now.Second.ToString());

                if (!BodyWalkingOn.IsStatic)
                {
                    BodyWalkingOn.IsActive = true;
                    // apply the negative impulse to the other body
                    BodyWalkingOn.ApplyImpulse(-1.0f * JumpVelocity * new JVector(0, 0, 1) * Body1.Mass * 5);
                }
            }
        }
    }
}

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
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using Jitter;
using FieldOfJudgement.Enviroment;
using System.Collections;

namespace FieldOfJudgement
{
    public class Player : RenderableRigidBody//RigidBody
    {
        public PlayerModel model = null;//new PlayerModel();

        public bool InsideForceField = false;
        public bool killed = false;

        public ArrayList alradyHitMe = new ArrayList();
        
        public float movementSpeed = 1;
        public float spellPower = 1;
        private float mana = 1;

        public float Mana
        {
            get { return mana; }
            set {
                dirtyRecord = true;
                mana = value; }
        }
        private float health = 100;

        private double redScreenDamagesElapsedTime = 0;
        private double redScreenDamagesElapsedMaxTime = 0.1;
        public float redScreenDamages = 0;
        public float Health
        {
            get { return health; }
            set {
                dirtyRecord = true;
                if (health > value)
                {
                    redScreenDamagesElapsedTime = redScreenDamagesElapsedMaxTime;
                    redScreenDamages += (health - value) / maxHealth;
                }
                health = value; }
        }
        public float maxMana = 100;
        public float maxHealth = 100;
        private int frags = 0;

        public bool dirtyRecord = true;
        public int Frags
        {
            get { return frags; }
            set {
                dirtyRecord = true;
                frags = value; }
        }
        public int armour = 0;
        public float knowledge = 1;
        public double healthRegreneration = 3;
        public double manaRegeneration = 1;
        private double healthRegrenerationElapsed = 0;
        private double manaRegenerationElapsed = 0;


        public int Id = 0;
        public float forward = 0;
        public float backrward = 0;
        //public float rotate = 0;
        public float left = 0;
        public float right = 0;
        public float jump = 0;

        public float lookUp = 0;
        public float lookDown = 0;
        public bool run = false;
        //public float up = 0;
        //public float down = 0;

        public JVector Color = new JVector(1, 1, 1);

        public int leftKey = (int)OpenTK.Input.Key.A;
        public int rightKey = (int)OpenTK.Input.Key.D;
        public int backrwardKey = (int)OpenTK.Input.Key.S;
        public int forwardKey = (int)OpenTK.Input.Key.W;
        public int jumpKey = (int)OpenTK.Input.Key.Z;
        public int shotKey = (int)OpenTK.Input.Key.Space;
        public int lookUpKey = (int)OpenTK.Input.Key.PageUp;
        public int lookDownKey = (int)OpenTK.Input.Key.PageDown;

        // current camera position and orientation
        public float[] view_xyz = new float[3];	// position x,y,z
        public float[] view_hpr = new float[3];	// heading, pitch, roll (degrees)

        public float HeadPosition = (30f * (0.16f / 4f) - 0.45f) - .1f;//+0.5f;

        public double lookingUpDownFactor = 0;

        public void RenderPlayer()
        {

        }

        public int castSpell = -1;

        public void CastSpell(int type)
        {
            castSpell = type;
            //program.ShootSphere(Id);
        }
        
        public void UpdateCamera(double time)
        {
            view_hpr[0] = characterController.Direction.Z * (float)(90 / Math.PI);
            view_hpr[1] = characterController.Direction.Y * (float)(90 / Math.PI);
            view_hpr[2] = characterController.Direction.X * (float)(90 / Math.PI);

            lookingUpDownFactor += (float)(time) * lookUp * 55.0f;
            if (lookingUpDownFactor > 90)
            {
                lookingUpDownFactor = (float)90;
            }
            lookingUpDownFactor += (float)(time) * lookDown * -55.0f;
            if (lookingUpDownFactor < -90)
            {
                lookingUpDownFactor = (float)(-90);
            }

            view_hpr[1] += (float)lookingUpDownFactor;

            view_xyz[0] = Position.X;
            view_xyz[1] = Position.Y;
            view_xyz[2] = Position.Z;
        }
        
        public void UpdateKeyboardInput()
        {
            JVector targetVelocity = JVector.Zero;
            if (left > FieldOfJudgementGame.eps)
            {
                characterController.rotate = 0.2f * movementSpeed * left;
            }
            else if (right > FieldOfJudgementGame.eps)
            {
                characterController.rotate = -0.2f * movementSpeed * right;
            }
            else
                characterController.rotate = 0;

            if (forward > FieldOfJudgementGame.eps)
            {
                targetVelocity = JVector.Transform(new JVector(1, 0, 0), Orientation) * forward;
            }
            if (backrward > FieldOfJudgementGame.eps)
            {
                targetVelocity = JVector.Multiply(JVector.Transform(new JVector(1, 0, 0), Orientation), -1f) * backrward;
            }
            if ((forward > FieldOfJudgementGame.eps | backrward > FieldOfJudgementGame.eps) && !(right > FieldOfJudgementGame.eps | left > FieldOfJudgementGame.eps))
            {
                characterController.resetAngular = true;
            }
            else
            {
                characterController.resetAngular = false;
            }
            targetVelocity *= 1.0f * movementSpeed;

            characterController.TargetVelocity = targetVelocity;
            if (jump > FieldOfJudgementGame.eps)
                characterController.TryJump = true;
            else
                characterController.TryJump = false;
        }
        bool firstRespownOnGameBegin = true;
        public void RespownPlayer(GameMap gm)
        {
            Health = maxHealth;
            Mana = maxMana;
            redScreenDamages = 0;
            redScreenDamagesElapsedTime = redScreenDamagesElapsedMaxTime;

            healthRegrenerationElapsed = healthRegreneration = 5;
            manaRegenerationElapsed = manaRegeneration;

            JVector oldPosition = JVector.Zero;
            JMatrix oldOrientation = JMatrix.Identity;
            if (!firstRespownOnGameBegin)
            {
                oldPosition = Position;
                oldOrientation = Orientation;
            }

            LinearVelocity = JVector.Zero;
            AngularVelocity = JVector.Zero;
            float randx = (float)(-2.0 * program.random.NextDouble() + 1f);
            float randy = (float)(-2.0 * program.random.NextDouble() + 1f);
            Position = new JVector((float)(randx * gm.mapWidth / 2f),
                (float)(randy * gm.mapHeight / 2f),
                2);
            //Position = new JVector(2, 2, 2);
            if (Position.X < -gm.mapWidth / 2.0 + 1f)
                Position = new JVector(-gm.mapWidth / 2.0f + 2f, Position.Y, Position.Z);
            if (Position.Y < -gm.mapWidth / 2.0 + 1f)
                Position = new JVector(Position.X, -gm.mapWidth / 2.0f + 2f, Position.Z);

            if (Position.X > gm.mapWidth / 2.0 - 2f)
                Position = new JVector(gm.mapWidth / 2.0f - 2f, Position.Y, Position.Z);
            if (Position.Y > gm.mapWidth / 2.0 - 2f)
                Position = new JVector(Position.X, gm.mapWidth / 2.0f - 2f, Position.Z);


            program.world.RemoveConstraint(characterController);
            characterController = new CharacterController(program.world, this, program);
            program.world.AddConstraint(characterController);

            Orientation = JMatrix.CreateRotationZ((float)(2.0 * Math.PI * program.random.NextDouble()));
            try
            {
                program.world.AddBody(this);
            }
            catch
            { }
            killed = false;
            if (!firstRespownOnGameBegin)
            {
                if (program.goreEffects)
                    AddDeathBody(program, oldOrientation, oldPosition);
            }
            else
            {
                firstRespownOnGameBegin = false;
            }
        }

        private FieldOfJudgementGame program = null;
        public bool ToNear = false;
        public int SkeletonArrayId = -1;

        public bool IsInsideForceField = false;
        public JVector IsInsideForceFieldPosition = JVector.Zero;
        public JMatrix IsInsideForceFieldOrientation = JMatrix.Identity;

        public CharacterController characterController = null;
        public Player(World world, JVector position, JVector color, FieldOfJudgementGame program)
            :base(new CapsuleShape(1.0f, .5f))
        {
            //RigidBody rb = new RigidBody(new CapsuleShape(1.0f, 0.5f));
            this.Color = color;
            this.program = program;
            Material.Restitution = 0;
            Position = position;
            //rb.Mass = 0.00001f;
            IsStatic = false;
            characterController = new CharacterController(world, this, program);
            world.AddBody(this);
            world.AddConstraint(characterController);
            //body.Material.Restitution = 0;
            //body.Material.KineticFriction = 1;
            //body.Material.StaticFriction = 1;
            //body.Material.
            //body.LinearVelocity = velocity;
            //IsActive = false;

            //rb. UseUserMassProperties(JMatrix.Zero, 1.0f, true);
            //Restitution = 0.0f;
        }

        public void AddDeathBody(FieldOfJudgementGame program, JMatrix Orientation, JVector Position)
        {
            model.AddDeathBody(program, Orientation, Position);
        }

        public override void Render(double time, FieldOfJudgementGame program, float iterator)
        {
            if (redScreenDamages > 0)
            {
                redScreenDamagesElapsedTime -= time;
                if (redScreenDamagesElapsedTime < 0)
                {
                    redScreenDamages -= 0.01f;
                    redScreenDamagesElapsedTime = redScreenDamagesElapsedMaxTime;
                    if (redScreenDamages < 0)
                        redScreenDamages = 0;
                }
            }
            IsActive = true;
            if (mana < maxMana)
            {
                //healthRegrenerationElapsed = healthRegreneration = 5;
                //manaRegenerationElapsed = manaRegeneration;
                if (manaRegenerationElapsed < 0)
                {
                    Mana++;
                    manaRegenerationElapsed = manaRegeneration;
                }
                manaRegenerationElapsed -= time;
            }

            if (health < maxHealth)
            {
                //healthRegrenerationElapsed = healthRegreneration = 5;
                //manaRegenerationElapsed = manaRegeneration;
                if (healthRegrenerationElapsed < 0)
                {
                    Health++;
                    healthRegrenerationElapsed = healthRegreneration;
                }
                healthRegrenerationElapsed -= time;
            }

            model.DrawModel(program, Orientation, Position);
        }
    }
}

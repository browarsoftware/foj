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
using Jitter.Dynamics;

namespace FieldOfJudgement.Magic
{
    /**
     *This class is spells classes factory.
     */ 
    public class SpellFactory
    {
        private static float ForceFieldMana = 10;
        private static float FireballMana = 4;
        private static float MagicMissleMana = 5;

        public static Fireabll[] CreateFireball(Player player, double gameTime)
        {
            if (player.Mana < FireballMana)
                return null;
            player.Mana -= FireballMana;

            Fireabll[] bl = null;
            bl = new Fireabll[1];

            JVector pos, ang;
            pos = player.Position;
            ang = player.characterController.Direction;

            JVector unit;
            unit.X = (float)Math.Cos(ang.Z / 2.0);
            unit.Y = (float)Math.Sin(ang.Z / 2.0);
            unit.Z = (float)(Math.PI * player.lookingUpDownFactor / 180.0);

            JVector forward = new JVector(0.5f, 0, 0);
            forward = JVector.Transform(forward, player.Orientation);

            bl[0] = new Fireabll(player.Id, 1, 50, 50, 1f, false, true, false, 5.0);


            bl[0].LinearVelocity = unit * 50.0f;
            bl[0].Position = new JVector(pos.X + unit.X, pos.Y + unit.Y, pos.Z + 1f) + forward;
            for (int a = 0; a < bl[0].previousPosition.Length; a++)
                bl[0].previousPosition[a] = bl[0].Position;

            bl[0].disortTextureFactor1Offset = (float)random.NextDouble();
            bl[0].disortTextureFactor2Offset = (float)random.NextDouble();
            bl[0].disortTextureFactor3Offset = (float)random.NextDouble();

            bl[0].disortTextureFactor1OffsetUp = (random.Next(0, 2) > 0) ? true : false;
            bl[0].disortTextureFactor2OffsetUp = (random.Next(0, 2) > 0) ? true : false;
            bl[0].disortTextureFactor3OffsetUp = (random.Next(0, 2) > 0) ? true : false;

            return bl;
        }
        public static Random random = new Random();
        public static MagicMissile[] CreateMagicMissle(Player player, double gameTime)
        {
            if (player.Mana < MagicMissleMana)
                return null;
            player.Mana -= MagicMissleMana;

            MagicMissile[] mm = null;

            JVector pos, ang;
            pos = player.Position;
            ang = player.characterController.Direction;

            JVector unit;
            unit.X = (float)Math.Cos(ang.Z / 2.0);
            unit.Y = (float)Math.Sin(ang.Z / 2.0);
            unit.Z = (float)(Math.PI * player.lookingUpDownFactor / 180.0);


            int maxIts = 10;
            mm = new MagicMissile[maxIts];
            for (int a = 0; a < maxIts; a++)
            {
                mm[a] = new MagicMissile(player.Id, 0.50f, 0.1f, 15, 0.01f, true, true, true, 10.0);
                mm[a].LinearVelocity = unit * (40.0f + (float)random.Next(-25,25));
                unit += new JVector((float)(random.Next(-1000, 1000) / 8000.0),
                        (float)(random.Next(-1000, 1000) / 8000.0),
                        (float)(random.Next(-1000, 1000) / 8000.0));
                mm[a].Position = new JVector(pos.X + unit.X, pos.Y + unit.Y, pos.Z + 1);

                mm[a].disortTextureFactor1Offset = 0;
                mm[a].disortTextureFactor2Offset = 0;
                mm[a].disortTextureFactor3Offset = 0;
                
                mm[a].changeColorFactor = .1f;

                float r = Saturate((float)(0.75 * random.NextDouble()));
                float g = Saturate((float)(0.75 * random.NextDouble()));
                float b = Saturate((float)(0.75 * random.NextDouble()));
                int ran = random.Next(3);
                if (ran == 0)
                    r = 1;
                if (ran == 1)
                    g = 1;
                if (ran == 2)
                    b = 1;
                mm[a].basicColor = new OpenTK.Vector4(
                    r,
                    g,
                    b, 
                    1.0f - (float)(random.NextDouble() / 2.0f));
            }
            return mm;
        }

        private static float Saturate(float value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        public static ForceField[] CreateForceField(Player player, double gameTime)
        {
            if (player.Mana < ForceFieldMana)
                return null;
            player.Mana -= ForceFieldMana;

            ForceField[] ff = null;
            JVector pos, ang;
            pos = player.Position;
            ang = player.characterController.Direction;

            JVector unit;
            unit.X = (float)Math.Cos(ang.Z / 2.0);
            unit.Y = (float)Math.Sin(ang.Z / 2.0);
            unit.Z = (float)(Math.PI * player.lookingUpDownFactor / 180.0);

            ff = new ForceField[1];
            ff[0] = new ForceField(player.Id, 1.5f, 10000f, 99, -1, false, false, true, 5.0);
            ff[0].Position = new JVector(pos.X, pos.Y, pos.Z + 0.5f);
            ff[0].IsStatic = true;

            ff[0].disortTextureFactor1Offset = (float)random.NextDouble();
            ff[0].disortTextureFactor2Offset = (float)random.NextDouble();
            ff[0].disortTextureFactor3Offset = (float)random.NextDouble();

            ff[0].disortTextureFactor1OffsetUp = (random.Next(0, 2) > 0) ? true : false;
            ff[0].disortTextureFactor2OffsetUp = (random.Next(0, 2) > 0) ? true : false;
            ff[0].disortTextureFactor3OffsetUp = (random.Next(0, 2) > 0) ? true: false;

            return ff;
        }
    }
}

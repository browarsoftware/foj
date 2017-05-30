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
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using OpenTK;

namespace FieldOfJudgement
{
    public class PlayerModel
    {
        public Matrix4 JMatrixToMatrix4(JMatrix matrix)
        {
            Matrix4 helpm = Matrix4.Identity;
            helpm.M11 = matrix.M11;
            helpm.M12 = matrix.M12;
            helpm.M13 = matrix.M13;
            helpm.M21 = matrix.M21;
            helpm.M22 = matrix.M22;
            helpm.M23 = matrix.M23;
            helpm.M31 = matrix.M31;
            helpm.M32 = matrix.M32;
            helpm.M33 = matrix.M33;
            return helpm;
        }
        public PlayerModelBlock chest = new PlayerModelBlock();
        public PlayerModelBlock belly = new PlayerModelBlock();
        public PlayerModelBlock hips = new PlayerModelBlock();

        public PlayerModelBlock rightShoulder = new PlayerModelBlock();
        public PlayerModelBlock rightElbow = new PlayerModelBlock();
        public PlayerModelBlock rightHand = new PlayerModelBlock();
        public PlayerModelBlock leftShoulder = new PlayerModelBlock();
        public PlayerModelBlock leftElbow = new PlayerModelBlock();
        public PlayerModelBlock leftHand = new PlayerModelBlock();

        public PlayerModelBlock rightHip = new PlayerModelBlock();
        public PlayerModelBlock rightKnee = new PlayerModelBlock();
        public PlayerModelBlock rightFoot = new PlayerModelBlock();

        public PlayerModelBlock leftHip = new PlayerModelBlock();
        public PlayerModelBlock leftKnee = new PlayerModelBlock();
        public PlayerModelBlock leftFoot = new PlayerModelBlock();

        public PlayerModelBlock head = new PlayerModelBlock();
        public PlayerModelBlock neck = new PlayerModelBlock();

        public PlayerModelBlock hat = new PlayerModelBlock();
        public PlayerModelBlock hatTop = new PlayerModelBlock();

        public PlayerModelBlock auraPosition = new PlayerModelBlock();

        public PlayerModel()
        {
            float headSize = 0.16f;
            float quarterHead = headSize / 4.0f;
            float groundOffset = 0.45f;
            chest = new PlayerModelBlock();
            
            chest.scaling = new Vector3(1.5f * headSize, 2 * headSize, headSize);
            chest.translation = new Vector3(0, 0, 23f * quarterHead + chest.scaling.Z / 2f - groundOffset);

            chest.color[0] = 255;
            chest.color[1] = 255;
            chest.color[2] = 255;
            chest.color[3] = 255;

            belly = new PlayerModelBlock();
            belly.scaling = new Vector3(5f * quarterHead, 6f * quarterHead, 3f * quarterHead);
            belly.translation = new Vector3(0, 0, 20f * quarterHead + belly.scaling.Z / 2f - groundOffset);
            belly.color[0] = 255;
            belly.color[1] = 255;
            belly.color[2] = 255;
            belly.color[3] = 255;

            hips = new PlayerModelBlock();
            hips.scaling = new Vector3(1.5f * headSize, 2 * headSize, headSize);
            hips.translation = new Vector3(0, 0, 16 * quarterHead + hips.scaling.Z / 2f - groundOffset);
            hips.color[0] = 255;
            hips.color[1] = 255;
            hips.color[2] = 255;
            hips.color[3] = 255;

            //LEFT HAND
            leftShoulder = new PlayerModelBlock();
            leftShoulder.scaling = new Vector3(3f * quarterHead, 3f * quarterHead, 6 * quarterHead);
            leftShoulder.translation = new Vector3(0, 4f * quarterHead + leftShoulder.scaling.Y / 2f, 21f * quarterHead + leftShoulder.scaling.Z / 2f - groundOffset);

            leftShoulder.color[0] = 255;
            leftShoulder.color[1] = 255;
            leftShoulder.color[2] = 255;
            leftShoulder.color[3] = 255;

            leftElbow = new PlayerModelBlock();
            leftElbow.scaling = new Vector3(2f * quarterHead, 2f * quarterHead, 6 * quarterHead);
            leftElbow.translation = new Vector3(0, 4f * quarterHead + (leftElbow.scaling.Y / 2f + quarterHead / 2f), 15f * quarterHead + leftElbow.scaling.Z / 2f - groundOffset);

            leftElbow.color[0] = 255;
            leftElbow.color[1] = 255;
            leftElbow.color[2] = 255;
            leftElbow.color[3] = 255;

            leftHand = new PlayerModelBlock();
            leftHand.scaling = new Vector3(3f * quarterHead, 2f * quarterHead, 3 * quarterHead);
            leftHand.translation = new Vector3(0, 4f * quarterHead + (leftHand.scaling.Y / 2f + quarterHead / 2f), 12f * quarterHead + leftHand.scaling.Z / 2f - groundOffset);

            leftHand.color[0] = 255;
            leftHand.color[1] = 255;
            leftHand.color[2] = 255;
            leftHand.color[3] = 255;
            //RIGHT HAND
            rightShoulder = new PlayerModelBlock();
            rightShoulder.scaling = new Vector3(3f * quarterHead, 3f * quarterHead, 6 * quarterHead);
            rightShoulder.translation = new Vector3(0, -4f * quarterHead - rightShoulder.scaling.Y / 2f, 21f * quarterHead + rightShoulder.scaling.Z / 2f - groundOffset);

            rightShoulder.color[0] = 255;
            rightShoulder.color[1] = 255;
            rightShoulder.color[2] = 255;
            rightShoulder.color[3] = 255;

            rightElbow = new PlayerModelBlock();
            rightElbow.scaling = new Vector3(2f * quarterHead, 2f * quarterHead, 6 * quarterHead);
            rightElbow.translation = new Vector3(0, -4f * quarterHead - (rightElbow.scaling.Y / 2f + quarterHead / 2f), 15f * quarterHead + rightElbow.scaling.Z / 2f - groundOffset);

            rightElbow.color[0] = 255;
            rightElbow.color[1] = 255;
            rightElbow.color[2] = 255;
            rightElbow.color[3] = 255;

            rightHand = new PlayerModelBlock();
            rightHand.scaling = new Vector3(3f * quarterHead, 2f * quarterHead, 3 * quarterHead);
            rightHand.translation = new Vector3(0, -4f * quarterHead - (rightHand.scaling.Y / 2f + quarterHead / 2f), 12f * quarterHead + rightHand.scaling.Z / 2f - groundOffset);

            rightHand.color[0] = 255;
            rightHand.color[1] = 255;
            rightHand.color[2] = 125550;
            rightHand.color[3] = 255;
            //RIGHT LEG

            rightHip = new PlayerModelBlock();
            rightHip.scaling = new Vector3(3f * quarterHead, 3f * quarterHead, 8f * quarterHead);
            rightHip.translation = new Vector3(0, -2 * quarterHead, 8f * quarterHead + rightHip.scaling.Z / 2f - groundOffset);
            rightHip.color[0] = 255;
            rightHip.color[1] = 255;
            rightHip.color[2] = 255;
            rightHip.color[3] = 255;

            rightKnee.scaling = new Vector3(2.5f * quarterHead, 2.5f * quarterHead, 7f * quarterHead);
            rightKnee.translation = new Vector3(0, -2 * quarterHead - 0.125f * quarterHead, 1f * quarterHead + rightKnee.scaling.Z / 2f - groundOffset);
            rightKnee.color[0] = 255;
            rightKnee.color[1] = 255;
            rightKnee.color[2] = 255;
            rightKnee.color[3] = 255;

            rightFoot = new PlayerModelBlock();
            rightFoot.scaling = new Vector3(1.25f * headSize, 2.5f * quarterHead, quarterHead);
            rightFoot.translation = new Vector3(1.25f * quarterHead, -2 * quarterHead - 0.125f * quarterHead, rightFoot.scaling.Z / 2f - groundOffset);
            rightFoot.color[0] = 255;
            rightFoot.color[1] = 255;
            rightFoot.color[2] = 255;
            rightFoot.color[3] = 255;
            //LEFT LEG
            leftHip = new PlayerModelBlock();
            leftHip.scaling = new Vector3(3f * quarterHead, 3f * quarterHead, 8f * quarterHead);
            leftHip.translation = new Vector3(0, +2 * quarterHead, 8f * quarterHead + leftHip.scaling.Z / 2f - groundOffset);
            leftHip.color[0] = 255;
            leftHip.color[1] = 255;
            leftHip.color[2] = 255;
            leftHip.color[3] = 255;

            leftKnee.scaling = new Vector3(2.5f * quarterHead, 2.5f * quarterHead, 7f * quarterHead);
            leftKnee.translation = new Vector3(0, +2 * quarterHead + 0.125f * quarterHead, 1f * quarterHead + leftKnee.scaling.Z / 2f - groundOffset);
            leftKnee.color[0] = 255;
            leftKnee.color[1] = 255;
            leftKnee.color[2] = 255;
            leftKnee.color[3] = 255;

            leftFoot = new PlayerModelBlock();
            leftFoot.scaling = new Vector3(1.25f * headSize, 2.5f * quarterHead, quarterHead);
            leftFoot.translation = new Vector3(1.25f * quarterHead , +2 * quarterHead + 0.125f * quarterHead, leftFoot.scaling.Z / 2f - groundOffset);
            leftFoot.color[0] = 255;
            leftFoot.color[1] = 255;
            leftFoot.color[2] = 255;
            leftFoot.color[3] = 255;

            //HEAD ETC.
            neck = new PlayerModelBlock();
            neck.scaling = new Vector3(3 * quarterHead, 3 * quarterHead, quarterHead);
            neck.translation = new Vector3(0, 0, 27f * quarterHead + neck.scaling.Z / 2f - groundOffset);
            neck.color[0] = 255;
            neck.color[1] = 255;
            neck.color[2] = 255;
            neck.color[3] = 255;

            head = new PlayerModelBlock();
            head.scaling = new Vector3(headSize, headSize, headSize);
            head.translation = new Vector3(0, 0, 28f * quarterHead + head.scaling.Z / 2f - groundOffset);
            head.color[0] = 255;
            head.color[1] = 255;
            head.color[2] = 255;
            head.color[3] = 255;

            hat = new PlayerModelBlock();
            hat.scaling = new Vector3(2f * headSize, 2f * headSize, quarterHead);
            hat.translation = new Vector3(0, 0, 32 * quarterHead + hat.scaling.Z / 2f - groundOffset);
            hat.color[0] = 255;
            hat.color[1] = 255;
            hat.color[2] = 255;
            hat.color[3] = 255;

            hatTop = new PlayerModelBlock();
            hatTop.scaling = new Vector3(3f * quarterHead, 3f * quarterHead, 3f * quarterHead);
            hatTop.translation = new Vector3(0, 0, 33 * quarterHead + hatTop.scaling.Z / 2f - groundOffset);
            hatTop.color[0] = 255;
            hatTop.color[1] = 255;
            hatTop.color[2] = 255;
            hatTop.color[3] = 255;

            auraPosition = new PlayerModelBlock();
            auraPosition.scaling = new Vector3(quarterHead, quarterHead, 400 * quarterHead);
            auraPosition.translation = new Vector3(0, 0, 40 * quarterHead + 10 * quarterHead + auraPosition.scaling.Z / 2f - groundOffset);
            auraPosition.color[0] = 255;
            auraPosition.color[1] = 255;
            auraPosition.color[2] = 255;
            auraPosition.color[3] = 25;
            
            belly.FirstLevelChildren = new PlayerModelBlock[2];
            belly.FirstLevelChildren[0] = chest;
            belly.FirstLevelChildren[1] = hips;
            chest.Parent = belly;
            hips.Parent = belly;

            chest.FirstLevelChildren = new PlayerModelBlock[3];
            chest.FirstLevelChildren[0] = neck;
            neck.Parent = chest;
            chest.FirstLevelChildren[1] = rightShoulder;
            rightShoulder.Parent = chest;
            chest.FirstLevelChildren[2] = leftShoulder;
            leftShoulder.Parent = chest;

            leftShoulder.FirstLevelChildren = new PlayerModelBlock[1];
            leftShoulder.FirstLevelChildren[0] = leftElbow;
            leftElbow.Parent = leftShoulder;

            leftElbow.FirstLevelChildren = new PlayerModelBlock[1];
            leftElbow.FirstLevelChildren[0] = leftHand;
            leftHand.Parent = leftElbow;

            rightShoulder.FirstLevelChildren = new PlayerModelBlock[1];
            rightShoulder.FirstLevelChildren[0] = rightElbow;
            rightElbow.Parent = rightShoulder;

            rightElbow.FirstLevelChildren = new PlayerModelBlock[1];
            rightElbow.FirstLevelChildren[0] = rightHand;
            rightHand.Parent = rightElbow;

            hips.FirstLevelChildren = new PlayerModelBlock[2];
            hips.FirstLevelChildren[0] = rightHip;
            hips.FirstLevelChildren[1] = leftHip;
            rightHip.Parent = hips;
            leftHip.Parent = hips;

            rightHip.FirstLevelChildren = new PlayerModelBlock[1];
            rightHip.FirstLevelChildren[0] = rightKnee;
            rightKnee.Parent = rightHip;

            rightKnee.FirstLevelChildren = new PlayerModelBlock[1];
            rightKnee.FirstLevelChildren[0] = rightFoot;
            rightFoot.Parent = rightKnee;

            leftHip.FirstLevelChildren = new PlayerModelBlock[1];
            leftHip.FirstLevelChildren[0] = leftKnee;
            leftKnee.Parent = leftHip;


            leftKnee.FirstLevelChildren = new PlayerModelBlock[1];
            leftKnee.FirstLevelChildren[0] = leftFoot;
            leftFoot.Parent = leftKnee;


            neck.FirstLevelChildren = new PlayerModelBlock[1];
            neck.FirstLevelChildren[0] = head;
            head.Parent = neck;

            head.FirstLevelChildren = new PlayerModelBlock[1];
            head.FirstLevelChildren[0] = hat;
            hat.Parent = head;

            hat.FirstLevelChildren = new PlayerModelBlock[1];
            hat.FirstLevelChildren[0] = hatTop;
            hatTop.Parent = hat;

            hatTop.FirstLevelChildren = new PlayerModelBlock[1];
            hatTop.FirstLevelChildren[0] = auraPosition;
            auraPosition.Parent = hatTop;
        }
        private static JVector half = new JVector(1f, 1f, 1f);

        public static Matrix4 CreateFromQuaternion(Quaternion q)
        {
            Matrix4 result = Matrix4.Identity;

			float X = q.X;
			float Y = q.Y;
			float Z = q.Z;
			float W = q.W;
			
			float xx = X * X;
			float xy = X * Y;
			float xz = X * Z;
			float xw = X * W;
			float yy = Y * Y;
			float yz = Y * Z;
			float yw = Y * W;
			float zz = Z * Z;
			float zw = Z * W;
			
			result.M11 = 1 - 2 * (yy + zz);
			result.M21 = 2 * (xy - zw);
			result.M31 = 2 * (xz + yw);
			result.M12 = 2 * (xy + zw);
			result.M22 = 1 - 2 * (xx + zz);
			result.M32 = 2 * (yz - xw);
			result.M13 = 2 * (xz - yw);
			result.M23 = 2 * (yz + xw);
			result.M33 = 1 - 2 * (xx + yy);
            return result;
        }


        public static Quaternion calculateQ(Vector3 rot)
        {
            Quaternion q = new Quaternion();
            q = Quaternion.FromAxisAngle(Vector3.UnitX, rot.X);
            //q = Quaternion.FromAxisAngle(Vector3.UnitY, rot.Y);
            q *= Quaternion.FromAxisAngle(Vector3.UnitZ, rot.Y);
            q *= Quaternion.FromAxisAngle(Vector3.UnitY, rot.Z);
            return q;
        }
        float x = -1;
        float y = 0;
        float z = 0;
        public void DrawBlock(PlayerModelBlock pmb, JMatrix orientation, JVector position, FieldOfJudgementGame p)
        {
            p.color[0] = pmb.color[0];
            p.color[1] = pmb.color[1];
            p.color[2] = pmb.color[2];
            p.color[3] = pmb.color[3];

            Matrix4 m4 = Matrix4.Scale(pmb.scaling.X, pmb.scaling.Y, pmb.scaling.Z);
            m4 *= Matrix4.CreateTranslation(pmb.translation.X, pmb.translation.Y, pmb.translation.Z);
            if (pmb == leftFoot)
            {
                int z = 0;
                z++;
            }
            //sums up parents rotations
            pmb.rotationHelp = Vector3.Zero;
            pmb.rotationHelp.X = pmb.rotation.X;
            pmb.rotationHelp.Y = pmb.rotation.Y;
            pmb.rotationHelp.Z = pmb.rotation.Z;
            PlayerModelBlock pmbHelp = pmb.Parent;

            pmb.rotationQHelp.X = pmb.rotationQ.X;
            pmb.rotationQHelp.Y = pmb.rotationQ.Y;
            pmb.rotationQHelp.Z = pmb.rotationQ.Z;
            pmb.rotationQHelp.W = pmb.rotationQ.W;

            if (pmb == rightShoulder)
            {
                int z = 0; z++;
            }
            if (pmb == rightElbow)
            {
                int z = 0; z++;
            }
            while (pmbHelp != null)
            {
                pmb.rotationQHelp = pmbHelp.rotationQ * pmb.rotationQHelp;
                pmbHelp = pmbHelp.Parent;
            }
            pmb.recalculatePoints();

            m4 *= CreateFromQuaternion(pmb.rotationQHelp);

            if (pmb.Parent != null)
            {
                Vector4 d1 = pmb.Parent.bottomPoint - pmb.topPoint;
                Vector4 d2 = pmb.Parent.bottomPointNew - pmb.topPointNew;
                Vector4 t = d2 - d1;

                pmb.topPointNew += t;
                pmb.bottomPointNew += t;
                m4 *= Matrix4.CreateTranslation(t.X, t.Y, t.Z);
            }
            m4 *= JMatrixToMatrix4(orientation);
            m4 *= Matrix4.CreateTranslation(position.X, position.Y, position.Z);
            
            p.dsDrawModel(m4, half, pmb.blockTextures);
        }

        public void DrawModel(FieldOfJudgementGame p, JMatrix Orientation, JVector Position)
        {
            float[] oldColor = p.color;
            bool ut = p.use_textures;
            p.use_textures = true;
            DrawRecursion(belly, Orientation, Position, p);
            p.use_textures = ut;
            p.color = oldColor;
        }


        public void AddDeathBody(FieldOfJudgementGame p, JMatrix Orientation, JVector Position)
        {
            float[] oldColor = p.color;
            bool ut = p.use_textures;
            p.use_textures = true;
            AddDeathBodyRecursion(belly, Orientation, Position, p);
            p.use_textures = ut;
            p.color = oldColor;
        }

        public void AddDeathBodyRecursion(PlayerModelBlock pmb, JMatrix orientation, JVector position, FieldOfJudgementGame p)
        {
            if (pmb == auraPosition || pmb == hat || pmb == hatTop)
                return;
            FieldOfJudgement.Enviroment.DeathBodyFragment df = new FieldOfJudgement.Enviroment.DeathBodyFragment(
                position + new JVector(pmb.translation.X, pmb.translation.Y, pmb.translation.Z),
                new JVector(pmb.scaling.X + 0.01f, pmb.scaling.Y + 0.01f, pmb.scaling.Z + 0.01f));
            df.Orientation = orientation;
            p.world.AddBody(df);
            if (pmb.FirstLevelChildren != null)
                for (int a = 0; a < pmb.FirstLevelChildren.Length; a++)
                {
                    AddDeathBodyRecursion(pmb.FirstLevelChildren[a], orientation, position, p);
                }
        }

        public void DrawRecursion(PlayerModelBlock pmb, JMatrix orientation, JVector position, FieldOfJudgementGame p)
        {
            bool oldShadow = p.use_shadows;
            if (pmb == auraPosition)
                p.use_shadows = false;
            DrawBlock(pmb, orientation, position, p);
            p.use_shadows = oldShadow;
            if (pmb.FirstLevelChildren != null)
                for (int a = 0; a < pmb.FirstLevelChildren.Length; a++)
                {
                    DrawRecursion(pmb.FirstLevelChildren[a], orientation, position, p);
                }
        }

        public void finalize()
        {
            finalizeRecursive(belly);
        }

        private void finalizeRecursive(PlayerModelBlock pmb)
        {
            pmb.finalize();
            if (pmb.FirstLevelChildren != null)
                for (int a = 0; a < pmb.FirstLevelChildren.Length; a++)
                {
                    finalizeRecursive(pmb.FirstLevelChildren[a]);
                }
        }
    }
}

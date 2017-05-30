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

namespace FieldOfJudgement.Magic
{
    /**
     *This class is used in rendering loop of the game. It is rendered as rigid body block
     */ 
    public class MagicMissile : MagicBullet
    {
        public new static DS_TEXTURE_NUMBER textureHandler = DS_TEXTURE_NUMBER.DS_NONE;
        public MagicMissile(int ownerId, float radius, float mass, float power,
            float minimalSpeed, bool vanishAfterCollisionWithAnyObject,
            bool vanishAfterCollisionWithPlayer, bool doNotHurtOwner,
            double lastingTimeMiliseconds)
                    : base(ownerId, radius, mass, power,
                    minimalSpeed, vanishAfterCollisionWithAnyObject,
                    vanishAfterCollisionWithPlayer, doNotHurtOwner,
                    lastingTimeMiliseconds)
        {
        }
        public override DS_TEXTURE_NUMBER GetTexture()
        {
            return textureHandler;
        }
    }
}

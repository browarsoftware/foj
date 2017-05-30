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
using Jitter.LinearMath;

namespace FieldOfJudgement
{
    /**
     * Conversion of vectors and matrices to arrays. It is used in OpenGL calls.
     */
    public class Conversion
    {
        public static float[] ToFloat(JVector vector)
        {
            return new float[4] { vector.X, vector.Y, vector.Z, 0.0f };
        }

        public static float[] ToFloat(JMatrix matrix)
        {
            return new float[12] { matrix.M11, matrix.M21, matrix.M31, 0.0f,
                                matrix.M12, matrix.M22, matrix.M32, 0.0f,
                                matrix.M13, matrix.M23, matrix.M33, 1.0f };
        }
    }
}

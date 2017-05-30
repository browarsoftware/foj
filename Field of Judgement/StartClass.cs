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
using FieldOfJudgement.Sound;

namespace FieldOfJudgement
{
    class StartClass
    {
        public static bool StartGame = false;
        public static bool CloseGame = false;
        //public static bool fullscrean = false;
        public static GameConf gameConf = null;
        static void Main(string[] args)
        {
            gameConf = GameConf.LoadFromFileOrCreate("conf.xml");
            Audio.SoundEnabled = gameConf.IsAudioEnabled;
            if (args != null)
                if (args.Length > 0)
                {
                    if (args[0] == "-calibrate" || args[0] == "-c")
                    {
                        using (FieldOfJudgementGame p = new FieldOfJudgementGame(800, 600, true))
                        {
                            p.Run();
                        }
                        return;
                    }
                }
            /*
            using (FieldOfJudgementGame p = new FieldOfJudgementGame(800, 600, false))
            {
                p.Run();
            }*/
            
            do
            {
                StartGame = false;
                CloseGame = false;
                using (StartScreen p = new StartScreen(800, 600))
                {
                    p.Run();
                }
                if (StartGame == true)
                {
                    using (FieldOfJudgementGame p = new FieldOfJudgementGame(800, 600))
                    {
                        p.Run();
                    }
                }
            }
            
            while (CloseGame != true);
        }
    }
}

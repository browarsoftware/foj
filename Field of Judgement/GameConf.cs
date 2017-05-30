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
using System.Xml.Serialization;
using System.IO;

namespace FieldOfJudgement
{
    [XmlRootAttribute("GameConf", Namespace = "", IsNullable = false)]
    public class GameConf
    {
        public bool IsGoreEffects = true;
        public int MaxBoxCount = 529;//23 * 23
        public bool IsFullScrean = false;
        public bool IsVerticalSplit = true;
        public bool IsAudioEnabled = true;

        public GameConf()
        {
        }
        public static void SaveToFile(GameConf gc, String path)
        {
            using (TextWriter textWriter = new StreamWriter(path))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameConf));
                xmlSerializer.Serialize(textWriter, gc);
            }
        }
        public static GameConf LoadFromFile(String path)
        {
            GameConf gc = null;
            using (TextReader textReader = new StreamReader(path))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameConf));
                gc = xmlSerializer.Deserialize(textReader) as GameConf;

            }
            return gc;
        }

        public static GameConf LoadFromFileOrCreate(String path)
        {
            GameConf gc = null;
            try
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameConf));
                    gc = xmlSerializer.Deserialize(textReader) as GameConf;

                }
            }
            catch { }
            if (gc == null)
            {
                gc = new GameConf();
                SaveToFile(gc, path);
            }
            return gc;
        }
    }
}

/**
 This code is part of Voxel Game project http://www.voxelgame.com/ (public domain)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FieldOfJudgement.Sound
{
    internal static class Music
    {
        internal static void StartMusic(SoundType music)
        {
            Audio.PlaySoundLooping(music, 0.4f);
        }

        internal static void StopMusic(SoundType music)
        {
            Audio.StopSound(music);
        }
    }
}

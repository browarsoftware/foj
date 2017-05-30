using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Jitter.LinearMath;
using System.IO;
using System.Reflection;

namespace FieldOfJudgement
{
    public class PlayerModelBlock
    {
        public PlayerModelBlock[] FirstLevelChildren = null;
        public PlayerModelBlock Parent = null;

        public float []color;
        public Vector3 translation;
        public Vector3 rotation;
        public Vector3 scaling;

        public Quaternion rotationQ = new Quaternion();
        public Quaternion rotationQHelp = new Quaternion();

        public Vector4 topPoint;
        public Vector4 bottomPoint;

        public Vector4 topPointNew;
        public Vector4 bottomPointNew;

        public DS_TEXTURE_NUMBER[] textures = new DS_TEXTURE_NUMBER[6];
        public Texture[] blockTextures = null;

        public Vector3 rotationHelp;


        public void recalculatePoints()
        {
            //Skeleton knots are here
            topPoint.X = (float)(0 / 2.0f) + translation.X;
            topPoint.Y = (float)(0 / 2.0f) + translation.Y;
            topPoint.Z = (float)(scaling.Z / 2.0f) + translation.Z;
            topPoint.W = 0;

            bottomPoint.X = (float)(0 / 2.0f) + translation.X;
            bottomPoint.Y = (float)(0 / 2.0f) + translation.Y;
            bottomPoint.Z = (float)(-scaling.Z / 2.0f) + translation.Z;

            bottomPoint.W = 0;
            Matrix4 rotationM = PlayerModel.CreateFromQuaternion(rotationQHelp);
            
            topPointNew = Vector4.Transform(topPoint, rotationM);
            bottomPointNew = Vector4.Transform(bottomPoint, rotationM);
        }

        private void LoadTexturesHelper(String name, String defaultPath, int id)
        {
            Stream readerStream;
            Assembly a = Assembly.GetExecutingAssembly();
            String[] nombres = a.GetManifestResourceNames();
            readerStream = a.GetManifestResourceStream(name);
            try
            {
                blockTextures[id] = new Texture(readerStream);
            }
            catch
            {
                readerStream = a.GetManifestResourceStream(defaultPath);
                blockTextures[id] = new Texture(readerStream);
            }
        }



        public void LoadTextures(String name, String path, String defaultPath)
        {
            blockTextures = new Texture[6];
            LoadTexturesHelper(path + "." + name + "_top.png", defaultPath, 0);
            LoadTexturesHelper(path + "." + name + "_bottom.png", defaultPath, 1);
            LoadTexturesHelper(path + "." + name + "_front.png", defaultPath, 2);
            LoadTexturesHelper(path + "." + name + "_back.png", defaultPath, 3);
            LoadTexturesHelper(path + "." + name + "_left.png", defaultPath, 4);
            LoadTexturesHelper(path + "." + name + "_right.png", defaultPath, 5);
        }

        public PlayerModelBlock()
        {
            rotation.X = 0;
            rotation.Y = 0;
            rotation.Z = 0;
            color = new float[4];
            rotationQ = Quaternion.Identity;
            for (int a = 0; a < color.Length; a++)
                color[a] = 255;
            for (int a = 0; a < textures.Length; a++ )
                textures[a] = DS_TEXTURE_NUMBER.DS_CHECKERED;
        }

        public void finalize()
        {
            if (blockTextures == null) return;
            for (int a = 0; a < blockTextures.Length; a++)
            {
                if (blockTextures[a] != null)
                    blockTextures[a].finalize();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLevelLibrary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Matrix4x4
    {
        [XmlAttribute]
        public float M11 = 1;
        [XmlAttribute]
        public float M12 = 0;
        [XmlAttribute]
        public float M13 = 0;
        [XmlAttribute]
        public float M14 = 0;

        [XmlAttribute]
        public float M21 = 0;
        [XmlAttribute]
        public float M22 = 1;
        [XmlAttribute]
        public float M23 = 0;
        [XmlAttribute]
        public float M24 = 0;

        [XmlAttribute]
        public float M31 = 0;
        [XmlAttribute]
        public float M32 = 0;
        [XmlAttribute]
        public float M33 = 1;
        [XmlAttribute]
        public float M34 = 0;

        [XmlAttribute]
        public float M41 = 0;
        [XmlAttribute]
        public float M42 = 0;
        [XmlAttribute]
        public float M43 = 0;
        [XmlAttribute]
        public float M44 = 1;

        public System.Numerics.Matrix4x4 ToMatrix()
        {
            var mat = new System.Numerics.Matrix4x4(
                M11, M12, M13, M14,
                M21, M22, M23, M24,
                M31, M32, M33, M34,
                M41, M42, M43, M44);

            return mat;

            return System.Numerics.Matrix4x4.Transpose(mat);
        }

        public OpenTK.Matrix4 ToGLMatrix()
        {
            var mat = new OpenTK.Matrix4(
                M11, M12, M13, M14,
                M21, M22, M23, M24,
                M31, M32, M33, M34,
                M41, M42, M43, M44);

            return mat;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Matrix4x3
    {
        [XmlAttribute]
        public float M11;
        [XmlAttribute]
        public float M12;
        [XmlAttribute]
        public float M13;
        [XmlAttribute]
        public float M14;

        [XmlAttribute]
        public float M21;
        [XmlAttribute]
        public float M22;
        [XmlAttribute]
        public float M23;
        [XmlAttribute]
        public float M24;

        [XmlAttribute]
        public float M31;
        [XmlAttribute]
        public float M32;
        [XmlAttribute]
        public float M33;
        [XmlAttribute]
        public float M34;
    }

    public class Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord0;
        public Vector4 TexCoord1;
        public Vector2 TexCoord2;
        public Vector2 TexCoord3;
        public Vector2 TexCoord4;

        public Vector4 Color = Vector4.One;
        public List<float> BoneWeights = new List<float>();
        public List<int> BoneIndices = new List<int>();

        //2 morph matrices applied, 3 vec4s each
        public Vector4[] Morphs = new Vector4[6];


        //LM3
        public Vector4 Tangent;
    }
}

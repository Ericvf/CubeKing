using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeKing.Core
{
    public class FaceNormal
    {
        public readonly Vector3 Normal;
        public readonly Face Face;

        public List<VertexPositionNormalTextureTexture> Vertices;
        public List<ushort> Indices;
        public int Channel;

        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;
        public bool IsPainted;

        public FaceNormal(Vector3 normal, Face face, int channel)
        {
            this.Normal = normal;
            this.Channel = channel;
            this.Face = face;
        }

        public Vector2 tlUV = new Vector2(0.0f, 0.0f);
        public Vector2 trUV = new Vector2(1.0f, 0.0f);
        public Vector2 blUV = new Vector2(0.0f, 1.0f);
        public Vector2 brUV = new Vector2(1.0f, 1.0f);

        // texture coordinates for XNA sub-image
        public Vector2 ttlUV = new Vector2(0.0f, 0.0f);
        public Vector2 ttrUV = new Vector2(1.0f, 0.0f);
        public Vector2 tblUV = new Vector2(0.0f, 1.0f);
        public Vector2 tbrUV = new Vector2(1.0f, 1.0f);

        // texture coordinates for XNA sub-image


        internal void Init(GraphicsDevice graphicsDevice)
        {
            var vertices = new List<VertexPositionNormalTextureTexture>();

            // Get two vectors perpendicular to the face normal and to each other.
            Vector3 side1 = new Vector3(Normal.Y, Normal.Z, Normal.X);
            Vector3 side2 = Vector3.Cross(Normal, side1);

            // Four vertices per face.
            var size = 1f;
            vertices.Add(new VertexPositionNormalTextureTexture((Normal - side1 - side2) * size / 2, Normal, tlUV, ttlUV));
            vertices.Add(new VertexPositionNormalTextureTexture((Normal - side1 + side2) * size / 2, Normal, trUV, ttrUV));
            vertices.Add(new VertexPositionNormalTextureTexture((Normal + side1 + side2) * size / 2, Normal, brUV, tbrUV));
            vertices.Add(new VertexPositionNormalTextureTexture((Normal + side1 - side2) * size / 2, Normal, blUV, tblUV));

            var vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTextureTexture.VertexDeclaration, vertices.Count, BufferUsage.None);
            vertexBuffer.SetData(0, vertices.ToArray(), 0, vertices.Count, VertexPositionNormalTextureTexture.Stride);
            this.vertexBuffer = vertexBuffer;

            this.Vertices = vertices;
        }

    }

    public enum Face : int
    {
        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeKing.Core
{
    /// <summary>
    /// Define a vertex with a position, color, normal and texture
    /// </summary>
    public struct VertexPositionNormalTextureTexture
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
        public Vector2 UVT;

        public const int Stride = 40;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexPositionNormalTextureTexture(Vector3 position, Vector3 normal, Vector2 uv, Vector2 uvTexture)
        {
            Position = position;
            Normal = normal;
            UV = uv;
            UVT = uvTexture;
        }

        /// <summary>
        /// A VertexDeclaration object, which contains information about the vertex
        /// elements contained within this struct.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
        );
    }
}

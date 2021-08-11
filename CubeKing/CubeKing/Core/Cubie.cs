using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;
using System.Diagnostics;

namespace CubeKing.Core
{
    public class Cubie
    {
        #region Readonly fields
        const float halfunitlength = 0.5f;
        readonly CubeScene scene;

        readonly GraphicsDevice graphicsDevice;
        readonly ContentManager contentManager;

        readonly SilverlightEffect basicEffect;
        readonly SilverlightEffectParameter worldViewProjectionParameter;
        readonly SilverlightEffectParameter worldParameter;

        readonly SilverlightEffectParameter useLightingParameter;
        readonly SilverlightEffectParameter lightPositionParameter;

        readonly SilverlightEffectParameter structureColorParameter;
        readonly SilverlightEffectParameter faceColorParameter;
        readonly SilverlightEffectParameter useStickerParameter;
        readonly SilverlightEffectParameter isPaintedParameter;
        readonly SilverlightEffectParameter isTextureParameter;

        readonly SilverlightEffectParameter bigStickerParameter;

        readonly SilverlightEffectParameter alphaParameter;

        readonly FaceNormal[] faceNormals =
        {
           new FaceNormal(Vector3.Up      , Face.Up, 0),
           new FaceNormal(Vector3.Down    , Face.Down, 1),
           new FaceNormal(Vector3.Left    , Face.Left, 2),
           new FaceNormal(Vector3.Right   , Face.Right, 3),
           new FaceNormal(-Vector3.Forward , Face.Front, 4),
           new FaceNormal(-Vector3.Backward, Face.Back, 5),           
        };

        // texture coordinates for XNA sub-image
        Vector2 topLeftUVXNA = new Vector2(0.0f, 0.0f);
        Vector2 topRightUVXNA = new Vector2(1.0f, 0.0f);
        Vector2 bottomLeftUVXNA = new Vector2(0.0f, 1.0f);
        Vector2 bottomRightUVXNA = new Vector2(1.0f, 1.0f);

        //// texture coordinates for XNA sub-image
        //Vector2 topLeftUVXNA2 = new Vector2(0.0f, 0.0f);
        //Vector2 topRightUVXNA2 = new Vector2(1.0f, 0.0f);
        //Vector2 bottomLeftUVXNA2 = new Vector2(0.0f, 1.0f);
        //Vector2 bottomRightUVXNA2 = new Vector2(1.0f, 1.0f);

        List<ushort> allindices = new List<ushort>();

        #endregion

        #region Effect Properties

        public Matrix WorldParameter
        {
            set
            {
                worldParameter.SetValue(value);
            }
        }

        public Matrix WorldViewProjection
        {
            set
            {
                worldViewProjectionParameter.SetValue(value);
            }
        }

        public Vector3 LightPosition
        {
            set
            {
                lightPositionParameter.SetValue(value);
            }
        }

        public Vector4 FaceColor
        {
            set
            {
                faceColorParameter.SetValue(value);
            }
        }

        public Vector4 StructureColor
        {
            set
            {
                structureColorParameter.SetValue(value);
            }
        }

        public bool UseLighting
        {
            set
            {
                useLightingParameter.SetValue(value ? 1f : 0f);
            }
        }

        public bool UseSticker
        {
            set
            {
                useStickerParameter.SetValue(value ? 1f : 0f);
            }
        }

        public bool BigSticker
        {
            set
            {
                bigStickerParameter.SetValue(value ? 1f : 0f);
            }
        }


        public float Alpha
        {
            set
            {
                alphaParameter.SetValue(value);
            }
        }

        #endregion

        public Matrix World = Matrix.Identity;

        public Vector3 Position { get; set; }

        public float Size { get; set; }

        private BoundingBox boundingBox;

        public bool iscenter;
        bool isedge;
        bool iscorner;

        public Cubie(CubeScene mainScene, Vector3 position, float size,
            bool center,
            bool edge,
            bool corner)
        {
            this.contentManager = mainScene.ContentManager;
            this.graphicsDevice = mainScene.GraphicsDevice;
            this.scene = mainScene;

            // Init Effect
            this.basicEffect = this.contentManager.Load<SilverlightEffect>(@"BasicEffect");
            
            // Init Effect parameters
            this.worldParameter = this.basicEffect.Parameters[@"World"];
            this.worldViewProjectionParameter = this.basicEffect.Parameters[@"WorldViewProjection"];

            this.useLightingParameter = this.basicEffect.Parameters[@"UseLighting"];
            this.lightPositionParameter = this.basicEffect.Parameters[@"LightPosition"];

            this.structureColorParameter = this.basicEffect.Parameters[@"StructureColor"];
            this.faceColorParameter = this.basicEffect.Parameters[@"FaceColor"];

            this.useStickerParameter = this.basicEffect.Parameters[@"UseSticker"];
            this.isPaintedParameter = this.basicEffect.Parameters[@"IsPainted"];
            this.isTextureParameter = this.basicEffect.Parameters[@"IsTexture"];

            this.bigStickerParameter = this.basicEffect.Parameters[@"BigSticker"];

            this.alphaParameter = this.basicEffect.Parameters[@"Alpha"];

            this.Position = position;
            this.Size = size;

            this.iscenter = center;
            this.iscorner = corner;
            this.isedge = edge;
        }

        public void InitStructure()
        {
            var vectors = new List<Vector3>();
            //var vertexCount = 0;

            // Create each face in turn.
            foreach (var faceNormal in this.faceNormals)
            {
                var vertices = new List<VertexPositionNormalTextureTexture>();
                var indices = new List<ushort>();

                var normal = faceNormal.Normal;
                var color = new Vector4(1, 1, 1, 1);

                // Get two vectors perpendicular to the face normal and to each other.
                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2 = Vector3.Cross(normal, side1);

                // Six indices (two triangles) per face.
                indices.Add((ushort)vertices.Count);
                indices.Add((ushort)(vertices.Count + 1));
                indices.Add((ushort)(vertices.Count + 2));

                indices.Add((ushort)vertices.Count);
                indices.Add((ushort)(vertices.Count + 2));
                indices.Add((ushort)(vertices.Count + 3));

                //// Six indices (two triangles) per face.
                //allindices.Add((ushort)vertexCount);
                //allindices.Add((ushort)(vertexCount + 1));
                //allindices.Add((ushort)(vertexCount + 2));

                //allindices.Add((ushort)vertexCount);
                //allindices.Add((ushort)(vertexCount + 2));
                //allindices.Add((ushort)(vertexCount + 3));

                //// Four vertices per face.
                var size = 1f;
                vertices.Add(new VertexPositionNormalTextureTexture((normal - side1 - side2) * size / 2, normal, topLeftUVXNA, topLeftUVXNA));
                vertices.Add(new VertexPositionNormalTextureTexture((normal - side1 + side2) * size / 2, normal, topRightUVXNA, topRightUVXNA));
                vertices.Add(new VertexPositionNormalTextureTexture((normal + side1 + side2) * size / 2, normal, bottomRightUVXNA, bottomRightUVXNA));
                vertices.Add(new VertexPositionNormalTextureTexture((normal + side1 - side2) * size / 2, normal, bottomLeftUVXNA, bottomLeftUVXNA));

                //vertexCount += vertices.Count;

                //var vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None);
                //vertexBuffer.SetData(0, vertices.ToArray(), 0, vertices.Count, VertexPositionColorNormalTexture.Stride);
                //faceNormal.vertexBuffer = vertexBuffer;

                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.None);
                indexBuffer.SetData(0, indices.ToArray(), 0, indices.Count);
                faceNormal.indexBuffer = indexBuffer;

                //faceNormal.Indices = indices;
                //faceNormal.Vertices = vertices;

                vectors.AddRange(vertices.Select(v => v.Position));
            }

            // Create bounding box
            this.boundingBox = BoundingBox.CreateFromPoints(vectors);
        }

        internal void Draw(RasterizerState cullMode)
        {

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.RasterizerState = cullMode;
            // graphicsDevice.RasterizerState = RasterizerState.CullNone;

            graphicsDevice.Textures[0] = scene.TileTexture;
            graphicsDevice.Textures[1] = scene.LogoTexture;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            this.UseLighting = this.scene.ViewModel.UseLights;
            this.UseSticker = this.scene.ViewModel.UseStickers;
            this.BigSticker = this.scene.ViewModel.BigStickers;
            this.StructureColor = this.scene.StructureColor;
            //this.StructureColor = new Vector4();

            var centers = this.iscenter && this.scene.ViewModel.UseCenters;
            var edges = this.isedge && this.scene.ViewModel.UseEdges;
            var corners = this.iscorner && this.scene.ViewModel.UseCorners;

            var none = this.scene.ViewModel.UseCenters || this.scene.ViewModel.UseEdges || this.scene.ViewModel.UseCorners;
            bool relevant = centers || edges || corners;

            if (none && this.scene.ViewModel.HideCubies && !relevant)
                return;

            //var faces = new[] { Face.Up, Face.Front, Face.Right }.ToList();

            //var normal1 = this.GetFaceNormal(Vector3.Up);
            //var normal2 = this.GetFaceNormal(-Vector3.Forward);
            //var normal3 = this.GetFaceNormal(Vector3.Right);

            foreach (var faceNormal in this.faceNormals)
            {
                //if (!(faceNormal == normal1 || faceNormal == normal2 || faceNormal == normal3))
                //    continue;

                ////if (!faces.Contains(faceNormal.Face))
                ////    continue; 

                if (this.scene.ViewModel.OnlyShowPaintedFaces && !faceNormal.IsPainted)
                    continue;

                DrawFace(faceNormal, faceNormal.IsPainted);
            }
        }

        //internal void Draw2(RasterizerState cullMode)
        //{

        //    graphicsDevice.DepthStencilState = DepthStencilState.Default;
        //    graphicsDevice.BlendState = BlendState.AlphaBlend;
        //    graphicsDevice.RasterizerState = cullMode;
        //    //graphicsDevice.RasterizerState = RasterizerState.CullNone;

        //    graphicsDevice.Textures[0] = scene.TileTexture;
        //    graphicsDevice.Textures[1] = scene.Textures[0];
        //    graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        //    graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

        //    this.UseLighting = true;
        //    this.UseSticker = this.scene.ViewModel.UseStickers;
        //    this.StructureColor = this.scene.StructureColor;


        //    //foreach (var faceNormal in this.faceNormals)
        //    //{
        //    //    //if (faceNormal.IsPainted)
        //    //    DrawFace(faceNormal);
        //    //}


        //    var vertices = new List<VertexPositionColorNormalTexture>();
        //    //  var indices = new List<ushort>();

        //    // Create each face in turn.
        //    foreach (var faceNormal in faceNormals)
        //    {
        //        var normal = faceNormal.Normal;

        //        // Get two vectors perpendicular to the face normal and to each other.
        //        Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
        //        Vector3 side2 = Vector3.Cross(normal, side1);

        //        // Four vertices per face.
        //        Vector4 color = this.scene.ChannelColors[faceNormal.Channel];
        //        //faceNormal.Init(color);

        //        vertices.AddRange(faceNormal.Vertices);
        //    }

        //    // Create a vertex buffer, and copy our vertex data into it.
        //    var vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None);

        //    vertexBuffer.SetData(0, vertices.ToArray(), 0, vertices.Count, VertexPositionColorNormalTexture.Stride);

        //    // Create an index buffer, and copy our index data into it.
        //    var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, allindices.Count, BufferUsage.None);

        //    indexBuffer.SetData(0, allindices.ToArray(), 0, allindices.Count);

        //    // Statistics
        //    var VerticesCount = vertices.Count;
        //    var FaceCount = allindices.Count / 3;

        //    // The shaders are already set so we can draw primitives
        //    foreach (var pass in basicEffect.CurrentTechnique.Passes)
        //    {
        //        // Apply pass
        //        pass.Apply();

        //        // Set vertex buffer and index buffer
        //        graphicsDevice.SetVertexBuffer(vertexBuffer);
        //        graphicsDevice.Indices = indexBuffer;

        //        // The shaders are already set so we can draw primitives
        //        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VerticesCount, 0, FaceCount);

        //    }
        //}

        internal void Draw(RasterizerState cullMode, Vector3 unit)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.RasterizerState = cullMode;
            graphicsDevice.Textures[0] = scene.TileTexture;

            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            this.UseLighting = false;
            this.UseSticker = this.scene.ViewModel.UseStickers;
            this.BigSticker = this.scene.ViewModel.BigStickers;
            this.StructureColor = this.scene.StructureColor;

            var normal = this.GetFaceNormal(unit);


            var centers = this.iscenter && this.scene.ViewModel.UseCenters;
            var edges = this.isedge && this.scene.ViewModel.UseEdges;
            var corners = this.iscorner && this.scene.ViewModel.UseCorners;

            var none = this.scene.ViewModel.UseCenters || this.scene.ViewModel.UseEdges || this.scene.ViewModel.UseCorners;
            bool relevant = centers || edges || corners;

            if (none && this.scene.ViewModel.HideCubies && !relevant)
                return;

            foreach (var faceNormal in this.faceNormals)
            {
                if (faceNormal != normal)
                    continue;

                DrawFace(faceNormal, faceNormal.IsPainted);
            }
        }

        internal void DrawFace(FaceNormal faceNormal, bool isPainted = true)
        {

            if (isPainted)
            {
                var color = this.scene.ViewModel.ChannelColors[faceNormal.Channel];
                this.FaceColor = color;

                this.isPaintedParameter.SetValue(1f);
            }
            else
            {
                //this.StructureColor = new Vector4();
                this.isPaintedParameter.SetValue(0f);
            }

            if (this.scene.ViewModel.IsTexture)
            {
                if (faceNormal.Channel < scene.Textures.Count)
                    graphicsDevice.Textures[1] = scene.Textures[faceNormal.Channel];
                else
                    graphicsDevice.Textures[1] = scene.LogoTexture;

                this.isTextureParameter.SetValue(1f);
            }
            else
            {
                this.isTextureParameter.SetValue(0f);
            }

            //if (faceNormal.Channel < scene.Textures.Count)
            //    graphicsDevice.Textures[1] = scene.Textures[faceNormal.Channel];
            //else
            //    graphicsDevice.Textures[1] = scene.Textures[0];

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                // Apply pass
                pass.Apply();

                // Set vertex buffer and index buffer
                graphicsDevice.SetVertexBuffer(faceNormal.vertexBuffer);
                graphicsDevice.Indices = faceNormal.indexBuffer;

                // The shaders are already set so we can draw primitives
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
        }

        internal void InitializeFace(Face face, int channel)
        {
            foreach (var faceNormal in faceNormals)
            {
                if (faceNormal.Face == face)
                {
                    #region Faces
                    if (face == Face.Up)
                    {
                        var nw = (1 / this.scene.depth);
                        var nx1 = (-this.Position.Z + this.scene.hd - halfunitlength) * nw;
                        var nx2 = (-this.Position.Z + this.scene.hd + halfunitlength) * nw;

                        var nh = (1 / this.scene.width);
                        var ny1 = (this.Position.X + this.scene.hw - halfunitlength) * nh;
                        var ny2 = (this.Position.X + this.scene.hw + halfunitlength) * nh;

                        faceNormal.ttlUV = new Vector2(nx1, ny1);
                        faceNormal.ttrUV = new Vector2(nx2, ny1);
                        faceNormal.tblUV = new Vector2(nx1, ny2);
                        faceNormal.tbrUV = new Vector2(nx2, ny2);
                    }

                    if (face == Face.Down)
                    {
                        var nw = (1 / this.scene.depth);
                        var nx1 = (-this.Position.Z + this.scene.hd - halfunitlength) * nw;
                        var nx2 = (-this.Position.Z + this.scene.hd + halfunitlength) * nw;

                        var nh = (1 / this.scene.width);
                        var ny1 = (-this.Position.X + this.scene.hw - halfunitlength) * nh;
                        var ny2 = (-this.Position.X + this.scene.hw + halfunitlength) * nh;

                        faceNormal.ttlUV = new Vector2(nx1, ny1);
                        faceNormal.ttrUV = new Vector2(nx2, ny1);
                        faceNormal.tblUV = new Vector2(nx1, ny2);
                        faceNormal.tbrUV = new Vector2(nx2, ny2);
                    }

                    if (face == Face.Front)
                    {
                        var nw = (1 / this.scene.width);
                        var nx1 = (-this.Position.X + this.scene.hw - halfunitlength) * nw;
                        var nx2 = (-this.Position.X + this.scene.hw + halfunitlength) * nw;

                        var nh = (1 / this.scene.height);
                        var ny1 = (this.Position.Y + this.scene.hh - halfunitlength) * nh;
                        var ny2 = (this.Position.Y + this.scene.hh + halfunitlength) * nh;
                        faceNormal.ttlUV = new Vector2(nx1, ny1);
                        faceNormal.ttrUV = new Vector2(nx2, ny1);
                        faceNormal.tblUV = new Vector2(nx1, ny2);
                        faceNormal.tbrUV = new Vector2(nx2, ny2);
                    }

                    if (face == Face.Back)
                    {
                        var nw = (1 / this.scene.width);
                        var nx1 = (-this.Position.X + this.scene.hw - halfunitlength) * nw;
                        var nx2 = (-this.Position.X + this.scene.hw + halfunitlength) * nw;

                        var nh = (1 / this.scene.height);
                        var ny1 = (-this.Position.Y + this.scene.hh - halfunitlength) * nh;
                        var ny2 = (-this.Position.Y + this.scene.hh + halfunitlength) * nh;
                        faceNormal.ttlUV = new Vector2(nx1, ny1);
                        faceNormal.ttrUV = new Vector2(nx2, ny1);
                        faceNormal.tblUV = new Vector2(nx1, ny2);
                        faceNormal.tbrUV = new Vector2(nx2, ny2);
                    }


                    if (face == Face.Right)
                    {
                        var nw = (1 / this.scene.height);
                        var nx1 = (-this.Position.Y + this.scene.hh - halfunitlength) * nw;
                        var nx2 = (-this.Position.Y + this.scene.hh + halfunitlength) * nw;

                        var nh = (1 / this.scene.depth);
                        var ny1 = (this.Position.Z + this.scene.hd - halfunitlength) * nh;
                        var ny2 = (this.Position.Z + this.scene.hd + halfunitlength) * nh;
                        faceNormal.ttlUV = new Vector2(nx1, ny1);
                        faceNormal.ttrUV = new Vector2(nx2, ny1);
                        faceNormal.tblUV = new Vector2(nx1, ny2);
                        faceNormal.tbrUV = new Vector2(nx2, ny2);
                    }

                    if (face == Face.Left)
                    {
                        var nw = (1 / this.scene.height);
                        var nx1 = (-this.Position.Y + this.scene.hh - halfunitlength) * nw;
                        var nx2 = (-this.Position.Y + this.scene.hh + halfunitlength) * nw;

                        var nh = (1 / this.scene.depth);
                        var ny1 = (-this.Position.Z + this.scene.hd - halfunitlength) * nh;
                        var ny2 = (-this.Position.Z + this.scene.hd + halfunitlength) * nh;
                        faceNormal.ttlUV = new Vector2(nx1, ny1);
                        faceNormal.ttrUV = new Vector2(nx2, ny1);
                        faceNormal.tblUV = new Vector2(nx1, ny2);
                        faceNormal.tbrUV = new Vector2(nx2, ny2);
                    }
                    #endregion

                    faceNormal.IsPainted = true;
                    faceNormal.Channel = channel;
                }

                faceNormal.Init(this.graphicsDevice);
            }
        }

        public FaceNormal GetFaceNormal(Vector3 unit)
        {
            for (int i = 0; i < faceNormals.Length; i++)
            {
                // Transform normal to cubie rotation
                var normal = Vector3.Transform(faceNormals[i].Normal, this.World);

                // Dotproduct of unit and normal
                var dot = Vector3.Dot(unit, normal);

                if (dot == 1f)
                    return faceNormals[i];
            }

            return null;
        }

        internal Vector3 IntersectsNormal(Ray ray)
        {
            var world = this.World * Matrix.CreateTranslation(this.Position);

            foreach (var normal in this.faceNormals)
            {
                if (normal.Vertices == null)
                    continue;

                var vertices = normal.Vertices.Select(v => v.Position);

                var box = BoundingBox.CreateFromPoints(vertices);

                box = new BoundingBox(
                   Vector3.Transform(box.Min, world),
                   Vector3.Transform(box.Max, world));

                if (box.Intersects(ray) != null)
                {
                    var n = Vector3.TransformNormal(normal.Normal, world);
                    if (n.X == 1 || n.Y == 1 || n.Z == 1)
                        return n;
                }
            }

            return Vector3.Zero;
        }

        internal bool Intersects(Ray ray)
        {
            var world = Matrix.CreateScale(this.Size)
                * this.World
                * Matrix.CreateTranslation(this.Position)
                * Matrix.CreateRotationZ(scene.r);
                ; 

            // Transform the bounding box 
            var bounding = new BoundingBox(
                Vector3.Transform(this.boundingBox.Min, world),
                Vector3.Transform(this.boundingBox.Max, world));

            // Intersect
            var intersect = bounding.Intersects(ray);
            if (intersect!= null)
                return true;

            return false;
        }


        internal float? Intersects2(Ray ray)
        {
            var world = Matrix.CreateScale(this.Size)
                * this.World
                * Matrix.CreateTranslation(this.Position)
                * Matrix.CreateRotationZ(scene.r);
                ;

            // Transform the bounding box 
            var bounding = new BoundingBox(
                Vector3.Transform(this.boundingBox.Min, world),
                Vector3.Transform(this.boundingBox.Max, world));

            // Intersect
            var intersect = bounding.Intersects(ray);
            if (intersect != null)
                return intersect;

            return null;
        }


        internal bool Is(CubeScene.CubieType cubieType, Vector3 unit, int channel)
        {
            bool isCubieType = false;
            switch (cubieType)
            {
                case CubeScene.CubieType.Center:
                    isCubieType = this.iscenter;
                    break;
                case CubeScene.CubieType.Edge:
                    isCubieType = this.isedge;
                    break;
                case CubeScene.CubieType.Corner:
                    isCubieType = this.iscorner;
                    break;
            }

            if (!isCubieType)
                return false;

            foreach (var fn in faceNormals)
            {
                if (fn.Normal == unit && fn.Channel == channel && fn.IsPainted)
                    return true;
            }

            return false;
        }

        internal FaceNormal IntersectsFaceNormal(Ray ray)
        {
            var world = this.World
                * Matrix.CreateTranslation(this.Position)
                * Matrix.CreateRotationZ(scene.r);

            var returnValue = default(FaceNormal);
            var lowest = 0f;

            foreach (var normal in this.faceNormals)
            {
                if (!normal.IsPainted || normal.Vertices == null)
                    continue;

                var vertices = normal.Vertices.Select(v => v.Position);

                var box = BoundingBox.CreateFromPoints(vertices);

                box = new BoundingBox(
                   Vector3.Transform(box.Min, world),
                   Vector3.Transform(box.Max, world));

                var intersect = box.Intersects(ray);
                if (intersect != null)
                {
                    if (returnValue == null || intersect < lowest)
                    {
                        returnValue = normal;
                        lowest = (float)intersect;
                    }

                  //  Debug.WriteLine(normal.Face + " " + intersect);
                    
                  //  var n1 = normal.Normal;
                  //  //n1.Normalize();

                  //  var n = Vector3.TransformNormal(n1, world);
                    

                  //  var d = Vector3.Dot(n, n1);
                    
                  //  if (d < 0)
                  //      continue;
                    
                    

                  //  var nx = Math.Round(Math.Abs(n.X));
                  //  var ny = Math.Round(Math.Abs(n.Y));
                  //  var nz = Math.Round(Math.Abs(n.Z));

                  ////  if (nx == 1 || ny == 1 || nz == 1)
                  //     // return normal;
                }
            }

            return returnValue;
        }
    }
}
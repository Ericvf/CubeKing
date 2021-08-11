using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Graphics;
using CubeKing.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CubeKing
{
    public class CubeScene : BaseScene
    {
        static Random rnd = new Random();

        const float quarterTurnClockwise = (float)(Math.PI / 2);
        const float quarterTurnCounterClockwise = (float)(Math.PI / -2);
        const float halfTurnClockwise = (float)Math.PI;
        const float halfTurnCounterClockwise = (float)-Math.PI;
        const float piDiv180 = 0.0174532925f;

        const float unitlength = 1.0f;
        const float halfunitlength = 0.5f;
        const float dragunit = 50;

        public enum CubieType
        {
            Center,
            Edge,
            Corner
        }

        protected int RandomBetween(int min, int max)
        {
            return rnd.Next(min, max);
        }

        public VM ViewModel { get; set; }

        readonly public Vector4 StructureColor = new Vector4(0, 0, 0, 1f);

        string startScrambleText;
        bool isScrambling = false;
        int scrambleSize = 0;
        int scrambleCount = 0;

        List<List<Move>> moveQueue = new List<List<Move>>();

        bool isMoving = false;
        bool started = false;
        bool reset = false;
        bool isPlaying = false;
        DateTime startTime;

        public List<Texture2D> Textures = new List<Texture2D>();
        public Texture2D TileTexture;
        public Texture2D LogoTexture;

        Matrix puzzleWorld = Matrix.Identity;

        public float reflectionAlpha = 0;
        float max = 0;

        public float width = 0;
        public float height = 0;
        public float depth = 0;

        bool isRightPressing = false;
        bool isLeftPressing = false;
        bool drawReflections = false;

        float wodd;
        float hodd;
        float dodd;

        public int mx;
        public int my;
        public int mz;

        public float hw;
        public float hh;
        public float hd;

        Vector2 pressedPosition;
        Vector2 rightPressedPosition;

        List<Cubie> cubies = new List<Cubie>();
        List<Cubie> cachedCubies;
        
        RenderTarget2D left;
        RenderTarget2D right;
        public float r = 0;

        public Ray CalculateRay(Matrix projectionMatrix, Matrix viewMatrix, Vector2 mousePosition)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(mousePosition, 0f);
            Vector3 farSource = new Vector3(mousePosition, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        void UpdateMouse(Matrix view, Matrix projection)
        {
            if (this.isScrambling)
                return;

            var mouseState = Mouse.GetState();
            this.HandleLeftMouse(mouseState, view, projection);
            this.HandleRightMouse(mouseState);
        }

        Cubie pressedCube1;
        void HandleLeftMouse(MouseState mouseState, Matrix view, Matrix projection)
        {
            Cubie pressedCube = null;

            int offsetFromTop = 0;
            int offsetFromFront = 0;
            int offsetFromRight = 0;

            if (!isLeftPressing && mouseState.LeftButton == ButtonState.Pressed)
            {
                pressedPosition = new Vector2(mouseState.X, mouseState.Y);
                isLeftPressing = true;
            }
            else if (isLeftPressing && mouseState.LeftButton == ButtonState.Released)
            {
                var releasedPosition = new Vector2(mouseState.X, mouseState.Y);
                var ray = CalculateRay(projection, view, pressedPosition);


                float cubieDistance = 0f;
                for (int i = 0; i < this.cubies.Count; i++)
                {
                    var intersect = this.cubies[i].Intersects2(ray);
                    if (intersect != null)
                    {
                        if (pressedCube == null || (intersect.Value < cubieDistance))
                        {
                            var x = this.cubies[i].IntersectsFaceNormal(ray);
                            if (x != null)
                            {
                                pressedCube = this.cubies[i];
                                cubieDistance = intersect.Value;
                            }
                        }
                    }
                }



                isLeftPressing = false;

                bool exitTop = false;
                for (int i = 0; i < height; i++)
                {
                    var faceslice = this.GetSlice(Face.Up, i);

                    foreach (var cubie in faceslice.OrderByDescending(c => c.Position.Z + c.Position.X))
                    {
                        if (cubie.Intersects(ray))
                        {
                        //    pressedCube = cubie;
                            offsetFromTop = i;
                            exitTop = true;
                            break;
                        }
                    }

                    if (exitTop)
                        break;
                }


                if (pressedCube == null)
                    return;

                bool exitRight = false;
                for (int i = 0; i < width; i++)
                {
                    var faceslice = this.GetSlice(Face.Right, i);

                    foreach (var cubie in faceslice)
                    {
                        if (cubie.Intersects(ray))
                        {
                            offsetFromRight = i;
                            exitRight = true;
                            break;
                        }
                    }

                    if (exitRight)
                        break;
                }

                bool exitFront = false;
                for (int i = 0; i < depth; i++)
                {
                    var faceslice = this.GetSlice(Face.Front, i);
                    foreach (var cubie in faceslice)
                    {
                        if (cubie.Intersects(ray))
                        {
                            offsetFromFront = i;
                            exitFront = true;
                            break;
                        }
                    }

                    if (exitFront)
                        break;
                }

                var n = pressedCube.IntersectsNormal(ray);
                var diff = releasedPosition - pressedPosition;
                var isHorizontal = Math.Abs(diff.X) > Math.Abs(diff.Y);
                var eq = this.ViewModel.TurnAnimation;
                var d = this.ViewModel.TurnAnimationSpeed;
                if (diff.Length() > dragunit)
                {
                    if (isHorizontal)
                    {
                        if (diff.X < 0)
                            this.TurnFace(Face.Up, offsetFromTop, false, false, false, eq, d);
                        else
                            this.TurnFace(Face.Up, offsetFromTop, true, false, false, eq, d);
                    }
                    else
                    {
                        if (n.Y == 1 && offsetFromTop == 0)
                        {
                            if (diff.X > 0)
                            {
                                if (diff.Y > 0)
                                    this.TurnFace(Face.Front, offsetFromFront, false, false, false, eq, d);
                                else
                                    this.TurnFace(Face.Front, offsetFromFront, true, false, false, eq, d);
                            }
                            else
                            {
                                if (diff.Y > 0)
                                    this.TurnFace(Face.Right, offsetFromRight, true, false, false, eq, d);
                                else
                                    this.TurnFace(Face.Right, offsetFromRight, false, false, false, eq, d);
                            }
                        }
                        if (n.X == 1)
                        {
                            if (diff.Y > 0)
                                this.TurnFace(Face.Front, offsetFromFront, false, false, false, eq, d);
                            else
                                this.TurnFace(Face.Front, offsetFromFront, true, false, false, eq, d);
                        }
                        else if (n.Z == 1)
                        {
                            if (diff.Y > 0)
                                this.TurnFace(Face.Right, offsetFromRight, true, false, false, eq, d);
                            else
                                this.TurnFace(Face.Right, offsetFromRight, false, false, false, eq, d);
                        }
                    }
                }

                this.pressedCube1 = pressedCube;
                pressedCube = null;
            }
        }

        void HandleRightMouse(MouseState mouseState)
        {
            if (!isRightPressing && mouseState.RightButton == ButtonState.Pressed)
            {
                rightPressedPosition = new Vector2(mouseState.X, mouseState.Y);
                isRightPressing = true;
            }
            else if (isRightPressing && mouseState.RightButton == ButtonState.Released)
            {
                var releasedPosition = new Vector2(mouseState.X, mouseState.Y);
                var centerOfScreen = viewportSize.X / 2;

                var diff = releasedPosition - rightPressedPosition;
                var isHorizontal = Math.Abs(diff.X) > Math.Abs(diff.Y);
                var eq = this.ViewModel.TwistAnimation;
                var d = this.ViewModel.TwistAnimationSpeed;
                if (diff.Length() > dragunit)
                {
                    if (isHorizontal)
                    {
                        if (diff.X > 0)
                            this.Twist(Face.Up, false, eq, d);
                        else
                            this.Twist(Face.Down, false, eq, d);
                    }
                    else if (rightPressedPosition.X > centerOfScreen)
                    {
                        if (diff.Y > 0)
                            this.Twist(Face.Back, false, eq, d);
                        else
                            this.Twist(Face.Front, false, eq, d);
                    }
                    else
                    {
                        if (diff.Y > 0)
                            this.Twist(Face.Left, false, eq, d);
                        else
                            this.Twist(Face.Right, false, eq, d);
                    }
                }

                isRightPressing = false;
            }
        }

        public CubeScene(DrawingSurface drawingSurface)
            : base(drawingSurface)
        {
            this.ViewModel = VM.Load(this);

            var texture = this.ContentManager.Load<Texture2D>("cubeking");
            this.LogoTexture = texture;

            var tiletexture = this.ContentManager.Load<Texture2D>("tile");
            this.TileTexture = tiletexture;

            if (this.ViewModel.commands != null && this.ViewModel.commands.Count > 0)
                this.startScrambleText = string.Join(" ", this.ViewModel.commands.ToArray());

            this.Reset(this.ViewModel.Width, this.ViewModel.Height, this.ViewModel.Depth);

        }

        internal void Reset(float w, float h, float d)
        {
            if (d != this.depth)
                this.ViewModel.BackReflection = (int)d;

            if (w != this.width)
                this.ViewModel.LeftReflection = (int)w;

            if (h != this.height)
                this.ViewModel.DownReflection = (int)h;

            this.ViewModel.Moves = 0;
            this.ViewModel.Time = string.Empty;
            this.ViewModel.ScramblePercentage = 0;

            this.StopPlaying();

            this.cachedCubies = null;
            this.width = w;
            this.height = h;
            this.depth = d;
            this.reflectionAlpha = 0f;
            this.moveQueue = new List<List<Move>>();
            this.isMoving = false;
            this.drawReflections = true;
            this.isScrambling = false;

            this.ViewModel.commands.Clear();
            this.ViewModel.RaisePropertyChanged("CommandHistory");
            started = true;
            reset = true;
        }

        protected override void Initialize()
        {
            // Halfs of the cube dimensions
            hw = width / 2;
            hh = height / 2;
            hd = depth / 2;

            // Determine if dimensions are oddly or evenly numbered
            wodd = (width % 2);
            hodd = (height % 2);
            dodd = (depth % 2);

            // This delta represents the middle layer position of each dimension
            mx = Convert.ToInt32(Math.Floor(width / 2)) - 1;
            my = Convert.ToInt32(Math.Floor(height / 2)) - 1;
            mz = Convert.ToInt32(Math.Floor(depth / 2)) - 1;

            max = Math.Max(Math.Max(width, height), depth);

            puzzleWorld = Matrix.Identity;

            var cubies = new List<Cubie>();

            // Loop through all dimensions and add the cubies
            for (int u = 0; u < depth; u++)
            {
                var z = (u - hd) * unitlength + halfunitlength;

                for (int j = 0; j < height; j++)
                {
                    var y = (j - hh) * unitlength + halfunitlength;

                    for (int i = 0; i < width; i++)
                    {
                        var x = (i - hw) * unitlength + halfunitlength;

                        var ax = Math.Abs(x);
                        var ay = Math.Abs(y);
                        var az = Math.Abs(z);

                        var nx = Math.Abs(hw - halfunitlength);
                        var ny = Math.Abs(hh - halfunitlength);
                        var nz = Math.Abs(hd - halfunitlength);

                        bool outside = ax == nx || ay == ny || az == nz;

                        bool corners = ax == nx && ay == ny && az == nz;

                        bool e1 = ax == nx && az < nz && ay == ny;
                        bool e2 = ay == ny && ax < nx && az == nz;
                        bool e3 = az == nz && ay < ny && ax == nx;
                        bool edges = e1 || e2 || e3;

                        bool c1 = ax != nx && az != nz;
                        bool c2 = ax != nx && ay != ny;
                        bool c3 = az != nz && ay != ny;
                        bool centers = c1 || c2 || c3;

                        if (outside)
                        {
                            var cube = new Cubie(this, new Vector3(x, y, z), this.ViewModel.CubieSize, centers, edges, corners);
                            cubies.Add(cube);
                        }
                    }
                }
            }

            this.cubies = cubies;

            this.InitializeFaces();

            foreach (var cubie in cubies)
                cubie.InitStructure();

            //PrepareCubies();
            // this.Reset(3, 3, 3);

            if (!string.IsNullOrEmpty(this.startScrambleText))
            {
                this.Scramble(this.startScrambleText);
                this.startScrambleText = null;
            }

            started = true;

            //var isSolved = this.IsSolved();
            // Debug.WriteLine("Solved: " + isSolved);

            //this.TurnFace(Face.Front, 0, false, false, false, Eqs.OutSine, 1000);
            //this.TurnFace(Face.Back, 0, false, false, false, Eqs.OutSine, 1000);
            //this.TurnFace(Face.Left, 0, false, false, false, Eqs.OutSine, 1000);
            //this.TurnFace(Face.Right, 0, false, false, false, Eqs.OutSine, 1000);
            //this.TurnFace(Face.Up, 0, false, false, false, Eqs.OutSine, 500);
            //this.TurnFace(Face.Up, 0, true, false, false, Eqs.OutSine, 500);
            //this.TurnFace(Face.Up, 0, false, true, false, Eqs.OutSine, 500);
            //this.TurnFace(Face.Up, 0, true, true, false, Eqs.OutSine, 500);
            //this.TurnPuzzle(Face.Front, false, Eqs.OutSine, 500);
            //this.TurnPuzzle(Face.Back, false, Eqs.OutSine, 500);
            //this.TurnPuzzle(Face.Left, false, Eqs.OutSine, 500);
            //this.TurnPuzzle(Face.Right, false, Eqs.OutSine, 500);
            //this.TurnPuzzle(Face.Up, false, Eqs.OutSine, 500);
            //this.TurnPuzzle(Face.Down, false, Eqs.OutSine, 500);
        }

        internal void InitializeFaces()
        {
            int i = 0;
            foreach (Face face in Enum.GetValues(typeof(Face)))
            {
                var cubies = this.GetSlice(face, 0);

                foreach (var cubie in cubies)
                    cubie.InitializeFace(face, i);

                i++;
            }
        }

        internal List<Cubie> GetSlice(Face face, int sliceDepth = 0)
        {
            var cubies = new List<Cubie>();
            float negate;
            int extra;

            switch (face)
            {
                case Face.Front:
                case Face.Back:
                    negate = (face == Face.Front) ? -1f : 1f;
                    extra = Math.Abs(my - mx);

                    sliceDepth = mz - sliceDepth;
                    float xy = sliceDepth + halfunitlength;

                    if (dodd == 1)
                        xy += halfunitlength;

                    //if (hodd != wodd)
                    //    faceObj.endRotationAngle = halfTurnClockwise;

                    cubies.AddRange(this.GetSliceZ(xy, negate, 0));
                    break;

                case Face.Left:
                case Face.Right:
                    negate = (face == Face.Left) ? -1f : 1f;
                    extra = Math.Abs(my - mx);

                    sliceDepth = mx - sliceDepth;
                    float xz = sliceDepth + halfunitlength;

                    if (wodd == 1)
                        xz += halfunitlength;

                    //if (hodd != dodd)
                    //    endAngle = halfTurnClockwise;

                    cubies.AddRange(this.GetSliceX(xz, negate, 0));
                    break;

                case Face.Up:
                case Face.Down:
                    negate = (face == Face.Down) ? -1f : 1f;
                    extra = Math.Abs(mz - mx);

                    sliceDepth = my - sliceDepth;
                    float xx = sliceDepth + halfunitlength;

                    if (hodd == 1)
                        xx += halfunitlength;

                    //if (wodd != dodd) 
                    //    angle = halfTurnClockwise;

                    cubies.AddRange(this.GetSliceY(xx, negate, 0));
                    break;
            }

            return cubies;

        }

        internal List<Move> GetTurnMoves(Face face, bool invert, EasingEquation eq, float angle, int duration = 500, int offset = 25)
        {
            var slices = new List<Move>();
            var low = -(int)max;
            var high = (int)max;

            switch (face)
            {
                case Face.Left:
                case Face.Right:
                    for (int j = low; j < high; j++)
                    {
                        var slice = new Move(this, face, j, invert, false, eq, duration);
                        slice.timeoffset = j * offset;
                        slice.lockShift = true;
                        slices.Add(slice);
                    }

                    break;

                case Face.Up:
                case Face.Down:
                    for (int j = low; j < high; j++)
                    {
                        var slice = new Move(this, face, j, invert, false, eq, duration);
                        slice.timeoffset = j * offset;
                        slice.lockShift = true;
                        slices.Add(slice);
                    }

                    break;

                case Face.Front:
                case Face.Back:
                    for (int j = low; j < high; j++)
                    {
                        var slice = new Move(this, face, j, invert, false, eq, duration);
                        slice.timeoffset = j * offset;
                        slice.lockShift = true;
                        slices.Add(slice);
                    }

                    break;
            }

            return slices;
        }

        internal List<Cubie> GetSliceX(float position, float negate, int extraslides)
        {
            var cubies = new List<Cubie>();
            foreach (var cube in this.cubies)
            {
                for (int e = 0; e <= extraslides; e++)
                {
                    if (cube.Position.X == (position + e) * negate)
                        cubies.Add(cube);
                }
            }

            return cubies;
        }

        internal List<Cubie> GetSliceY(float position, float negate, int extraslides)
        {
            var cubies = new List<Cubie>();

            foreach (var cube in this.cubies)
            {
                for (int e = 0; e <= extraslides; e++)
                {
                    if (cube.Position.Y == (position + e) * negate)
                        cubies.Add(cube);
                }
            }

            return cubies;
        }

        internal List<Cubie> GetSliceZ(float position, float negate, int extraslides)
        {
            var cubies = new List<Cubie>();
            foreach (var cube in this.cubies)
            {
                for (int e = 0; e <= extraslides; e++)
                {
                    if (cube.Position.Z == (position + e) * -negate)
                        cubies.Add(cube);
                }
            }

            return cubies;
        }

        public override void Draw()
        {
            if (!started)
                return;

            if (isPlaying)
                this.ViewModel.Time = new DateTime(DateTime.Now.Subtract(startTime).Ticks).ToString("HH:mm:ss.fff");

            // Compute matrices
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1f, aspectRatio, 0.01f, 1000.0f);
            var zoom = ViewModel.CameraZoom * max;

            Matrix view = Matrix.CreateLookAt(new Vector3(zoom, zoom, zoom), new Vector3(0, -1, 0), Vector3.Up);

            bool is3d = this.ViewModel.Use3d;
            bool swap = this.ViewModel.Swap3d;
            float depth = this.ViewModel.Depth3d;

            if (is3d)
            {
                var width = this.GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
                var height = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
                left = new RenderTarget2D(this.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                right = new RenderTarget2D(this.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

                var view1 = view * Matrix.CreateTranslation(new Vector3(-this.ViewModel.Depth3d, 0, 0));
                var view2 = view * Matrix.CreateTranslation(new Vector3(this.ViewModel.Depth3d, 0, 0));

                this.UpdateMouse(view1, projection);
                this.UpdateMouse(view2, projection);

                // Clear surface
                GraphicsDevice.SetRenderTarget(left);
                GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

                if (this.ViewModel.UseReflections)
                    this.DrawCubiesReflections(view1, projection);

                this.DrawCubies(view1, projection);


                GraphicsDevice.SetRenderTarget(right);
                GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

                if (this.ViewModel.UseReflections)
                    this.DrawCubiesReflections(view2, projection);

                this.DrawCubies(view2, projection);


                GraphicsDevice.SetRenderTarget(null);
                using (SpriteBatch sprite = new SpriteBatch(this.GraphicsDevice))
                {
                    sprite.Begin();

                    if (swap)
                    {
                        sprite.Draw(left, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 1);
                        sprite.Draw(right, new Vector2(width, 0), null, Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 1);
                    }
                    else
                    {
                        sprite.Draw(right, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 1);
                        sprite.Draw(left, new Vector2(width, 0), null, Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 1);
                    }
                    sprite.End();
                }
            }
            else
            {
                this.UpdateMouse(view, projection);

                GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

                if (this.ViewModel.UseReflections)
                    this.DrawCubiesReflections(view, projection);

                this.DrawCubies(view, projection);
            }


            this.UpdateMove();
            this.FinishMove();

            if (reset)
            {
                reset = false;
                started = false;

                this.Initialize();
            }

            base.Draw();
        }

        private void DrawCubies(Matrix view, Matrix projection)
        {
            var move = this.moveQueue.FirstOrDefault();

            var cubies = cachedCubies ?? this.cubies;
            // cubies = cubies.OrderBy(c => c.Position.Z).ToList();

            foreach (var cubie in cubies)
            {
                cubie.Size = this.ViewModel.CubieSize;
        

                var ro = Matrix.CreateRotationZ(r);

                var world = 
                    cubie.World   
                     * Matrix.CreateScale(cubie.Size)
                     * Matrix.CreateTranslation(cubie.Position)
                     ;

                if (move != null)
                {
                    foreach (var m in move)
                    {
                        if (m.Cubies != null && m.Cubies.Contains(cubie))
                            world *= m.GetAngle();
                    }
                }

                //world *= Matrix.CreateRotationY(r) 
                //    * Matrix.CreateRotationZ(r);
                //     r += 0.0005f;
                world *= ro;
                // Update per frame parameter values
                //  cubie.Alpha = cubie == pressedCube1 ? 0.5f : 1f;
                cubie.Alpha = 1f;
                cubie.WorldParameter = world;
                cubie.WorldViewProjection = world * view * projection;
                cubie.LightPosition = new Vector3(0, 0.5f, -max * ViewModel.CameraZoom * 2);

                // Drawing the cube
                cubie.Draw(RasterizerState.CullCounterClockwise);
            }
        }

        private void DrawCubiesReflections(Matrix view, Matrix projection)
        {
            if (drawReflections && reflectionAlpha < 1f)
                reflectionAlpha += 0.1f;

            if (reflectionAlpha == 0)
                return;

            var downCubies = this.GetSlice(Face.Down, 0);
            var leftCubies = this.GetSlice(Face.Left, 0);
            var backCubies = this.GetSlice(Face.Back, 0);

            var leftDelta = this.max - this.height;
            var backDelta = this.max - this.depth;

            var cubieSize = this.ViewModel.CubieSize;
            var df = this.ViewModel.DownReflection;
            var lf = this.ViewModel.LeftReflection;
            var bf = this.ViewModel.BackReflection;

            foreach (var cubie in downCubies)
            {
                var world = Matrix.CreateScale(cubieSize)
                    * cubie.World
                    * Matrix.CreateTranslation(cubie.Position)
                    * Matrix.CreateTranslation(new Vector3(0, -df, 0));

                cubie.Alpha = reflectionAlpha;
                cubie.WorldViewProjection = world * view * projection;
                cubie.Draw(RasterizerState.CullNone, Vector3.Down);
            }

            foreach (var cubie in leftCubies)
            {
                var world = Matrix.CreateScale(cubieSize)
                    * cubie.World
                    * Matrix.CreateTranslation(cubie.Position)
                    * Matrix.CreateTranslation(new Vector3(-lf, 0, 0));

                cubie.Alpha = reflectionAlpha;
                cubie.WorldViewProjection = world * view * projection;
                cubie.Draw(RasterizerState.CullNone, Vector3.Left);
            }

            foreach (var cubie in backCubies)
            {
                var world = Matrix.CreateScale(cubieSize)
                    * cubie.World
                    * Matrix.CreateTranslation(cubie.Position)
                    * Matrix.CreateTranslation(new Vector3(0, 0, -bf));

                cubie.Alpha = reflectionAlpha;
                cubie.WorldViewProjection = world * view * projection;
                cubie.Draw(RasterizerState.CullNone, Vector3.Forward);
            }
        }

        private List<Move> UpdateMove()
        {
            var moveGroup = this.moveQueue.FirstOrDefault();
            if (moveGroup != null)
            {
                if (!isMoving)
                {
                    foreach (var move in moveGroup)
                        move.Start();

                    isMoving = true;
                    reflectionAlpha = 0f;
                    drawReflections = false;
                }

                foreach (var move in moveGroup)
                    move.Update();
            }

            return moveGroup;
        }

        private void FinishMove()
        {
            var moveGroup = this.moveQueue.FirstOrDefault();
            if (moveGroup != null)
            {
                var finishedCount = moveGroup.RemoveAll(s => s.isFinished);

                if (moveGroup.Count == 0)
                {
                    if (finishedCount > 0 && isScrambling)
                    {
                        scrambleCount++;
                        Debug.WriteLine(scrambleCount + "-" + scrambleSize + "=" + ((float)scrambleCount / (float)scrambleSize));
                        var percentage = (float)scrambleCount / (float)scrambleSize;
                        this.ViewModel.ScramblePercentage = percentage;
                    }

                    this.moveQueue.Remove(moveGroup);
                    isMoving = false;

                    if (this.moveQueue.Count == 0)
                    {
                        this.drawReflections = true;

                        if (this.isScrambling)
                        {
                            this.isScrambling = false;
                            this.ViewModel.ScramblePercentage = 0;
                            this.ViewModel.Moves = 0;
                            this.StartPlaying();
                        }

                        if (this.ViewModel.OnlyShowFrontFaces)
                            PrepareCubies();
                        else if (cachedCubies != null)
                            cachedCubies = null;

                        if (this.ViewModel.UseStickers)
                        {
                            bool solved = this.IsSolved();
                            if (solved)
                            {
                                Debug.WriteLine("Fully solved: " + solved);
                                this.StopPlaying();
                            }
                        }
                        else
                        {
                            bool solved = this.IsSolved(false);
                            if (solved)
                            {
                                Debug.WriteLine("Colors solved: " + solved);

                                this.StopPlaying();
                            }
                        }
                    }
                }
            }
        }

        private void PrepareCubies()
        {
            var downCubies = this.GetSlice(Face.Front, 0);
            var leftCubies = this.GetSlice(Face.Right, 0);
            var backCubies = this.GetSlice(Face.Up, 0);

            var cubies = new List<Cubie>();
            cubies.AddRange(downCubies);
            cubies.AddRange(leftCubies);
            cubies.AddRange(backCubies);
            cachedCubies = cubies;
        }

        internal string GetFaceConfiguration(Face face, bool checkOrientation = false)
        {
            var unit = Vector3.Zero;
            var unit2 = Vector3.Zero;
            float w = 0;

            var faceslice = this.GetSlice(face, 0);
            switch (face)
            {
                case Face.Down:
                    unit = -Vector3.UnitY;
                    unit2 = -Vector3.UnitX;

                    w = this.width;

                    faceslice = faceslice
                        .OrderByDescending(c => c.Position.Z)
                        .ThenBy(c => c.Position.X)
                    .ToList();

                    break;

                case Face.Up:
                    unit = Vector3.UnitY;
                    unit2 = Vector3.UnitX;

                    w = this.width;

                    faceslice = faceslice
                        .OrderBy(c => c.Position.Z)
                        .ThenBy(c => c.Position.X)
                    .ToList();

                    break;

                case Face.Front:
                    unit = Vector3.UnitZ;
                    unit2 = Vector3.UnitX;
                    w = this.width;

                    faceslice = faceslice
                        .OrderByDescending(c => c.Position.Y)
                        .ThenBy(c => c.Position.X)
                    .ToList();
                    
                    break;

                case Face.Back:
                    unit = -Vector3.UnitZ;
                    unit2 = -Vector3.UnitX;

                     w = this.width;

                    faceslice = faceslice
                        .OrderByDescending(c => c.Position.Y)
                        .ThenByDescending(c => c.Position.X)
                    .ToList();
                    
                    break;

                case Face.Right:
                    unit = Vector3.UnitX;
                    unit2 = Vector3.UnitZ;

                    w = this.depth;

                    faceslice = faceslice
                        .OrderByDescending(c => c.Position.Y)
                        .ThenByDescending(c => c.Position.Z)
                    .ToList();

                    break;

                case Face.Left:
                    unit = -Vector3.UnitX;
                    unit2 = -Vector3.UnitZ;

                       w = this.depth;

                    faceslice = faceslice
                        .OrderByDescending(c => c.Position.Y)
                        .ThenBy(c => c.Position.Z)
                    .ToList();


                    break;
            }


            var sb = new StringBuilder();
            int i = 0;
            foreach (var cubie in faceslice)
            {
                var cubieFaceNormal = cubie.GetFaceNormal(unit);

                sb.Append(cubieFaceNormal.Channel);

                if (++i % w == 0)
                {
                    i = 0;
                    sb.Append(",");
                }
            }

            return sb.ToString().TrimEnd(',');
        }

        internal bool IsFaceSolved(Face face, bool checkOrientation = false)
        {
            bool returnValue = true;

            var unit = Vector3.Zero;
            var unit2 = Vector3.Zero;

            switch (face)
            {
                case Face.Down:
                    unit = -Vector3.UnitY;
                    unit2 = -Vector3.UnitX;
                    break;

                case Face.Up:
                    unit = Vector3.UnitY;
                    unit2 = Vector3.UnitX;
                    break;

                case Face.Front:
                    unit = Vector3.UnitZ;
                    unit2 = Vector3.UnitX;
                    break;

                case Face.Back:
                    unit = -Vector3.UnitZ;
                    unit2 = -Vector3.UnitX;
                    break;

                case Face.Right:
                    unit = Vector3.UnitX;
                    unit2 = Vector3.UnitZ;
                    break;

                case Face.Left:
                    unit = -Vector3.UnitX;
                    unit2 = -Vector3.UnitZ;
                    break;
            }

            var faceslice = this.GetSlice(face, 0);
            var firstCubie = faceslice.First();

            var firstFaceNormal = firstCubie.GetFaceNormal(unit);
            var firstNormal2 = firstCubie.GetFaceNormal(unit2);

            foreach (var cubie in faceslice)
            {
                var cubieFaceNormal = cubie.GetFaceNormal(unit);

                if (checkOrientation)
                {
                    var cubieNormal2 = cubie.GetFaceNormal(unit2);
                    if (firstNormal2.Face != cubieNormal2.Face)
                    {
                        returnValue = false;
                        break;
                    }
                }

                if (returnValue && cubieFaceNormal.Channel != firstFaceNormal.Channel)
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }

        internal bool IsSolved(bool checkOrientation = true)
        {
            var returnValue = true;
            foreach (Face face in Enum.GetValues(typeof(Face)))
            {
                if (!IsFaceSolved(face, checkOrientation))
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }

        internal void TurnFace(Face face, int sliceDepth, bool invert, bool doubleTurn, bool isUndo, EasingEquation eq, int duration = 500, int extraslices = 0)
        {
            bool inverted = invert;
            if (face == Face.Down || face == Face.Back || face == Face.Left)
                inverted = !invert;


            var move = new Move(this, face, sliceDepth, inverted, doubleTurn, eq, duration, extraslices);

            this.moveQueue.Add(new List<Move>() { move });

            if (isUndo)
            {
                this.ViewModel.Moves = Math.Max(0, this.ViewModel.Moves - 1);
                return;
            }

            move.ismove = this.isPlaying;
            //this.ViewModel.Moves = this.ViewModel.Moves + 1;

            //if (face == Face.Down || face == Face.Back || face == Face.Left)
            //    invert = !invert;

            if (face == Face.Up && sliceDepth >= hh)
            {
                face = Face.Down;
                sliceDepth = (int)this.height - sliceDepth - 1;
                invert = !invert;
            }

            if (face == Face.Front && sliceDepth >= hd)
            {
                face = Face.Back;
                sliceDepth = (int)this.depth - sliceDepth - 1;
                invert = !invert;
            }

            if (face == Face.Right && sliceDepth >= hw)
            {
                face = Face.Left;
                sliceDepth = (int)this.width - sliceDepth - 1;
                invert = !invert;
            }

            var letter = face.ToString().First();
            var prime = (invert ? "'" : "");
            var depths = sliceDepth > 0 ? (sliceDepth + 1).ToString() : "";
            var doubleTurns = doubleTurn ? "2" : "";

            var command = depths + letter + prime + doubleTurns;

            ViewModel.commands.Add(command);
            this.ViewModel.RaisePropertyChanged("CommandHistory");
        }

        internal void Twist(Face face, bool isUndo = false, EasingEquation eq = null, int duration = 500)
        {

            if (face == Face.Left || face == Face.Right)
            {
                bool invert = face == Face.Left;
                var angle = invert
                    ? quarterTurnClockwise
                    : -quarterTurnClockwise;

                if (this.width != this.depth)
                {
                    angle *= 2;
                }

                if (isUndo)
                    invert = !invert;


                var slices = this.GetTurnMoves(face, invert, eq, angle, duration);
                this.moveQueue.Add(slices);

                puzzleWorld *= Matrix.CreateRotationY(angle);

                if (isUndo)
                    return;

                var letter = "x";
                var prime = (invert ? "'" : "");

                var command = letter + prime;
                ViewModel.commands.Add(command);
            }

            if (face == Face.Up || face == Face.Down)
            {
                bool invert = face == Face.Up;
                var angle = invert ?
                    quarterTurnClockwise :
                    -quarterTurnClockwise;

                if (this.height != this.depth)
                {
                    angle *= 2;
                }
                if (isUndo)
                    invert = !invert;



                var slices = this.GetTurnMoves(face, invert, eq, angle, duration);
                this.moveQueue.Add(slices);

                puzzleWorld *= Matrix.CreateRotationX(angle);
                if (isUndo) return;

                var letter = "y";
                var prime = (invert ? "'" : "");

                var command = letter + prime;
                ViewModel.commands.Add(command);
            }

            if (face == Face.Front || face == Face.Back)
            {
                bool invert = face == Face.Front;

                var angle = invert ?
                    quarterTurnClockwise :
                    -quarterTurnClockwise;

                if (this.height != this.width)
                {
                    angle *= 2;
                }
                if (isUndo)
                    invert = !invert;


                var slices = this.GetTurnMoves(face, invert, eq, angle, duration);
                this.moveQueue.Add(slices);

                puzzleWorld *= Matrix.CreateRotationZ(angle);
                if (isUndo) return;

                var letter = "z";
                var prime = (invert ? "'" : "");

                var command = letter + prime;
                ViewModel.commands.Add(command);
            }

            this.ViewModel.RaisePropertyChanged("CommandHistory");
        }

        internal void Scramble(string scramble)
        {
            this.ViewModel.ScrambleSize = scramble.Split(' ').Count();
            this.scrambleSize = this.ViewModel.ScrambleSize;

            this.scrambleCount = 0;
            this.isScrambling = true;
            this.isPlaying = false;

            this.Command(scramble, false);
        }

        internal void Scramble()
        {
            var faces = new[] { Face.Up, Face.Down, Face.Left, Face.Right, Face.Front, Face.Back };
            var previousUnit = Vector3.UnitY;

            this.scrambleSize = this.ViewModel.ScrambleSize;
            this.scrambleCount = 0;
            this.isScrambling = true;
            this.isPlaying = false;

            var sb = new StringBuilder();

            for (int i = 0; i < this.scrambleSize; i++)
            {
                var isDoubleT = ViewModel.UseDoubleTurns && RandomBetween(0, 3) == 0;
                var isInvert = ViewModel.UseInvertedTurns && RandomBetween(0, 4) == 0;
                int sliceDepth = 0;

                var unit = previousUnit;
                Face face;

                do
                {
                    var faceId = RandomBetween(0, 5);
                    face = faces[faceId];

                    switch (face)
                    {
                        case Face.Up:
                        case Face.Down:
                            sliceDepth = RandomBetween(0, (int)height);
                            unit = Vector3.UnitY;
                            break;

                        case Face.Left:
                        case Face.Right:
                            sliceDepth = RandomBetween(0, (int)width);
                            unit = Vector3.UnitX;
                            break;

                        case Face.Front:
                        case Face.Back:
                            sliceDepth = RandomBetween(0, (int)depth);
                            unit = Vector3.UnitZ;
                            break;
                    }

                } while (unit == previousUnit);

                previousUnit = unit;

                var letter = face.ToString().First();
                var prime = (isInvert ? "'" : "");
                var depths = sliceDepth > 0 ? sliceDepth.ToString() : "";
                var doubleTurns = isDoubleT ? "2" : "";

                var command = depths + letter + prime + doubleTurns;
                sb.Append(command + " ");
            }

            this.ViewModel.ScrambleText = sb.ToString();
            this.Command(sb.ToString(), false);
        }

        internal void Command(string input, bool isUndo = false)
        {
            // input = input.ToLower();

            var commands = input.Split(' ');
            var eq = this.ViewModel.ScrambleAnimation;
            var duration = this.ViewModel.ScrambleAnimationSpeed;

            foreach (var cmdString in commands)
            {
                var command = cmdString.Trim();
                var invert = false;
                var doubleT = false;
                int d = 0;


                if (command == "x")
                    this.Twist(Face.Right, isUndo, eq, duration * 2);

                if (command == "x'")
                    this.Twist(Face.Left, isUndo, eq, duration * 2);

                if (command == "y")
                    this.Twist(Face.Down, isUndo, eq, duration * 2);

                if (command == "y'")
                    this.Twist(Face.Up, isUndo, eq, duration * 2);

                if (command == "z'")
                    this.Twist(Face.Front, isUndo, eq, duration * 2);

                if (command == "z")
                    this.Twist(Face.Back, isUndo, eq, duration * 2);

                if (command.Contains("'"))
                {
                    command = command.Replace("'", "");
                    invert = !invert;
                }

                if (command.EndsWith("2"))
                {
                    command = command.Substring(0, command.Length - 1);
                    doubleT = true;
                }

                var m = Regex.Match(command, @"\d+");
                if (m.Success)
                    d = Math.Max(Convert.ToInt32(m.Value) - 1, 0);

                if (isUndo)
                    invert = !invert;

                int extra = 0;
                if (command.Contains("w"))
                    extra = 1;

                if (command.Contains("r"))
                    this.TurnFace(Face.Right, d + 1, invert, doubleT, isUndo, eq, duration, 0);

                if (command.Contains("l"))
                    this.TurnFace(Face.Left, d + 1, invert, doubleT, isUndo, eq, duration, 0);

                if (command.Contains("f"))
                    this.TurnFace(Face.Front, d + 1, invert, doubleT, isUndo, eq, duration, 0);

                if (command.Contains("b"))
                    this.TurnFace(Face.Back, d + 1, invert, doubleT, isUndo, eq, duration, 0);

                if (command.Contains("u"))
                    this.TurnFace(Face.Up, d + 1, invert, doubleT, isUndo, eq, duration, 0);

                if (command.Contains("d"))
                    this.TurnFace(Face.Down, d + 1, invert, doubleT, isUndo, eq, duration, 0);


                if (command.Contains("R"))
                    this.TurnFace(Face.Right, d, invert, doubleT, isUndo, eq, duration, extra);

                if (command.Contains("L"))
                    this.TurnFace(Face.Left, d, invert, doubleT, isUndo, eq, duration, extra);

                if (command.Contains("F"))
                    this.TurnFace(Face.Front, d, invert, doubleT, isUndo, eq, duration, extra);

                if (command.Contains("B"))
                    this.TurnFace(Face.Back, d, invert, doubleT, isUndo, eq, duration, extra);

                if (command.Contains("U"))
                    this.TurnFace(Face.Up, d, invert, doubleT, isUndo, eq, duration, extra);

                if (command.Contains("D"))
                    this.TurnFace(Face.Down, d, invert, doubleT, isUndo, eq, duration, extra);



                if (command == "s")
                {
                    bool isSolved = this.IsSolved(false);
                    bool isSolvedOrientation = this.IsSolved();

                    Debug.WriteLine("Colors solved: " + isSolved);
                    Debug.WriteLine("Fully solved: " + isSolvedOrientation);
                }
            }
        }

        internal void Undo()
        {
            if (this.ViewModel.commands.Count > 0)
            {
                var lastIndex = this.ViewModel.commands.Count - 1;
                var command = this.ViewModel.commands[lastIndex];
                this.ViewModel.commands.RemoveAt(lastIndex);

                //if (command.Contains("'"))
                //    command = command.Replace("'", "");
                //else
                //    command += "'";

                this.Command(command, true);

                this.ViewModel.RaisePropertyChanged("CommandHistory");
            }
        }

        internal void StartPlaying()
        {
            if (isPlaying)
            {
                this.StopPlaying();
            }
            else
            {
                //this.ViewModel.Moves = 0;
                startTime = DateTime.Now;
                isPlaying = true;
            }
        }

        private void StopPlaying()
        {
            this.isPlaying = false;
        }

        internal void HandleInput(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Left:
                    this.Command("y");
                    break;
                case System.Windows.Input.Key.Right:
                    this.Command("y'");
                    break;

                case System.Windows.Input.Key.Up:
                    this.Command("x");
                    break;
                case System.Windows.Input.Key.Down:
                    this.Command("x'");
                    break;

                case System.Windows.Input.Key.R:
                    this.Command("U");
                    break;
                case System.Windows.Input.Key.Y:
                    this.Command("U'");
                    break;

                case System.Windows.Input.Key.D:
                    this.Command("D");
                    break;
                case System.Windows.Input.Key.J:
                    this.Command("D'");
                    break;

                case System.Windows.Input.Key.E:
                    this.Command("F'");
                    break;
                case System.Windows.Input.Key.F:
                    this.Command("F");
                    break;

                case System.Windows.Input.Key.U:
                    this.Command("R");
                    break;
                case System.Windows.Input.Key.H:
                    this.Command("R'");
                    break;

            }
        }

        internal void Solve()
        {
            // white
            // yellow
            // green
            // blue
            // red
            // orange

            var ufs = this.GetSlice(Face.Up);
            var mfs = this.GetSlice(Face.Up, 1);
            var dfs = this.GetSlice(Face.Down);
            var lfs = this.GetSlice(Face.Left);
            var rfs = this.GetSlice(Face.Right);
            var ffs = this.GetSlice(Face.Front);
            var bfs = this.GetSlice(Face.Back);
            
            var wc = this.Find(CubieType.Center, Vector3.Up, 0);
            if (!ufs.Contains(wc))
            {
                if (dfs.Contains(wc))
                {
                    this.Command("x' x'");
                    return;
                }
                else if (mfs.Contains(wc))
                {
                    if (ffs.Contains(wc))
                    {
                        this.Command("x'");
                        return;
                    }
                    else if (rfs.Contains(wc))
                    {
                        this.Command("z'");
                        return;
                    }
                    else if (lfs.Contains(wc))
                    {
                        this.Command("z");
                        return;
                    }
                    else if (bfs.Contains(wc))
                    {
                        this.Command("x");
                        return;
                    }
                }
            }

            var rc = this.Find(CubieType.Center, -Vector3.Forward, 4);
            if (!ffs.Contains(rc))
            {
                if (lfs.Contains(rc))
                {
                    this.Command("y");
                    return;
                }
                else if (rfs.Contains(rc))
                {
                    this.Command("y'");
                    return;
                }
                else if (bfs.Contains(rc))
                {
                    this.Command("y' y'");
                    return;
                }
            }
        }

        private Cubie Find(CubieType cubieType, Vector3 unit, int channel)
        {
            for (int i = 0; i < this.cubies.Count; i++)
            {
                if (this.cubies[i].Is(cubieType, unit, channel))
                {
                    return this.cubies[i];
                }
            }

            return null;
        }

        internal void Browse()
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            openFileDialog1.Multiselect = true;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            var textures = new List<Texture2D>();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                // Open the selected file to read.
                var files = openFileDialog1.Files.Count();
                if (files == 6)
                {
                    try
                    {

                        foreach (var file in openFileDialog1.Files)
                        {
                            FileStream fileStream = file.OpenRead();
                            var tex = FromFileStream(this.GraphicsDevice, fileStream);
                            textures.Add(tex);
                            fileStream.Close();
                        }

                        this.Textures = textures;
                        this.ViewModel.IsTexture = true;
                        this.ViewModel.BigStickers = true;
                        this.ViewModel.UseStickers = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Error loading textures. " + Environment.NewLine + ex.ToString(),
                            "CubeKing - Error message", MessageBoxButton.OK);
                    }
                }
                else
                {
                    MessageBox.Show("You must choose exactly 6 images.",
                        "CubeKing - Error message", MessageBoxButton.OK);
                }
            }
        }

        public static Texture2D FromFileStream(GraphicsDevice device, FileStream stream)
        {
            int length = (int)stream.Length;
            byte[] array = new byte[length];
            int count = stream.Read(array, 0, length);
            MemoryStream str = new MemoryStream(array);
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(str);

            return FromBitmapSource(device, bmp);
        }

        public static Texture2D FromBitmapSource(GraphicsDevice device, BitmapSource bmp)
        {
            Texture2D texture; if (bmp.PixelWidth > 2048 && bmp.PixelHeight > 2048)
            {
                texture = new Texture2D(device, 2048, 2048, false, SurfaceFormat.Color);
                bmp.CopyTo(texture, 0, new Rectangle(0, 0, 2048, 2048), 0, 0);
            }
            else
            {
                texture = new Texture2D(device, bmp.PixelWidth, bmp.PixelHeight, false, SurfaceFormat.Color);
                bmp.CopyTo(texture);
            }
            return texture;
        }

        internal string GetConfiguration()
        {
            var sb = new StringBuilder();

            foreach (Face face in Enum.GetValues(typeof(Face)))
            {
                var str = this.GetFaceConfiguration(face, true);
                sb.Append(str + " ");
            }

            return sb.ToString().Trim();
        }

        internal string GetAntiScramble()
        {
            var sb = new StringBuilder();

            foreach (var item in this.ViewModel.commands.Reverse())
            {
                if (item.Contains("'"))
                {
                    sb.Append(item.Replace("'", "") + " ");
                }
                else
                {
                    sb.Append(item + "' ");
                }
            }

            return sb.ToString();
        }
    }
}

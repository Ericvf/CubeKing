using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace CubeKing.Core
{
    public class Move
    {
        const float quarterTurnClockwise = (float)(Math.PI / 2);
        const float quarterTurnCounterClockwise = (float)(Math.PI / -2);
        const float halfTurnClockwise = (float)Math.PI;
        const float halfTurnCounterClockwise = (float)-Math.PI;

        public List<Cubie> Cubies;
        public EasingEquation eq;

        public float rotationAngle;
        public float endRotationAngle;
        public Vector3 rotationVector;

        private DateTime startTime;
        public int duration;
        public int timeoffset;
        private int extraslices;

        float start;
        float offset;
        float target;

        public bool isFinished;
        public bool isStarted;

        private Face face;
        private int depth;
        private bool inverted;
        private bool doubleTurn;
        public bool lockShift = true;
        public bool ismove;

        public CubeScene scene;

        public Move(CubeScene scene, Face face, int depth, bool inverted, bool doubleTurn, EasingEquation eq, int duration, int extraslices = 0)
        {
            
            this.scene = scene;
            this.face = face;
            this.depth = depth;
            this.inverted = !inverted;
            this.doubleTurn = doubleTurn;
            this.duration = duration;
            this.eq = eq;
            this.extraslices = extraslices;
            
        }

        internal void Start()
        {
            var slice = new List<Cubie>();

            //slice.AddRange(this.scene.GetSlice(this.face, this.depth));

            for (int i = this.depth; i <= this.depth + this.extraslices; i++)
            {
                slice.AddRange(this.scene.GetSlice(this.face, i));
            }

            var angle =
              this.inverted ?
              -quarterTurnClockwise :
              quarterTurnClockwise;

            var axis = Vector3.Zero;
            switch (face)
            {
                case Face.Front:
                case Face.Back:
                    axis = Vector3.UnitZ;

                    if (lockShift)
                    {
                        if (this.scene.width != this.scene.height)
                            angle *= 2;
                    }

                    break;

                case Face.Left:
                case Face.Right:
                    axis = Vector3.UnitX;

                    if (lockShift)
                    {
                        if (this.scene.height != this.scene.depth)
                            angle *= 2;
                    }

                    break;

                case Face.Up:
                case Face.Down:
                    axis = Vector3.UnitY;

                    if (lockShift)
                    {
                        if (this.scene.width != this.scene.depth)
                            angle *= 2;
                    }

                    break;
            }

            this.rotationAngle = angle;
            this.rotationVector = axis;
            this.endRotationAngle = angle;

            if (doubleTurn)
            {
                this.endRotationAngle *= 2;
                this.duration *= 2;
            }


            var turnAngle = Matrix.CreateFromAxisAngle(rotationVector, endRotationAngle);

            foreach (var cube in slice)
            {
                var newPosition = Vector3.Transform(cube.Position, turnAngle);

                cube.World *= turnAngle;
                cube.Position = new Vector3(
                        (float)Math.Round(newPosition.X, 1),
                        (float)Math.Round(newPosition.Y, 1),
                        (float)Math.Round(newPosition.Z, 1)
                    );
            }

            this.rotationAngle = -endRotationAngle;
            this.endRotationAngle = 0;

            this.start = this.rotationAngle;
            this.target = endRotationAngle;
            this.offset = target - start;

            this.startTime = DateTime.Now.AddMilliseconds(timeoffset);
            this.Cubies = slice;
        }

        internal void Update()
        {
            if (this.isFinished)
                return;

            if (DateTime.Now < startTime)
                return;

            var t = (DateTime.Now - startTime).TotalMilliseconds;
            rotationAngle = (float)(start + eq(Math.Min(t, duration), 0, offset, duration));

            this.isFinished = (duration == 0 || t >= duration);

            if (this.isFinished)
            {
                rotationAngle = 0;

                if (ismove)
                    this.scene.ViewModel.Moves++;
            }
        }

        internal Matrix GetAngle()
        {
            return Matrix.CreateFromAxisAngle(rotationVector, rotationAngle);
        }
    }

}

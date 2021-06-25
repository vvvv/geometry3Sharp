namespace g3
{
    public readonly struct Frame2d
    {
        public static Frame2d Identity => new Frame2d(Vector2d.Zero, 0);

        /// <summary>
        /// The origin of this frame as an offset from (0, 0)
        /// </summary>
        public Vector2d Origin { get; }

        /// <summary>
        /// The rotation angle of this frame relative to the X-axis in radians
        /// </summary>
        public double Rotation { get; }

        /// <summary>
        /// Map point *into* local coordinates of Frame
        /// </summary>
		public Vector2d ToFrameP(Vector2d point)
        {
            return (point - Origin).RotateByAngleRadians(-Rotation);
        }

        /// <summary>
        /// Map point *from* local frame coordinates into "world" coordinates
        /// </summary>
        public Vector2d FromFrameP(Vector2d point)
        {
            return point.RotateByAngleRadians(Rotation) + Origin;
        }

        /// <summary>
        /// Map vector *into* local coordinates of Frame
        /// </summary>
        public Vector2d ToFrameV(Vector2d vector)
        {
            return vector.RotateByAngleRadians(-Rotation);
        }

        /// <summary>
        /// Map vector *from* local frame coordinates into "world" coordinates
        /// </summary>
        public Vector2d FromFrameV(Vector2d vector)
        {
            return vector.RotateByAngleRadians(Rotation);
        }

        public Frame2d ToFrame(Frame2d frame)
        {
            var origin = frame.ToFrameP(Origin);

            return new Frame2d(origin, Rotation - frame.Rotation);
        }

        /// <summary>
        /// Create a new Frame2d from an origin point and a vector direction
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="direction">Direction to align the positive X-axis of the frame coordinate system to. Does not need to be normalized.</param>
        public Frame2d(Vector2d origin, Vector2d direction) : this(origin, Vector2d.AxisX.SignedAngleRadians(direction.Normalized))
        {
        }

        /// <summary>
        /// Create a new Frame2d from an origin point and an angle
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="angle">Rotation (in radians)</param>
        public Frame2d(Vector2d origin, double angle)
        {
            Origin = origin;
            Rotation = angle;
        }
    }
}
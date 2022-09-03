using System;
using System.Linq;

namespace g3
{
    public abstract class CylindricMeshGenerator : MeshGenerator
    {
        public bool AddSliceWhenOpen = false;
        public float StartAngleDeg = 0.0f;
        public float EndAngleDeg = 360.0f;
        public float Height = 1.0f;
        public int Slices = 16;
        public int Rings = 2;
        public float BaseRadius = 1.0f;
        // set to true if you are going to texture this cylinder, otherwise
        // last panel will not have UVs going from 1 to 0
        public bool NoSharedVertices = false;

        internal int GetSliceCount()
        {
            if (AddSliceWhenOpen)
                return EndAngleDeg - StartAngleDeg == 360 ? Slices : Slices - 1;
            return Slices;
        }

        /// <summary>
        /// Adds a ring of vertices (with uvs and normals) in a cylinder accounting for start and end angles.
        /// </summary>
        /// <param name="radius">Ring's radius</param>
        /// <param name="y">Y coordinate for all ring's vertices (Ring height).</param>
        /// <param name="slope">The ratio between the top and bottom radii, and the height of the cylinder</param>
        /// <param name="startRad">Start angle in radians</param>
        /// <param name="startIndex">Index to use as a starting position to add vertices, UVs and normals</param>
        /// <param name="closed">Boolean specifying if the cylinder has an angled opening (Start and end angles do not describe a single complete circle)</param>
        /// <param name="delta">Radial step to take between each vertex</param>
        /// <param name="ringSize">Amount of radial vertices in the ring</param>
        internal void AddRing(float radius, float y, float slope, float startRad, int startIndex, bool closed, float delta, int ringSize)
        {
            double cosa, sina;
            float angle, t;

            for (int k = 0; k < ringSize; ++k)
            {
                //force EndAngle on last vertex to prevent precission errors
                if (k == ringSize - 1)
                {
                    if (closed)
                    {
                        angle = StartAngleDeg * MathUtil.Deg2Radf;
                    }
                    else
                    {
                        angle = EndAngleDeg * MathUtil.Deg2Radf;
                    }
                }
                else
                {
                    angle = startRad + k * delta;
                }

                cosa = Math.Cos(angle);
                sina = Math.Sin(angle);
                t = (float)k / GetSliceCount();

                vertices[startIndex + k] = new Vector3d(radius * cosa, y, radius * sina);
                uv[startIndex + k] = new Vector2f(1 - t, y / (Height == 0 ? 1.0f : Height));
                Vector3f n = new Vector3f(cosa * Height, slope, sina * Height);
                n.Normalize();
                normals[startIndex + k] = n;
            }
        }

        /// <summary>
        /// Adds a cap (top or bottom) to a cylinder. Includes vertices, uvs, normals and triangles
        /// </summary>
        /// <param name="radius">Cylinder's radius</param>
        /// <param name="startRad">Start angle in radians</param>
        /// <param name="startIndex">Index to use as a starting position to add vertices, UVs and normals</param>
        /// <param name="closed">Boolean specifying if the cylinder has an angled opening (Start and end angles do not describe a single complete circle)</param>
        /// <param name="delta">Radial step to take between each vertex</param>
        /// <param name="triangleIndex">Index to use as a starting position to add triangles</param>
        /// <param name="capType">Used to specify if this is a top or a bottom cap. Bottom is the default</param>
        internal void AddCap(float radius, float startRad, int startIndex, bool closed, float delta, ref int triangleIndex, CapType capType = CapType.Bottom)
        {
            switch (capType)
            {
                case CapType.Top:
                    vertices[startIndex] = new Vector3d(0, Height, 0);
                    normals[startIndex] = new Vector3f(0, 1, 0);
                    break;
                case CapType.Bottom:
                default:
                    vertices[startIndex] = new Vector3d(0, 0, 0);
                    normals[startIndex] = new Vector3f(0, -1, 0);
                    break;
            }

            uv[startIndex] = new Vector2f(0.5f, 0.5f);

            int ringStart = startIndex + 1;
            double sina, cosa;
            float angle;

            for (int k = 0; k < Slices; ++k)
            {
                angle = startRad + k * delta;
                cosa = Math.Cos(angle);
                sina = Math.Sin(angle);
                vertices[ringStart + k] = new Vector3d(radius * cosa, capType == CapType.Bottom ? 0 : Height, radius * sina);
                uv[ringStart + k] = new Vector2f(0.5f * (1.0f + cosa), capType == CapType.Bottom ? 0.5 * (1 + sina) : 1.0f - 0.5 * (1 + sina));
                normals[ringStart + k] = capType == CapType.Bottom ? -Vector3f.AxisY : Vector3f.AxisY;
            }
            append_disc(Slices, startIndex, ringStart, closed, capType == CapType.Bottom ? Clockwise : !Clockwise, ref triangleIndex, 2);
        }

        /// <summary>
        /// Adds inner faces when a cylinder is not closed
        /// </summary>
        /// <param name="vStepSize">Y distance between each ring</param>
        /// <param name="startIndex">Index to use as a starting position to add vertices, UVs and normals</param>
        /// <param name="ringSize">Amount of radial vertices in the ring</param>
        /// <param name="innerFaceUVMode">Used to decide how to calculate inner face UVs</param>
        /// <param name="triangleIndex">Index to use as a starting position to add triangles</param>
        internal void AddInnerFaces(float vStepSize, int startIndex, int ringSize, InnerFaceUVMode innerFaceUVMode, ref int triangleIndex)
        {
            float ringBottom = 0;
            float ySpan = Height == 0 ? 1.0f : Height;
            float yb, yt, xb, xt;
            
            // amount to increase/decrease radius by on each ring starting from the base
            float radiusStep = BaseRadius / (Rings - 1);

            float currentRadius = BaseRadius;

            for (int i = 1; i < Rings; i++)
            {
                yb = vStepSize * (i - 1) / ySpan;
                yt = vStepSize * i / ySpan;
                xb = innerFaceUVMode == InnerFaceUVMode.Cone ? currentRadius / BaseRadius : 1;
                xt = innerFaceUVMode == InnerFaceUVMode.Cone ? (currentRadius - radiusStep) / BaseRadius : 1;            

                //rectangle 1
                vertices[startIndex] = new Vector3d(0, ringBottom, 0); //bottom 
                vertices[startIndex + 1] = new Vector3d(0, ringBottom + vStepSize, 0); //top
                vertices[startIndex + 2] = vertices[ringSize * i]; //top-a
                vertices[startIndex + 3] = vertices[ringSize * (i - 1)]; //a

                //rectangle 2
                vertices[startIndex + 4] = new Vector3d(0, ringBottom + vStepSize, 0); //top
                vertices[startIndex + 5] = new Vector3d(0, ringBottom, 0); //bottom
                vertices[startIndex + 6] = vertices[ringSize * i - 1]; //b
                vertices[startIndex + 7] = vertices[ringSize * (i + 1) - 1]; //top-b

                //these indexes will result in proper normal estimation in all cases except when bottom radius is 0 (they work for top radius == 0)
                int vil0 = 0, vil1 = 1, vir0 = 5, vir1 = 6;

                //bottom radius == 0
                if (currentRadius == 0 && i == 1)
                {
                    //since we add a rectangle below using these vertices but on this case only one triangle has an area (the other one collapses due to radius of 0)
                    //we need to make sure we use the right indexes to estimate the normal. If we pick the collapsed triangle in the rectangle the estimated normal is 0,0,0
                    vil0 = 1;
                    vil1 = 2;
                    vir0 = 4;
                    vir1 = 5;
                }

                normals[startIndex] = estimate_normal(startIndex + vil0, startIndex + vil1, startIndex + 3);
                normals[startIndex + 1] = estimate_normal(startIndex + vil0, startIndex + vil1, startIndex + 3);
                normals[startIndex + 2] = estimate_normal(startIndex + vil0, startIndex + vil1, startIndex + 3);
                normals[startIndex + 3] = estimate_normal(startIndex + vil0, startIndex + vil1, startIndex + 3);
                normals[startIndex + 4] = estimate_normal(startIndex + vir0, startIndex + vir1, startIndex + 7);
                normals[startIndex + 5] = estimate_normal(startIndex + vir0, startIndex + vir1, startIndex + 7);
                normals[startIndex + 6] = estimate_normal(startIndex + vir0, startIndex + vir1, startIndex + 7);
                normals[startIndex + 7] = estimate_normal(startIndex + vir0, startIndex + vir1, startIndex + 7);


                uv[startIndex] = new Vector2f(0, yb); //vertex:bottom uv:bottom-left
                uv[startIndex + 1] = new Vector2f(0, yt); //vertex:top uv:top-left
                uv[startIndex + 2] = new Vector2f(xt, yt); //vertex:top-a uv:top-right
                uv[startIndex + 3] = new Vector2f(xb, yb); //vertex:a uv:bottom-right
                uv[startIndex + 4] = new Vector2f(0, yt); //vertex:top uv:top-left
                uv[startIndex + 5] = new Vector2f(0, yb); //vertex:bottom uv:bottom-left
                uv[startIndex + 6] = new Vector2f(xb, yb); //vertex:b uv:bottom-right
                uv[startIndex + 7] = new Vector2f(xt, yt); //vertex:top-b uv:top-right

                append_rectangle(startIndex + 0, startIndex + 1, startIndex + 2, startIndex + 3, !Clockwise, ref triangleIndex, 4);
                append_rectangle(startIndex + 4, startIndex + 5, startIndex + 6, startIndex + 7, !Clockwise, ref triangleIndex, 5);

                ringBottom += vStepSize;
                startIndex += 8;
                currentRadius -= radiusStep;
            }
        }

        /// <summary>
        /// Adds cylinder panel triangles (slope triangles)
        /// </summary>
        /// <param name="ringSize">Amount of radial vertices in the ring</param>
        /// <param name="triangleIndex">Index to use as a starting position to add triangles</param>
        internal void AddCylinderPanels(int ringSize, ref int triangleIndex)
        {
            for (int k = 0; k < ringSize - 1; ++k)
            {
                for (int i = 0; i < Rings - 1; i++)
                {
                    var k1 = k + ringSize * i;
                    groups[triangleIndex] = 1;
                    triangles.Set(triangleIndex++, k1, k1 + 1, ringSize + k1, Clockwise);
                    groups[triangleIndex] = 1;
                    triangles.Set(triangleIndex++, k1 + 1, ringSize + k1 + 1, ringSize + k1, Clockwise);
                }
            }
        }

        internal enum CapType
        {
            Bottom,
            Top
        }

        internal enum InnerFaceUVMode
        {
            Cylinder,
            Cone
        }
    }

    /// <summary>
    /// Generate a Cylinder without caps. Supports sections of cylinder (eg wedges) as well as
    /// vertical divisions (Rings). Curently UV islands are overlapping for different mesh 
    /// components, if NoSharedVertices
    /// Positioned along Y axis such that base-center is at Origin, and top is at Y=Height
    /// You get a cone unless BaseRadius = TopRadius
    /// </summary>
    public class OpenCylinderGenerator : CylindricMeshGenerator
    {
        public float TopRadius = 1.0f;

        override public MeshGenerator Generate()
        {
            bool closed = EndAngleDeg - StartAngleDeg == 360;
            int ringSize = (NoSharedVertices && closed) ? Slices + 1 : Slices;
            vertices = new VectorArray3d(ringSize * Rings);
            uv = new VectorArray2f(vertices.Count);
            normals = new VectorArray3f(vertices.Count);
            triangles = new IndexArray3i(2 * Slices * Rings);
            groups = new int[triangles.Count];

            float totalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.Deg2Radf;
            float startRad = StartAngleDeg * MathUtil.Deg2Radf;

            // Y distance between each ring
            float vStepSize = Height / (Rings - 1);
            // amount to increase/decrease radius by on each ring starting from the base
            float radiusStep = (BaseRadius - TopRadius) / (Rings - 1);

            float currentRadius = BaseRadius;
            float slope = BaseRadius - TopRadius / Height;
            float delta = (float)totalRange / GetSliceCount();

            for (int i = 0; i < Rings; i++)
            {
                AddRing(i == Rings - 1 ? TopRadius : currentRadius, i == Rings - 1 ? Height : vStepSize * i, slope, startRad, i * ringSize, closed, delta, ringSize);
                currentRadius -= radiusStep;
            }

            // generate cylinder panels
            int ti = 0;
            AddCylinderPanels(ringSize, ref ti);

            // close disc if we went all the way
            if (closed && NoSharedVertices == false)
            {
                groups[ti] = 1;
                triangles.Set(ti++, ringSize - 1, 0, ringSize, Clockwise);
                groups[ti] = 1;
                triangles.Set(ti++, ringSize - 1, ringSize, 2 * ringSize - 1, Clockwise);
            }

            return this;
        }
    }




    /// <summary>
    /// Generate a Cylinder with caps. Supports sections of cylinder (eg wedges) as well as
    /// vertical divisions (Rings). Curently UV islands are overlapping for different mesh 
    /// components, if NoSharedVertices
    /// Positioned along Y axis such that base-center is at Origin, and top is at Y=Height
    /// You get a cone unless BaseRadius = TopRadius
    /// No subdivisions along top/base rings or height steps.
    /// cylinder triangles have groupid = 1, top cap = 2, bottom cap = 3, wedge faces 5 and 6
    /// </summary>
    public class CappedCylinderGenerator : CylindricMeshGenerator
    {
        public float TopRadius = 1.0f;

        override public MeshGenerator Generate()
        {
            bool closed = EndAngleDeg - StartAngleDeg == 360;
            int ringSize = (NoSharedVertices && closed) ? Slices + 1 : Slices;
            int capVertices = (NoSharedVertices) ? Slices + 1 : 1;
            int faceVertices = (NoSharedVertices && closed == false) ? 8 * (Rings - 1) : 0;
            vertices = new VectorArray3d(ringSize * Rings + 2 * capVertices + faceVertices);
            uv = new VectorArray2f(vertices.Count);
            normals = new VectorArray3f(vertices.Count);

            int cylTris = 2 * Slices * (Rings - 1);
            int capTris = 2 * Slices;
            int faceTris = (closed == false) ? 2 * 2 * (Rings - 1) : 0;
            triangles = new IndexArray3i(cylTris + capTris + faceTris);
            groups = new int[triangles.Count];

            float totalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.Deg2Radf;
            float startRad = StartAngleDeg * MathUtil.Deg2Radf;

            // Y distance between each ring
            float vStepSize = Height / (Rings - 1);
            // amount to increase/decrease radius by on each ring starting from the base
            float radiusStep = (BaseRadius - TopRadius) / (Rings - 1);

            float currentRadius = BaseRadius;
            float slope = BaseRadius - TopRadius / Height;
            float delta = (float)totalRange / GetSliceCount();

            for (int i = 0; i < Rings; i++)
            {
                AddRing(i == Rings - 1 ? TopRadius : currentRadius, i == Rings - 1 ? Height : vStepSize * i, slope, startRad, i * ringSize, closed, delta, ringSize);
                currentRadius -= radiusStep;
            }

            int ti = 0;

            AddCylinderPanels(ringSize, ref ti);

            // close disc if we went all the way
            if (closed && NoSharedVertices == false)
            {
                groups[ti] = 1;
                triangles.Set(ti++, ringSize - 1, 0, ringSize, Clockwise);
                groups[ti] = 1;
                triangles.Set(ti++, ringSize - 1, ringSize, 2 * ringSize - 1, Clockwise);
            }

            int bottomCap = ringSize * Rings;
            int topCap = ringSize * Rings + 1;
            //the fact that the bottom and top cap's center vertices is no longer added at this point is braking shared vertices caps

            if (NoSharedVertices)
            {
                AddCap(BaseRadius, startRad, ringSize * Rings, closed, delta, ref ti);
                AddCap(TopRadius, startRad, ringSize * Rings + 1 + Slices, closed, delta, ref ti, CapType.Top);
                
                int startF = ringSize * Rings + 2 + 2 * Slices;
                if (closed == false)
                {
                    AddInnerFaces(vStepSize, startF, ringSize, InnerFaceUVMode.Cylinder, ref ti);
                }
            }
            else
            {
                append_disc(Slices, bottomCap, 0, closed, Clockwise, ref ti, 2);
                append_disc(Slices, topCap, ringSize, closed, !Clockwise, ref ti, 3);
                if (closed == false)
                {
                    append_rectangle(bottomCap, 0, ringSize, topCap, Clockwise, ref ti, 4);
                    append_rectangle(ringSize - 1, bottomCap, topCap, 2 * ringSize - 1, Clockwise, ref ti, 5);
                }
            }
            return this;
        }
    }




    // Generate a cone with base cap. Supports sections of cone (eg wedges) as well as
    // vertical divisions (Rings). Curently UV islands are overlapping for different mesh components, if NoSharedVertices
    // Also, if NoSharedVertices, then the 'tip' vertex is duplicated Slices times.
    // This causes the normals to look...weird.
    // For the conical region, we use the planar disc parameterization (ie tip at .5,.5) rather than
    // a cylinder-like projection
    public class ConeGenerator : CylindricMeshGenerator
    {
        public SlopeUVMode SlopeUVMode = SlopeUVMode.OnShape;

        override public MeshGenerator Generate()
        {
            bool closed = EndAngleDeg - StartAngleDeg == 360;
            int ringSize = (NoSharedVertices && closed) ? Slices + 1 : Slices;
            int tipVertices = (NoSharedVertices) ? ringSize : 1;
            int capVertices = (NoSharedVertices) ? Slices + 1 : 1;
            int faceVertices = (NoSharedVertices && closed == false) ? 8 * (Rings - 1) : 0; //these are the "inner faces" resulting from opening the cone up using angles
            vertices = new VectorArray3d(ringSize * (Rings - 1) + tipVertices + capVertices + faceVertices);
            uv = new VectorArray2f(vertices.Count);
            normals = new VectorArray3f(vertices.Count);

            int coneTris = (NoSharedVertices) ? 2 * Slices * (Rings - 1) : Slices;
            int capTris = Slices;
            int faceTris = (closed == false) ? 2 * 2 * (Rings - 1) : 0; //2 faces, 2 triangles per face per (Rings - 1)
            triangles = new IndexArray3i(coneTris + capTris + faceTris);
            groups = new int[triangles.Count];

            float totalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.Deg2Radf;
            float startRad = StartAngleDeg * MathUtil.Deg2Radf;
            float delta = totalRange / GetSliceCount();

            float ySpan = Height;
            if (ySpan == 0)
                ySpan = 1.0f;

            // Y distance between each ring
            float vStepSize = Height / (Rings - 1);
            // amount to increase/decrease radius by on each ring starting from the base
            float radiusStep = BaseRadius / (Rings - 1);

            // generate rings
            for (int k = 0; k < ringSize; ++k)
            {
                float angle = startRad + (float)k * delta;
                double cosa = Math.Cos(angle), sina = Math.Sin(angle);
                float t = (float)k / (float)GetSliceCount();
                float topUVStep = t - 1.0f / (GetSliceCount() * 2.0f);

                float currentRadius = BaseRadius;
                for (int i = 0; i < Rings; i++)
                {

                    float yt = vStepSize * i / ySpan;
                    vertices[ringSize * i + k] = new Vector3d(currentRadius * cosa, vStepSize * i, currentRadius * sina);
                    // UV
                    switch (SlopeUVMode)
                    {
                        case SlopeUVMode.SideProjected:
                            if (i == (Rings - 1))
                            {
                                uv[ringSize * i + k - 1] = new Vector2f(1.0f - topUVStep, yt);
                            }
                            else
                            {
                                uv[ringSize * i + k] = new Vector2f(1 - t, yt);
                            }
                            break;
                        case SlopeUVMode.OnShape:
                        default:
                            uv[ringSize * i + k] = new Vector2f(0.5f * (1 + (currentRadius / BaseRadius) * cosa), 1 - 0.5 * (1 + (currentRadius / BaseRadius) * sina));
                            break;
                    }
                    Vector3f n = new Vector3f(cosa * Height, BaseRadius / Height, sina * Height);
                    n.Normalize();
                    normals[ringSize * i + k] = n;
                    currentRadius -= radiusStep;
                }
            }
            if (NoSharedVertices == false)
            {
                vertices[ringSize] = new Vector3d(0, Height, 0);
                normals[ringSize] = Vector3f.AxisY;//TODO: verify normal calculation
                uv[ringSize] = new Vector2f(0.5f, 0.5f);//TODO: verify uv calculation
            }

            int ti = 0;
            if (NoSharedVertices)
            {
                AddCylinderPanels(ringSize, ref ti);

            }
            else
                append_disc(Slices, ringSize, 0, closed, !Clockwise, ref ti);

            int nBottomC = ringSize * Rings;

            if (NoSharedVertices)
            {
                AddCap(BaseRadius, startRad, ringSize * Rings, closed, delta, ref ti);

                if (closed == false)
                {
                    AddInnerFaces(vStepSize, nBottomC + 1 + Slices, ringSize, InnerFaceUVMode.Cone, ref ti);
                }
            }
            else
            {
                append_disc(Slices, nBottomC, 0, closed, Clockwise, ref ti);
                if (closed == false)
                {
                    triangles.Set(ti++, nBottomC, ringSize, 0, !Clockwise);
                    triangles.Set(ti++, nBottomC, ringSize, ringSize - 1, Clockwise);
                }
            }

            return this;
        }
    }

    public enum SlopeUVMode
    {
        OnShape,
        SideProjected
    }

    public class VerticalGeneralizedCylinderGenerator : MeshGenerator
    {
        public CircularSection[] Sections;
        public int Slices = 16;
        public bool Capped = true;

        // set to true if you are going to texture this cone or want sharp edges
        public bool NoSharedVertices = true;

        public int startCapCenterIndex = -1;
        public int endCapCenterIndex = -1;

        override public MeshGenerator Generate()
        {
            int nRings = (NoSharedVertices) ? 2 * (Sections.Length - 1) : Sections.Length;
            int nRingSize = (NoSharedVertices) ? Slices + 1 : Slices;
            int nCapVertices = (NoSharedVertices) ? Slices + 1 : 1;
            if (Capped == false)
                nCapVertices = 0;
            vertices = new VectorArray3d(nRings * nRingSize + 2 * nCapVertices);
            uv = new VectorArray2f(vertices.Count);
            normals = new VectorArray3f(vertices.Count);

            int nSpanTris = (Sections.Length - 1) * (2 * Slices);
            int nCapTris = (Capped) ? 2 * Slices : 0;
            triangles = new IndexArray3i(nSpanTris + nCapTris);

            float fDelta = (float)((Math.PI * 2.0) / Slices);

            float fYSpan = Sections.Last().SectionY - Sections[0].SectionY;
            if (fYSpan == 0)
                fYSpan = 1.0f;

            // generate top and bottom rings for cylinder
            int ri = 0;
            for (int si = 0; si < Sections.Length; ++si)
            {
                int nStartR = ri * nRingSize;
                float y = Sections[si].SectionY;
                float yt = (y - Sections[0].SectionY) / fYSpan;
                for (int j = 0; j < nRingSize; ++j)
                {
                    int k = nStartR + j;
                    float angle = (float)j * fDelta;
                    double cosa = Math.Cos(angle), sina = Math.Sin(angle);
                    vertices[k] = new Vector3d(Sections[si].Radius * cosa, y, Sections[si].Radius * sina);
                    float t = (float)j / (float)(Slices - 1);
                    uv[k] = new Vector2f(t, yt);
                    Vector3f n = new Vector3f((float)cosa, 0, (float)sina);
                    n.Normalize();
                    normals[k] = n;
                }
                ri++;
                if (NoSharedVertices && si != 0 && si != Sections.Length - 1)
                {
                    duplicate_vertex_span(nStartR, nRingSize);
                    ri++;
                }
            }

            // generate triangles
            int ti = 0;
            ri = 0;
            for (int si = 0; si < Sections.Length - 1; ++si)
            {
                int r0 = ri * nRingSize;
                int r1 = r0 + nRingSize;
                ri += (NoSharedVertices) ? 2 : 1;
                for (int k = 0; k < nRingSize - 1; ++k)
                {
                    triangles.Set(ti++, r0 + k, r0 + k + 1, r1 + k, Clockwise);
                    triangles.Set(ti++, r0 + k + 1, r1 + k + 1, r1 + k, Clockwise);
                }
                if (NoSharedVertices == false)
                {      // close disc if we went all the way
                    triangles.Set(ti++, r1 - 1, r0, r1, Clockwise);
                    triangles.Set(ti++, r1 - 1, r1, r1 + nRingSize - 1, Clockwise);
                }
            }

            if (Capped)
            {
                // add endcap verts
                var s0 = Sections[0];
                var sN = Sections.Last();
                int nBottomC = nRings * nRingSize;
                vertices[nBottomC] = new Vector3d(0, s0.SectionY, 0);
                uv[nBottomC] = new Vector2f(0.5f, 0.5f);
                normals[nBottomC] = new Vector3f(0, -1, 0);
                startCapCenterIndex = nBottomC;

                int nTopC = nBottomC + 1;
                vertices[nTopC] = new Vector3d(0, sN.SectionY, 0);
                uv[nTopC] = new Vector2f(0.5f, 0.5f);
                normals[nTopC] = new Vector3f(0, 1, 0);
                endCapCenterIndex = nTopC;

                if (NoSharedVertices)
                {
                    int nStartB = nTopC + 1;
                    for (int k = 0; k < Slices; ++k)
                    {
                        float a = (float)k * fDelta;
                        double cosa = Math.Cos(a), sina = Math.Sin(a);
                        vertices[nStartB + k] = new Vector3d(s0.Radius * cosa, s0.SectionY, s0.Radius * sina);
                        uv[nStartB + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
                        normals[nStartB + k] = -Vector3f.AxisY;
                    }
                    append_disc(Slices, nBottomC, nStartB, true, Clockwise, ref ti);

                    int nStartT = nStartB + Slices;
                    for (int k = 0; k < Slices; ++k)
                    {
                        float a = (float)k * fDelta;
                        double cosa = Math.Cos(a), sina = Math.Sin(a);
                        vertices[nStartT + k] = new Vector3d(sN.Radius * cosa, sN.SectionY, sN.Radius * sina);
                        uv[nStartT + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
                        normals[nStartT + k] = Vector3f.AxisY;
                    }
                    append_disc(Slices, nTopC, nStartT, true, !Clockwise, ref ti);

                }
                else
                {
                    append_disc(Slices, nBottomC, 0, true, Clockwise, ref ti);
                    append_disc(Slices, nTopC, nRingSize * (Sections.Length - 1), true, !Clockwise, ref ti);
                }
            }

            return this;
        }
    }



}

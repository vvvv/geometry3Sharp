using System;

namespace g3
{
    // generate a triangle fan, no subdvisions
    public class TrivialDiscGenerator : MeshGenerator
    {
        public float Radius = 1.0f;
        public float StartAngleDeg = 0.0f;
        public float EndAngleDeg = 360.0f;
        public int Slices = 32;
        public bool AddSliceWhenOpen = false;

        private int GetSliceCount()
        {
            if (AddSliceWhenOpen)
                return EndAngleDeg - StartAngleDeg == 360 ? Slices : Slices - 1;
            return Slices;
        }

        override public MeshGenerator Generate()
        {
            vertices = new VectorArray3d(Slices + 1);
            uv = new VectorArray2f(Slices + 1);
            normals = new VectorArray3f(Slices + 1);
            triangles = new IndexArray3i(Slices);

            int vi = 0;
            vertices[vi] = Vector3d.Zero;
            uv[vi] = new Vector2f(0.5f, 0.5f);
            normals[vi] = Vector3f.AxisY;
            vi++;

            bool bFullDisc = ((EndAngleDeg - StartAngleDeg) > 359.99f);
            float fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.Deg2Radf;
            float fStartRad = StartAngleDeg * MathUtil.Deg2Radf;
            float fDelta = fTotalRange / GetSliceCount();
            for (int k = 0; k < Slices; ++k)
            {
                float a = fStartRad + (float)k * fDelta;
                double cosa = Math.Cos(a), sina = Math.Sin(a);
                vertices[vi] = new Vector3d(Radius * cosa, 0, Radius * sina);
                uv[vi] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
                normals[vi] = Vector3f.AxisY;
                vi++;
            }

            int ti = 0;
            for (int k = 1; k < Slices; ++k)
                triangles.Set(ti++, k, 0, k + 1, Clockwise);
            if (bFullDisc)      // close disc if we went all the way
                triangles.Set(ti++, Slices, 0, 1, Clockwise);

            return this;
        }
    }



    // generate a triangle fan, no subdvisions
    public class PuncturedDiscGenerator : FlatMeshGenerator
    {
        public float OuterRadius = 1.0f;
        public float InnerRadius = 0.5f;
        public float StartAngleDeg = 0.0f;
        public float EndAngleDeg = 360.0f;
        public int Slices = 32;
        public bool AddSliceWhenOpen = false;

        private int GetSliceCount()
        {
            if (AddSliceWhenOpen)
                return EndAngleDeg - StartAngleDeg == 360 ? Slices : Slices - 1;
            return Slices;
        }

        override public MeshGenerator Generate()
        {
            var count = GenerateBackFace ? 4 * Slices : 2 * Slices;
            vertices = new VectorArray3d(count);
            uv = new VectorArray2f(count);
            normals = new VectorArray3f(count);
            triangles = new IndexArray3i(count);

            bool bFullDisc = ((EndAngleDeg - StartAngleDeg) > 359.99f);
            float fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.Deg2Radf;
            float fStartRad = StartAngleDeg * MathUtil.Deg2Radf;
            float fDelta = fTotalRange / GetSliceCount();
            float fUVRatio = InnerRadius / OuterRadius;
            for (int k = 0; k < Slices; ++k)
            {
                float angle = fStartRad + k * fDelta;
                double cosa = Math.Cos(angle), sina = Math.Sin(angle);
                vertices[k] = new Vector3d(InnerRadius * cosa, 0, InnerRadius * sina);
                vertices[Slices + k] = new Vector3d(OuterRadius * cosa, 0, OuterRadius * sina);

                double uvY1, uvY2;
                switch (TextureSpace)
                {
                    case TextureSpace.DirectX:
                        uvY1 = 0.5f * (1.0f - fUVRatio * sina);
                        uvY2 = 0.5f * (1.0f - sina); 
                        break;
                    case TextureSpace.OpenGL:
                    default:
                        uvY1 = 0.5f * (1.0f + fUVRatio * sina);
                        uvY2 = 0.5f * (1.0f + sina);
                        break;
                }
                uv[k] = new Vector2f(0.5f * (1.0f + fUVRatio * cosa), uvY1);//1.0f - 0.5 * (1 + sina)
                uv[Slices + k] = new Vector2f(0.5f * (1.0f + cosa), uvY2);

                switch (Normal)
                {
                    default:
                    case NormalDirection.UpZ: normals[k] = normals[Slices + k] = Vector3f.AxisZ; break;
                    case NormalDirection.UpY: normals[k] = normals[Slices + k] = Vector3f.AxisY; break;
                    case NormalDirection.UpX: normals[k] = normals[Slices + k] = Vector3f.AxisX; break;
                }
            }

            int ti = 0;
            for (int k = 0; k < Slices - 1; ++k)
            {
                triangles.Set(ti++, k, k + 1, Slices + k + 1, Clockwise);
                triangles.Set(ti++, k, Slices + k + 1, Slices + k, Clockwise);
            }
            if (bFullDisc)
            {      
                // close disc if we went all the way
                triangles.Set(ti++, Slices - 1, 0, Slices, Clockwise);
                triangles.Set(ti++, Slices - 1, Slices, 2 * Slices - 1, Clockwise);
            }

            return this;
        }
    }


}

namespace g3
{
    public abstract class FlatMeshGenerator : MeshGenerator
    {
        public bool GenerateBackFace = false;
        public TextureSpace TextureSpace = TextureSpace.OpenGL;
        public NormalDirection Normal = NormalDirection.UpY;
    }

    public enum TextureSpace
    {
        DirectX,
        OpenGL
    }

    public enum NormalDirection
    {
        UpZ,
        UpY,
        UpX
    }
}

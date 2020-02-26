using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace g3 {
    public class ThreeMFReader : IMeshReader {

        [XmlRoot(ElementName = "model", Namespace = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02")]
        public struct Model {
            [XmlElement(ElementName = "resources")]
            public Resources resources;
        }

        public struct Resources {
            [XmlElement(ElementName = "object")]
            public Object[] objects;
        }

        public struct Object {
            [XmlElement(ElementName = "mesh")]
            public Mesh mesh;
        }

        public struct Mesh {
            [XmlElement(ElementName = "vertices")]
            public Vertices vertices;
            [XmlElement(ElementName = "triangles")]
            public Triangles triangles;
        }

        public struct Vertices {
            [XmlElement(ElementName = "vertex")]
            public Vertex[] vertices;
        }

        public struct Vertex {
            [XmlAttribute(AttributeName = "x")]
            public double x;
            [XmlAttribute(AttributeName = "y")]
            public double y;
            [XmlAttribute(AttributeName = "z")]
            public double z;
        }

        public struct Triangles {
            [XmlElement(ElementName = "triangle")]
            public Triangle[] triangles;
        }

        public struct Triangle {
            [XmlAttribute(AttributeName = "v1")]
            public int v1;
            [XmlAttribute(AttributeName = "v2")]
            public int v2;
            [XmlAttribute(AttributeName = "v3")]
            public int v3;
        }

        public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder) {
            try {
                using (var archive = new ZipArchive(reader.BaseStream)) {
                    foreach (var entry in archive.Entries) {
                        if (entry.Name == "3dmodel.model") {
                            var result = Read(new StreamReader(entry.Open()), options, builder);
                            if (result.code != IOCode.Ok)
                                return result;
                        }
                    }
                }
            } catch (Exception e) {
                return new IOReadResult(IOCode.FileParsingError, e.ToString());
            }
            return IOReadResult.Ok;
        }

        public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder) {
            XmlSerializer serializer = new XmlSerializer(typeof(Model));
            Model model;
            try {
                model = (Model)serializer.Deserialize(reader);
            } catch (Exception e) {
                return new IOReadResult(IOCode.FileParsingError, $"XML parsing error: {e}");
            }
            
            foreach (var @object in model.resources.objects) {
                var mesh = @object.mesh;
                int meshId = builder.AppendNewMesh(false, false, false, true);
                var mapV = new int[mesh.vertices.vertices.Length];
                for (int k = 0; k < mesh.vertices.vertices.Length; ++k) {
                    var vertex = mesh.vertices.vertices[k];
                    mapV[k] = builder.AppendVertex(vertex.x, vertex.y, vertex.z);
                }
                for (int j = 0; j < mesh.triangles.triangles.Length; ++j) {
                    var triangle = mesh.triangles.triangles[j];
                    builder.AppendTriangle(mapV[triangle.v1], mapV[triangle.v2], mapV[triangle.v3]);
                }
            }

            return IOReadResult.Ok;
        }
    }
}

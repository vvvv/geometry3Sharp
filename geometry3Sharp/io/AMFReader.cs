using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

namespace g3 {

    public class AMFReader : IMeshReader {

        [XmlRoot(ElementName = "amf")]
        public struct AMF {
            [XmlElement(ElementName = "object")]
            public Object[] objects;
        }

        public struct Object {
            [XmlElement(ElementName = "mesh")]
            public Mesh[] meshes;
        }

        public struct Mesh {
            [XmlElement(ElementName = "vertices")]
            public Vertices vertices;
            [XmlElement(ElementName = "volume")]
            public Volume[] volumes;
        }

        public struct Vertices {
            [XmlElement(ElementName = "vertex")]
            public Vertex[] vertices;
        }

        public struct Vertex {
            [XmlElement(ElementName = "coordinates")]
            public Coordinates coordinates;
        }

        public struct Coordinates {
            [XmlElement(ElementName = "x")]
            public double x;
            [XmlElement(ElementName = "y")]
            public double y;
            [XmlElement(ElementName = "z")]
            public double z;
        }

        public struct Volume {
            [XmlElement(ElementName = "triangle")]
            public Triangle[] triangles;
        }

        public struct Triangle {
            [XmlElement(ElementName = "v1")]
            public int v1;
            [XmlElement(ElementName = "v2")]
            public int v2;
            [XmlElement(ElementName = "v3")]
            public int v3;
        }

        public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder) {

            try {
                using (var archive = new ZipArchive(reader.BaseStream)) {
                    var entries = archive.Entries;
                    if (entries.Count != 1) {
                        return new IOReadResult(IOCode.FileParsingError, $"Wrong nuumber of entries in AMF zip archive. Expected 1, got {entries.Count}.");
                    }
                    using (var file = entries[0].Open()) {
                        return Read(new StreamReader(file), options, builder);
                    }
                }
            } catch (Exception e) {
                return new IOReadResult(IOCode.FileParsingError, e.ToString());
            }
        }

        public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder) {
            XmlSerializer serializer = new XmlSerializer(typeof(AMF));
            AMF amf;
            try {
                amf = (AMF)serializer.Deserialize(reader);
            } catch (Exception e){
                return new IOReadResult(IOCode.FileParsingError, $"XML parsing error: {e}");
            }

            foreach (var @object in amf.objects) {
                foreach (var mesh in @object.meshes) {
                    int meshId = builder.AppendNewMesh(false, false, false, true);
                    var mapV = new int[mesh.vertices.vertices.Length];
                    for (int k = 0; k < mesh.vertices.vertices.Length; ++k) {
                        var coordinates = mesh.vertices.vertices[k].coordinates;
                        mapV[k] = builder.AppendVertex(coordinates.x, coordinates.y, coordinates.z);
                    }
                    for (int i = 0; i < mesh.volumes.Length; ++i) {
                        for (int j = 0; j < mesh.volumes[i].triangles.Length; ++j) {
                            var triangle = mesh.volumes[i].triangles[j];
                            builder.AppendTriangle(mapV[triangle.v1], mapV[triangle.v2], mapV[triangle.v3], i);
                        }
                    }
                }
            }

            return IOReadResult.Ok;
        }
    }
}

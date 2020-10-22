using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Numerics;
using System.Linq;
using System.Globalization;

namespace KclLibrary
{
    /// <summary>
    /// Represents a 3D model stored in the Wavefront OBJ format.
    /// </summary>
    public class ObjModel
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private static readonly char[] _argSeparators = new char[] { ' ' };
        private static readonly char[] _vertexSeparators = new char[] { '/' };

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjModel"/> class.
        /// </summary>
        public ObjModel()
        {
            Meshes = new List<ObjMesh>();
            Materials = new List<ObjMaterial>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjModel"/> class from the given stream.
        /// </summary>
        /// <param name="stream">The stream from which the instance will be loaded.</param>
        public ObjModel(Stream stream)
        {
            Load(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjModel"/> class from the file with the given name.
        /// </summary>
        /// <param name="fileName">The name of the file from which the instance will be loaded.</param>
        public ObjModel(string fileName)
        {
            Load(fileName);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the list of meshes of the model.
        /// </summary>
        public List<ObjMesh> Meshes { get; set; }

        /// <summary>
        /// Gets or sets the list of materials of the model.
        /// </summary>
        public List<ObjMaterial> Materials { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a global list of all the triangles in the object file.
        /// </summary>
        /// <returns></returns>
        public List<Triangle> ToTriangles()
        {
            DebugLogger.WriteLine($"Creating triangle list....");

            List<ushort> attributes = new List<ushort>();

            List<Triangle> triangles = new List<Triangle>();
            foreach (var mesh in Meshes)
            {
                foreach (var face in mesh.Faces)
                {
                    if (!attributes.Contains(face.CollisionAttribute))
                        attributes.Add(face.CollisionAttribute);

                    var triangle = new Triangle();
                    triangle.Attribute = face.CollisionAttribute;
                    triangle.Vertices = new Vector3[3];
                    for (int i = 0; i < face.Vertices.Length; i++)
                        triangle.Vertices[i] = face.Vertices[i].Position;

                    triangles.Add(triangle);
                }
            }

            return triangles;
        }

        public string[] GetMeshNameList()
        {
            List<string> meshNames = new List<string>();
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (!meshNames.Contains(Meshes[i].Name))
                    meshNames.Add(Meshes[i].Name);
            }
            return meshNames.ToArray();
        }

        public string[] GetMaterialNameList()
        {
            List<string> materialNames = new List<string>();
            for (int i = 0; i < Meshes.Count; i++)
            {
                for (int f = 0; f < Meshes[i].Faces.Count; f++)
                {
                    if (!materialNames.Contains(Meshes[i].Faces[f].Material))
                        materialNames.Add(Meshes[i].Faces[f].Material);
                }
            }
            return materialNames.ToArray();
        }

        /// <summary>
        /// Loads the object file data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after loading the instance.
        /// </param>
        public void Load(Stream stream)
        {
            DebugLogger.WriteLine($"Loading obj file....");

            Meshes = new List<ObjMesh>();
            Materials = new List<ObjMaterial>();

            ObjMesh currentMesh = new ObjMesh("Mesh");

            HashSet<string> faceHashes = new HashSet<string>();

            Dictionary<ObjFace, int> faceDupes = new Dictionary<ObjFace, int>();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                List<Vector3> Positions = new List<Vector3>();
                List<Vector2> TexCoords = new List<Vector2>();
                List<Vector3> Normals = new List<Vector3>();

                var enusculture = new CultureInfo("en-US");
                string currentMaterial = null;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = line.Replace(",", ".");

                    // Ignore empty lines and comments.
                    if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    string[] args = line.Split(_argSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length == 1)
                        continue;

                    switch (args[0])
                    {
                        case "o":
                        case "g":
                            currentMesh = new ObjMesh(args.Length > 1 ? args[1] : $"Mesh{Meshes.Count}");
                            Meshes.Add(currentMesh);
                            continue;
                        case "v":
                            Positions.Add(new Vector3(
                                Single.Parse(args[1], enusculture),
                                Single.Parse(args[2], enusculture),
                                Single.Parse(args[3], enusculture)));
                            continue;
                        case "vt":
                            TexCoords.Add(new Vector2(Single.Parse(args[1], enusculture), Single.Parse(args[2], enusculture)));
                            continue;
                        case "vn":
                            Normals.Add(new Vector3(Single.Parse(args[1], enusculture), Single.Parse(args[2], enusculture),
                                Single.Parse(args[3])));
                            continue;
                        case "f":
                            if (args.Length != 4)
                                throw new Exception("Obj must be trianglulated!");

                            int[] indices = new int[3 * 2]; //3 faces, position and normal indices

                            // Only support triangles for now.
                            ObjFace face = new ObjFace() { Vertices = new ObjVertex[3] };
                            face.Material = currentMaterial;
                            for (int i = 0; i < face.Vertices.Length; i++)
                            {
                                string[] vertexArgs = args[i + 1].Split(_vertexSeparators, StringSplitOptions.None);
                                int positionIndex = Int32.Parse(vertexArgs[0]) - 1;

                                face.Vertices[i].Position = Positions[positionIndex];

                                if (float.IsNaN(face.Vertices[i].Position.X) ||
                                    float.IsNaN(face.Vertices[i].Position.Y) ||
                                    float.IsNaN(face.Vertices[i].Position.Z))
                                {
                                    face.Vertices = null;
                                    break;
                                }

                                if (vertexArgs.Length > 1 && vertexArgs[1] != String.Empty)
                                {
                                    face.Vertices[i].TexCoord = TexCoords[Int32.Parse(vertexArgs[1]) - 1];
                                }
                                if (vertexArgs.Length > 2 && vertexArgs[2] != String.Empty)
                                {
                                    face.Vertices[i].Normal = Normals[Int32.Parse(vertexArgs[2]) - 1];
                                }
                            }

                            string faceStr = face.ToString();
                            if (faceHashes.Contains(faceStr))
                                continue;

                            faceHashes.Add(faceStr);

                            if (face.Vertices != null)
                                currentMesh.Faces.Add(face);
                            continue;
                        case "usemtl":
                            {
                                if (args.Length < 2) continue;
                                currentMaterial = args[1];
                                continue;
                            }
                    }
                }
            }

            Console.WriteLine($"FACE COUNT {currentMesh.Faces.Count}");

            faceDupes.Clear();

            if (Meshes.Count == 0) //No object or groups present, use one single mesh
                Meshes.Add(currentMesh);
        }

        /// <summary>
        /// Loads the obj material data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after loading the instance.
        /// </param>
        public void LoadMTL(Stream stream, bool leaveOpen = false)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.Default, true, 81920, leaveOpen))
            {
                ObjMaterial currentMaterial = null;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    // Ignore empty lines and comments.
                    if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    string[] args = line.Split(_argSeparators, StringSplitOptions.RemoveEmptyEntries);
                    switch (args[0])
                    {
                        case "newmtl": //New Material
                            currentMaterial = new ObjMaterial();
                            currentMaterial.Name = args[1];
                            break;
                        case "Ka": //Ambient Color
                            currentMaterial.Ambient = new Vector3(
                                float.Parse(args[1]),
                                float.Parse(args[2]),
                                float.Parse(args[3]));
                            break;
                        case "Kd": //Diffuse Color
                            currentMaterial.Diffuse = new Vector3(
                                float.Parse(args[1]),
                                float.Parse(args[2]),
                                float.Parse(args[3]));
                            break;
                        case "Ks": //Specular Color
                            currentMaterial.Specular = new Vector3(
                                float.Parse(args[1]),
                                float.Parse(args[2]),
                                float.Parse(args[3]));
                            break;
                        case "map_Kd ": //Diffuse Map
                            currentMaterial.DiffuseTexture = args[1];
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Saves the material data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// </param>
        public void SaveMTL(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default))
            {
                foreach (var material in Materials)
                {
                    writer.WriteLine($"newmtl {material.Name}");
                    if (material.Diffuse != null)
                        writer.WriteLine($"Kd {material.Diffuse.X} {material.Diffuse.Y} {material.Diffuse.Z}");
                    if (material.Ambient != null)
                        writer.WriteLine($"Ka {material.Ambient.X} {material.Ambient.Y} {material.Ambient.Z}");
                    if (material.Specular != null)
                        writer.WriteLine($"Ks {material.Specular.X} {material.Specular.Y} {material.Specular.Z}");
                    if (material.DiffuseTexture != null)
                        writer.WriteLine($"map_Kd  {material.DiffuseTexture}");
                }
            }
        }

        /// <summary>
        /// Saves the data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// </param>
        public void Save(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default))
            {
                int positionShift = 1;
                int normalShift = 1;
                foreach (var mesh in Meshes)
                {
                    Dictionary<string, int> positionTable = new Dictionary<string, int>();
                    Dictionary<string, int> normalTable = new Dictionary<string, int>();

                    List<Vector3> positons = new List<Vector3>();
                    List<Vector3> normals = new List<Vector3>();

                    writer.WriteLine($"o {mesh.Name}");
                    foreach (var face in mesh.Faces)
                    {
                        foreach (var v in face.Vertices)
                        {
                            string positionKey = v.Position.ToString();
                            string normalKey = v.Normal.ToString();
                            string texCoordKey = v.TexCoord.ToString();

                            if (!positionTable.ContainsKey(positionKey))
                            {
                                positionTable.Add(positionKey, positons.Count);
                                positons.Add(v.Position);
                            }

                            if (!normalTable.ContainsKey(normalKey))
                            {
                                normalTable.Add(normalKey, normals.Count);
                                normals.Add(v.Normal);
                            }
                        }
                    }

                    foreach (var pos in positons)
                        writer.WriteLine($"v {pos.X} {pos.Y} {pos.Z}");
                    foreach (var nrm in normals)
                        writer.WriteLine($"vn {nrm.X} {nrm.Y} {nrm.Z}");

                    string currentMaterial = "";
                    foreach (var face in mesh.Faces.OrderBy(x => x.Material))
                    {
                        if (face.Material != currentMaterial)
                        {
                            currentMaterial = face.Material;
                            writer.WriteLine($"usemtl {currentMaterial}");
                        }

                        string faceData = "f";
                        foreach (var v in face.Vertices)
                        {
                            int positionIndex = positionShift + positionTable[v.Position.ToString()];
                            int normalIndex = normalShift + normalTable[v.Normal.ToString()];
                            faceData += " " + string.Join("//", new string[] { positionIndex.ToString(), normalIndex.ToString() });
                        }
                        writer.WriteLine(faceData);
                    }

                    positionShift += positons.Count;
                    normalShift += normals.Count;
                }
            }
        }

        /// <summary>
        /// Loads the data from the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Load(string fileName) {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(stream);
            }
        }


        /// <summary>
        /// Saves the data to the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Save(string fileName, bool saveMTL = true) {
            if (saveMTL)
                SaveMTL(fileName.Replace(".obj", ".mtl"));
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Saves the material data to the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void SaveMTL(string fileName) {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                SaveMTL(stream);
            }
        }
    }

    /// <summary>
    /// Represents a material in an <see cref="ObjModel"/>.
    /// </summary>
    public class ObjMaterial
    {
        /// <summary>
        /// Gets or sets the name of the material.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the diffuse texture.
        /// </summary>
        public string DiffuseTexture { get; set; }

        /// <summary>
        /// Gets or sets the diffuse color.
        /// </summary>
        public Vector3 Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the amient color.
        /// </summary>
        public Vector3 Ambient { get; set; }

        /// <summary>
        /// Gets or sets the specular color.
        /// </summary>
        public Vector3 Specular { get; set; }
    }

    /// <summary>
    /// Represents a mesh in an <see cref="ObjModel"/>.
    /// </summary>
    public class ObjMesh
    {
        /// <summary>
        /// Gets or sets the list of faces of the mesh.
        /// </summary>
        public List<ObjFace> Faces { get; set; }

        public string Name { get; set; }

        public ObjMesh(string name)
        {
            Name = name;
            Faces = new List<ObjFace>();
        }
    }

    /// <summary>
    /// Represents a triangle in an <see cref="ObjMesh"/>.
    /// </summary>
    public struct ObjFace
    {
        /// <summary>
        /// The material used for this face.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// The attribute ID used to assign material information to a collision face.
        /// </summary>
        public ushort CollisionAttribute { get; set; }

        /// <summary>
        /// The three <see cref="ObjVertex"/> vertices which define this triangle.
        /// </summary>
        public ObjVertex[] Vertices;

        public override string ToString() {
            string f = CollisionAttribute.ToString();
            for (int i = 0; i < Vertices.Length; i++)
                f += Vertices[i].ToString();
            return f;
        }

        public ObjFace(ObjFace face, ushort value)
        {
            Vertices = face.Vertices;
            Material = face.Material;
            CollisionAttribute = value;
        }
    }

    /// <summary>
    /// Represents the indices required to define a vertex of an <see cref="ObjModel"/>.
    /// </summary>
    public struct ObjVertex
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The vertex position from the positions array of the owning <see cref="ObjModel"/>.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex texture coordinates from the texture coordinate array of the owning <see cref="ObjModel"/>.
        /// </summary>
        public Vector2 TexCoord;

        /// <summary>
        /// The vertex normal from the normal array of the owning <see cref="ObjModel"/>.
        /// </summary>
        public Vector3 Normal;

        public override string ToString() {
            return $"{Position}_{Normal}";
        }
    }

    public class ObjFaceComparer : IEqualityComparer<ObjFace>
    {
        public bool Equals(ObjFace x, ObjFace y)
        {
            if (x.GetHashCode() != y.GetHashCode())
                return false;

            return true;
        }

        public int GetHashCode(ObjFace obj) {
            return obj.GetHashCode();
        }
    }
}

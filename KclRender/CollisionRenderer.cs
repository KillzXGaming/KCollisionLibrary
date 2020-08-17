using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KclLibrary;
using OpenTK;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;

namespace CollisionGUI
{
    public class CollisionRenderer : GenericRenderer
    {
        public KCLFile KclFile { get; set; }

        private VertexArrayObject vao;

        public Dictionary<ushort, Color> ColorList = new Dictionary<ushort, Color>();

        public CollisionRenderer(string fileName) :
            base(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            KclFile = new KCLFile(fileName);
            KclFile.Transform = System.Numerics.Matrix4x4.CreateScale(0.01f);

            Random r = new Random();
            foreach (var model in KclFile.Models)
            {
                foreach (var prisim in model.Prisms)
                {
                }
            }
        }

        public int IndicesLength;
        public int IndicesSelectionLength;

        public ShaderProgram defaultShaderProgram;
        public ShaderProgram solidColorShaderProgram;

        ShaderProgram Shader;

        public override void Prepare(GL_ControlModern control)
        {
            if (Shader != null && Shader.programs.ContainsKey(control)) return;

            var solidColorFrag = new FragmentShader(
           @"#version 330
                in vec3 position;
                in vec3 normal;
                in vec4 color;

                uniform int selectionOverride;
                uniform int colorOverride;

				out vec4 FragColor;

				void main(){
                    vec4 highlighted = vec4(1);
                     if (selectionOverride == 1) {
                         highlighted = vec4(1,0,0,1);
                      }

                    vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
                    float halfLambert = max(displayNormal.y,0.5);

                    FragColor = vec4(vec3(color.rgb * halfLambert), 1.0f) * highlighted;
                    if (colorOverride == 1)
                        FragColor = vec4(0,0,0,1);
				}");

            var solidColorVert = new VertexShader(
          @"#version 330
                layout(location = 0) in vec3 vPosition;
                layout(location = 1) in vec3 vNormal;
                layout(location = 2) in vec4 vColor;
                layout(location = 3) in float vIndex;

                out vec3 position;
                out vec3 normal;
                out vec4 color;

	            uniform mat4 mtxMdl;
				uniform mat4 mtxCam;

				void main(){
	                position = vPosition;
                    normal = vNormal;
	                color = vColor;

                    gl_Position = mtxCam * mtxMdl * vec4(vPosition.xyz, 1.0);
				}");

            Shader = new ShaderProgram(solidColorFrag, solidColorVert, control);
            PrepareModel(control);
        }

        private int vaoBuffer;
        private int iboBuffer;
        private int iboSelBuffer;

        public void PrepareModel(GL_ControlBase control)
        {
            int[] buffers = new int[3];

            //Load vao
            GL.GenBuffers(3, buffers);
            vaoBuffer = buffers[0];
            iboBuffer = buffers[1];
            iboSelBuffer = buffers[2];

            vao = new VertexArrayObject(vaoBuffer);
            vao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, 32, 0);
            vao.AddAttribute(1, 3, VertexAttribPointerType.Float, false, 32, 12);
            vao.AddAttribute(2, 4, VertexAttribPointerType.UnsignedByte, true, 32, 24);
            vao.AddAttribute(3, 1, VertexAttribPointerType.Float, false, 32, 28);
            vao.Initialize(control);

            UpdateVertexData();
        }

        public void Destroy() {
            GL.DeleteBuffer(vaoBuffer);
            GL.DeleteBuffer(iboBuffer);
            GL.DeleteBuffer(iboSelBuffer);
        }

        public void UpdateIndexBuffer()
        {
            List<int> indexBuffer = new List<int>();
            List<int> selIndexBuffer = new List<int>();

            int faceIndex = 0;
            foreach (var model in KclFile.Models)
            {
                for (int i = 0; i < model.Prisms.Length; i++)
                {
                    if (model.HitPrisms.Contains(model.Prisms[i]))
                    {
                        selIndexBuffer.Add(faceIndex++);
                        selIndexBuffer.Add(faceIndex++);
                        selIndexBuffer.Add(faceIndex++);
                    }
                    else
                    {
                        indexBuffer.Add(faceIndex++);
                        indexBuffer.Add(faceIndex++);
                        indexBuffer.Add(faceIndex++);
                    }
                }
            }

            IndicesLength = indexBuffer.Count;
            IndicesSelectionLength = selIndexBuffer.Count;

            GL.BindBuffer(BufferTarget.ArrayBuffer, iboBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(indexBuffer.Count * sizeof(int)),
             indexBuffer.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, iboSelBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(selIndexBuffer.Count * sizeof(int)),
             selIndexBuffer.ToArray(), BufferUsageHint.StaticDraw);
        }

        public void UpdateVertexData()
        {
            List<uint> indexList = new List<uint>();
            List<float> vertexData = new List<float>();
            uint faceIndex = 0;
            foreach (var model in KclFile.Models)
            {
                foreach (var face in model.Prisms)
                {
                    var triangle = model.GetTriangle(face);

                    for (int i = 0; i < 3; i++)
                    {
                        vertexData.Add(triangle.Vertices[i].X);
                        vertexData.Add(triangle.Vertices[i].Y);
                        vertexData.Add(triangle.Vertices[i].Z);
                        vertexData.Add(triangle.Normal.X);
                        vertexData.Add(triangle.Normal.Y);
                        vertexData.Add(triangle.Normal.Z);

                        Vector4 color = new Vector4(255, 255, 255,255);
                        if (ColorList.ContainsKey(triangle.Attribute)) {
                            var col = ColorList[triangle.Attribute];
                            color = new Vector4(col.R, col.G, col.B, col.A);
                        }

                        vertexData.Add(BitConverter.ToSingle(new byte[4]
                        {
                                (byte)color.X,
                                (byte)color.Y,
                                (byte)color.Z,
                                (byte)color.W
                        }, 0));
                        vertexData.Add(faceIndex);

                        if (i % 3 == 0)
                            faceIndex++;
                    }
                }
            }

            float[] bufferData = vertexData.ToArray();

            UpdateIndexBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.Length * 4, bufferData, BufferUsageHint.StaticDraw);
        }   

        public override void DrawModel(GL_ControlModern control, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {
            if (pass != Pass.OPAQUE || Shader == null)
                return;

            var modelMatrix = new Matrix4(
                 KclFile.Transform.M11, KclFile.Transform.M21, KclFile.Transform.M31, KclFile.Transform.M41,
                 KclFile.Transform.M12, KclFile.Transform.M22, KclFile.Transform.M32, KclFile.Transform.M42,
                 KclFile.Transform.M13, KclFile.Transform.M23, KclFile.Transform.M33, KclFile.Transform.M43,
                 KclFile.Transform.M14, KclFile.Transform.M24, KclFile.Transform.M34, KclFile.Transform.M44);

            control.CurrentShader = Shader;
            control.ApplyModelTransform(modelMatrix);

            Draw(control);

            Matrix4 camMat = modelMatrix * control.CameraMatrix * control.ProjectionMatrix;
            DrawOctrees(ref camMat);
        }

        private void DrawOctrees(ref Matrix4 mvp)
        {
            foreach (var model in KclFile.Models)
            {
                if (model.HitOctrees.Count > 0)
                {
                    var boundings = model.GetOctreeBoundings();
                    var hitOctrees = model.HitOctrees;
                    foreach (var bounding in boundings)
                    {
                        foreach (var octree in hitOctrees)
                        {
                            if (octree == bounding.Octree)
                            {
                                Vector3 pos = new Vector3(bounding.Position.X, bounding.Position.Y, bounding.Position.Z);
                                Vector3 bsize = new Vector3(bounding.Size);
                                DrawableBoundingBox.DrawBoundingBox(mvp, bsize, pos + bsize, System.Drawing.Color.Red);
                            }
                        }
                    }
                }
            }

            return;

            var octreeMax = KclFile.MaxCoordinate;
            var octreeOrigin = KclFile.MinCoordinate;
            Vector3 max = new Vector3((float)octreeMax.X, (float)octreeMax.Y, (float)octreeMax.Z);
            Vector3 min = new Vector3((float)octreeOrigin.X, (float)octreeOrigin.Y, (float)octreeOrigin.Z);
            Vector3 size = max - min;

            Vector3 boxSize = new Vector3(
                (1 << (int)KclFile.CoordinateShift.X),
                (1 << (int)KclFile.CoordinateShift.Y),
                (1 << (int)KclFile.CoordinateShift.Z));

            Console.WriteLine($"boxSize {boxSize} SIZE {max - min}");

            DrawSubdivision(ref mvp, min, boxSize / 2f, KclFile.ModelOctreeRoot.Children, 0);
        }

        private void DrawSubdivision(ref Matrix4 mvp, Vector3 position, Vector3 boxSize, ModelOctreeNode[] modelOctrees, int subdiv)
        {
            int index = 0;
            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        Vector3 cubePosition = position + boxSize * new Vector3(x, y, z);

                        if (modelOctrees[index].Children == null)
                            DrawableBoundingBox.DrawBoundingBox(mvp, boxSize / 2f, cubePosition + boxSize / 2f, System.Drawing.Color.Red);
                        else
                            DrawSubdivision(ref mvp, cubePosition, boxSize / 2f, modelOctrees[index].Children, subdiv++);

                        index++;
                    }
                }
            }
        }

        private void Draw(GL_ControlBase control)
        {
            if (KclFile.Models.Sum(x => x.HitPrisms.Count) > 0 || IndicesSelectionLength > 0)
                UpdateIndexBuffer();

            GL.Disable(EnableCap.CullFace);

            vao.Enable(control);
            vao.Use(control);

            Shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            Draw();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            Shader.SetInt("colorOverride", 0);

            Draw();
            GL.Enable(EnableCap.CullFace);
        }

        private void Draw()
        {
            if (IndicesLength > 0)
            {
                Shader.SetInt("selectionOverride", 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboBuffer);
                GL.DrawElements(BeginMode.Triangles, IndicesLength, DrawElementsType.UnsignedInt, 0);
            }
            if (IndicesSelectionLength > 0)
            {
                Shader.SetInt("selectionOverride", 1);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboSelBuffer);
                GL.DrawElements(BeginMode.Triangles, IndicesSelectionLength, DrawElementsType.UnsignedInt, 0);
                Shader.SetInt("selectionOverride", 0);
            }
        }
    }
}

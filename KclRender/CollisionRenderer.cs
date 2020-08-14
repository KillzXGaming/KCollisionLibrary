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
using OpenTK;
using GL_EditorFramework.Interfaces;
using GL_EditorFramework.EditorDrawables;

namespace CollisionGUI
{
    public class CollisionRenderer : GenericRenderer
    {
        public KCLFile KclFile { get; set; }

        private VertexArrayObject vao;

        public CollisionRenderer(string fileName) :
            base(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            KclFile = new KCLFile(fileName);
        }

        public int IndicesLength;

        public ShaderProgram defaultShaderProgram;
        public ShaderProgram solidColorShaderProgram;

        ShaderProgram Shader;

        public override void Prepare(GL_ControlModern control)
        {
            var solidColorFrag = new FragmentShader(
           @"#version 330
                in vec3 normal;
                in vec3 position;
                in vec4 color;

				out vec4 FragColor;

				void main(){
                    vec3 displayNormal = (normal.xyz * 0.5) + 0.5;
                    float halfLambert = max(displayNormal.y,0.5);

                    FragColor = vec4(vec3(color.rgb * halfLambert), 1.0f);
				}");

            var solidColorVert = new VertexShader(
          @"#version 330
                in vec3 vPosition;
                in vec3 vNormal;
                in vec4 vColor;

                out vec3 normal;
                out vec3 position;
                out vec4 color;

	            uniform mat4 mtxMdl;
				uniform mat4 mtxCam;

				void main(){
                    normal = vNormal;
	                position = vPosition;
	                color = vColor;

                    gl_Position = mtxCam * mtxMdl * vec4(vPosition.xyz, 1.0);
				}");

            Shader = new ShaderProgram(solidColorFrag, solidColorVert, control);
            PrepareModel(control);
        }

        private int vaoBuffer;

        public void PrepareModel(GL_ControlBase control)
        {
            //Load vao
            vaoBuffer = GL.GenBuffer();

            vao = new VertexArrayObject(vaoBuffer);
            vao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, 28, 0);
            vao.AddAttribute(1, 3, VertexAttribPointerType.Float, false, 28, 12);
            vao.AddAttribute(2, 4, VertexAttribPointerType.UnsignedByte, true, 28, 24);
            vao.Initialize(control);

            UpdateVertexData();
        }

        public void Destroy() {
            GL.DeleteBuffer(vaoBuffer);
        }

        public void UpdateVertexData()
        {
            List<uint> indexList = new List<uint>();
            List<float> vertexData = new List<float>();
            uint index = 0;
            foreach (var model in KclFile.Models)
            {
                foreach (var face in model.Prisims)
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
                        if (model.HitPrisims.Contains(face))
                            color = new Vector4(255, 0, 0, 255);

                        vertexData.Add(BitConverter.ToSingle(new byte[4]
                        {
                                (byte)color.X,
                                (byte)color.Y,
                                (byte)color.Z,
                                (byte)color.W
                        }, 0));

                        indexList.Add(index++);
                    }
                }
            }

            uint[] indices = indexList.ToArray();
            float[] bufferData = vertexData.ToArray();

            IndicesLength = indices.Length;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.Length * 4, bufferData, BufferUsageHint.StaticDraw);
        }

        public override void DrawModel(GL_ControlModern control, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {
            if (pass != Pass.OPAQUE)
                return;

            var modelMatrix = new Matrix4(
                 KclFile.Transform.M11, KclFile.Transform.M21, KclFile.Transform.M31, KclFile.Transform.M41,
                 KclFile.Transform.M12, KclFile.Transform.M22, KclFile.Transform.M32, KclFile.Transform.M42,
                 KclFile.Transform.M13, KclFile.Transform.M23, KclFile.Transform.M33, KclFile.Transform.M43,
                 KclFile.Transform.M14, KclFile.Transform.M24, KclFile.Transform.M34, KclFile.Transform.M44);

            control.CurrentShader = Shader;
            control.ApplyModelTransform(modelMatrix);

            Draw(control);

            GL.UseProgram(0);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            Matrix4 camMat = modelMatrix * control.CameraMatrix * control.ProjectionMatrix;
            DrawOctrees(ref camMat);
        }

        private void DrawOctrees(ref Matrix4 mvp)
        {
            var boundings = KclFile.Models[0].GetOctreeBoundings();
            var hitOctrees = KclFile.Models[0].HitOctrees;
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
            GL.Disable(EnableCap.CullFace);

            vao.Enable(control);
            vao.Use(control);
            GL.DrawArrays(PrimitiveType.Triangles, 0, IndicesLength);

            GL.Enable(EnableCap.CullFace);
        }
    }
}

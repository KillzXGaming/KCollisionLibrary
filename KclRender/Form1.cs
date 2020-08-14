using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using KclLibrary;
using GL_EditorFramework.StandardCameras;

namespace CollisionGUI
{
    public partial class Form1 : Form
    {
        public GL_ControlModern glControl;

        public Form1()
        {
            InitializeComponent();

            glControl = new GL_ControlModern() { Dock = DockStyle.Fill };
            panel1.Controls.Add(glControl);
        }

        List<TestObject> collidableObjects = new List<TestObject>();

        EditorScene Scene;

        public KCLFile KclFile { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            Renderers.ColorBlockRenderer.Initialize(glControl);

            Scene = new EditorScene();

            glControl.MouseMove += glControl_MouseMove;
            glControl.MainDrawable = Scene;
        }

        private void glControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (KclFile == null || Scene.SelectedObjects.Count == 0) return;

            foreach (var obj in collidableObjects) {
                var hit = KclFile.CheckHit(new System.Numerics.Vector3(obj.CurrentPosition.X, obj.CurrentPosition.Y, obj.CurrentPosition.Z));
                if (hit != null)
                    obj.IsColliding = true;
                else
                    obj.IsColliding = false;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                foreach (var ob in Scene.objects)
                {
                    if (ob is CollisionRenderer)
                        ((CollisionRenderer)ob).Destroy();
                }

                Scene.objects.Clear();
                collidableObjects.Clear();

                TestObject obj = new TestObject();
                collidableObjects.Add(obj);
                Scene.objects.Add(obj);

                CollisionRenderer collisonRenderer = new CollisionRenderer(ofd.FileName);
                Scene.objects.Add(collisonRenderer);
                collisonRenderer.Prepare(glControl);

                KclFile = collisonRenderer.KclFile;

                glControl.Invalidate();
            }
        }

        class TestObject : TransformableObject
        {
            public Vector4 CollideColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

            public bool IsColliding = false;

            public Vector3 CurrentPosition;

            protected override Vector4 Color => IsColliding ? CollideColor : base.Color;

            public TestObject() : base(Vector3.Zero, Vector3.Zero, Vector3.One)
            {

            }

            public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
            {
                if (pass == Pass.TRANSPARENT)
                    return;

                if (!ObjectRenderState.ShouldBeDrawn(this))
                    return;

                bool hovered = editorScene.Hovered == this;

                Matrix3 rotMtx = GlobalRotation;

                CurrentPosition = Selected ? editorScene.CurrentAction.NewPos(Position) : Position;

                control.UpdateModelMatrix(
                    Matrix4.CreateScale((Selected ? editorScene.CurrentAction.NewScale(Scale, rotMtx) : Scale) * BoxScale) *
                    new Matrix4(Selected ? editorScene.CurrentAction.NewRot(rotMtx) : rotMtx) *
                    Matrix4.CreateTranslation(CurrentPosition));

                Vector4 blockColor;
                Vector4 lineColor;

                if (hovered && Selected)
                    lineColor = hoverSelectColor;
                else if (Selected)
                    lineColor = selectColor;
                else if (hovered)
                    lineColor = hoverColor;
                else
                    lineColor = Color;

                if (hovered && Selected)
                    blockColor = Color * 0.5f + hoverSelectColor * 0.5f;
                else if (Selected)
                    blockColor = Color * 0.5f + selectColor * 0.5f;
                else if (hovered)
                    blockColor = Color * 0.5f + hoverColor * 0.5f;
                else
                    blockColor = Color;

                Renderers.ColorBlockRenderer.Draw(control, pass, blockColor, lineColor, control.NextPickingColor());

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;

namespace CollisionGUI
{
    public class GenericRenderer : TransformableObject
    {
        public new static Vector4 selectColor = new Vector4(EditableObject.selectColor.Xyz, 0.5f);
        public new static Vector4 hoverSelectColor = new Vector4(EditableObject.hoverSelectColor.Xyz, 0.5f);
        public new static Vector4 hoverColor = new Vector4(EditableObject.hoverColor.Xyz, 0.125f);

        [PropertyCapture.Undoable]
        public Vector3 ObjectTransform { get; set; }

        public PickingMode PickingAction = PickingMode.Object;

        public override Vector3 GlobalPosition
        {
            get { return Position; }
            set
            {
                Position = value;
                DisplayTranslation = value;
            }
        }

        public override Vector3 GlobalScale
        {
            get { return Scale; }
            set
            {
                Scale = value;
                DisplayScale = value;
            }
        }

        public enum PickingMode
        {
            Object, //Selects entire model
            MeshGroup, //Selects a group of meshes
            Mesh, //Selects per mesh
            Material, //Selects per material
            Face,
        }

        [PropertyCapture.Undoable]
        public virtual Vector3 DisplayRotation { get; set; }
        [PropertyCapture.Undoable]
        public virtual Vector3 DisplayTranslation { get; set; }
        [PropertyCapture.Undoable]
        public virtual Vector3 DisplayScale { get; set; } = new Vector3(1, 1, 1);

        public virtual bool CanPick { get; set; } = true;

        public virtual void FrameCamera(GL_ControlBase control) { }

        public string Name { get; set; }

        public GenericRenderer(Vector3 position, Vector3 rotationEuler, Vector3 scale) :
            base(position, rotationEuler, scale)
        {
        }

        public override void Prepare(GL_ControlModern control)
        {
            base.Prepare(control);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (!ObjectRenderState.ShouldBeDrawn(this))
                return;

            bool hovered = editorScene.Hovered == this;

            if (!CanPick)
            {
                control.SkipPickingColors(1);
                Selected = false;
                hovered = false;
                if (pass == Pass.PICKING)
                    return;
            }

            Matrix3 rotMtx = GlobalRotation;

            Vector4 highlightColor = Vector4.Zero;
            if (Selected && hovered)
                highlightColor = hoverSelectColor;
            else if (Selected)
                highlightColor = selectColor;
            else if (hovered)
                highlightColor = hoverColor;

            bool positionChanged = false;
            Vector3 position = !Selected ? GlobalPosition :
              editorScene.CurrentAction.NewPos(GlobalPosition, out positionChanged);

            if (positionChanged && editorScene.CurrentAction is TranslateAction)
            {
                var newPosition = OnPositionChanged(position);
                if (newPosition != position)
                    ((TranslateAction)editorScene.CurrentAction).SetAxisXZ();
                position = newPosition;
            }

            control.UpdateModelMatrix(
                  Matrix4.CreateScale((Selected ? editorScene.CurrentAction.NewScale(GlobalScale, rotMtx) : GlobalScale)) *
                      new Matrix4(Selected ? editorScene.CurrentAction.NewRot(rotMtx) : rotMtx) *
                      Matrix4.CreateTranslation(position));

            DrawModel(control, editorScene, pass, highlightColor);
        }

        private Vector3 OnPositionChanged(Vector3 position)
        {
            return position;
        }

        public virtual void DrawModel(GL_ControlModern control, EditorSceneBase editorScene, Pass pass, Vector4 highlightColor)
        {
        }

        public override void GetSelectionBox(ref BoundingBox boundingBox)
        {
            if (Selected)
                boundingBox.Include(Vector3.Transform(Framework.Mat3FromEulerAnglesDeg(Rotation), GlobalPosition));
        }

        public override void ApplyTransformActionToSelection(EditorSceneBase scene, AbstractTransformAction transformAction, ref TransformChangeInfos infos)
        {
            if (!Selected)
                return;

            Vector3 pp = Position, pr = Rotation, ps = Scale;

            GlobalPosition = OnPositionChanged(transformAction.NewPos(GlobalPosition, out bool posHasChanged));

            Matrix3 rotMtx = GlobalRotation;

            GlobalRotation = transformAction.NewRot(GlobalRotation, out bool rotHasChanged);

            GlobalScale = transformAction.NewScale(GlobalScale, rotMtx, out bool scaleHasChanged);

            infos.Add(this, 0,
                posHasChanged ? new Vector3?(pp) : new Vector3?(),
                rotHasChanged ? new Vector3?(pr) : new Vector3?(),
                scaleHasChanged ? new Vector3?(ps) : new Vector3?());
        }
    }
}

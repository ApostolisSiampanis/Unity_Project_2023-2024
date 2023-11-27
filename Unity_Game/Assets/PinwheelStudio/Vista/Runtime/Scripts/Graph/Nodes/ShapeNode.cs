#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Shape",
        path = "Base Shape/Shape",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.7demu3609sra",
        keywords = "shape, square, disc, hemisphere, cone, paraboloid, bell, thorn, pyramid, brick, torus",
        description = "Generate a primitive shape.\n<ss>Tips: Search for the shape directly (eg: square, disc, hemisphere, cone, etc.)</ss>")]
    public class ShapeNode : ImageNodeBase, ISetupWithHint
    {
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        public enum Shape
        {
            Square, Disc, Hemisphere, Cone, Paraboloid, Bell, Thorn, Pyramid, Brick, Torus
        }

        [SerializeField]
        private Shape m_shape;
        public Shape shape
        {
            get
            {
                return m_shape;
            }
            set
            {
                m_shape = value;
            }
        }

        [SerializeField]
        private Vector2 m_scale;
        public Vector2 scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                Vector2 v = value;
                m_scale = new Vector2(Mathf.Max(0.001f, v.x), Mathf.Max(0.001f, v.y));
            }
        }

        [SerializeField]
        private float m_innerSize;
        public float innerSize
        {
            get
            {
                return m_innerSize;
            }
            set
            {
                m_innerSize = Mathf.Clamp01(value);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Shape";
        private static readonly int UV_TO_SHAPE_MATRIX = Shader.PropertyToID("_UvToShapeMatrix");
        private static readonly int INNER_SIZE = Shader.PropertyToID("_InnerSize");

        private Material m_material;

        public ShapeNode() : base()
        {
            m_shape = Shape.Square;
            m_scale = Vector2.one;
            m_innerSize = 0.5f;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            Vector3 t = new Vector3(0.5f, 0.5f);
            Quaternion r = Quaternion.identity;
            Vector3 s = new Vector3(m_scale.x, m_scale.y, 1);
            Matrix4x4 uvToShapeMatrix = Matrix4x4.TRS(t, r, s).inverse;
            m_material.SetMatrix(UV_TO_SHAPE_MATRIX, uvToShapeMatrix);
            m_material.SetFloat(INNER_SIZE, m_innerSize);
            Drawing.DrawQuad(targetRt, m_material, (int)m_shape);
            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public void SetupWithHint(string hint)
        {
            string[] shapeNames = System.Enum.GetNames(typeof(Shape));
            foreach (string n in shapeNames)
            {
                if (n.ToLower().StartsWith(hint))
                {
                    shape = (Shape)System.Enum.Parse(typeof(Shape), n, true);
                    break;
                }
            }
        }
    }
}
#endif

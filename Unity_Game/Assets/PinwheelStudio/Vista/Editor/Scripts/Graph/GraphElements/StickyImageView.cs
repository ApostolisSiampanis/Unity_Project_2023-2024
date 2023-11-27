#if VISTA
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class StickyImageView : GraphElement
    {
        public static readonly Vector2 defaultSize = new Vector2(320, 180);

        protected VisualElement mainContainer { get; private set; }
        protected Image image { get; private set; }
        protected VisualElement selectionBorder { get; private set; }

        public string imageId { get; set; }

        public StickyImageView() : base()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/StickyImage");
            styleSheets.Add(uss);

            base.capabilities = Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Ascendable | Capabilities.Copiable | Capabilities.Groupable | Capabilities.Snappable;
            base.usageHints = UsageHints.DynamicTransform;

            AddToClassList("sticky-image");

            selectionBorder = new VisualElement() { name = "selection-border" };
            Add(selectionBorder);

            mainContainer = new VisualElement() { name = "main-container" };
            Add(mainContainer);

            image = new Image() { name = "image" };
            image.scaleMode = ScaleMode.ScaleAndCrop;
            mainContainer.Add(image);
            image.StretchToParentSize();

            Add(new ResizableElement());

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        public void SetImage(Texture2D tex)
        {
            image.image = tex;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            selectionBorder.AddToClassList("selected");
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            selectionBorder.RemoveFromClassList("selected");
        }

        public override bool IsCopiable()
        {
            return true;
        }

        public void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
           
        }
    }
}
#endif

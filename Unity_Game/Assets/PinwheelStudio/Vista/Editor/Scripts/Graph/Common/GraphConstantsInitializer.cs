#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;

namespace Pinwheel.VistaEditor.Graph
{
    [InitializeOnLoad]
    public static class GraphConstantsInitializer
    {
        [InitializeOnLoadMethod]
        public static void InitInputNameSelector()
        {
            InputNodeEditor.nameSelector.Add(new NameSelectorEntry()
            {
                name = GraphConstants.BIOME_MASK_INPUT_NAME,
                slotType = typeof(MaskSlot)
            });

            InputNodeEditor.nameSelector.Add(new NameSelectorEntry()
            {
                name = GraphConstants.SCENE_HEIGHT_INPUT_NAME,
                slotType = typeof(MaskSlot)
            });
        }

        [InitializeOnLoadMethod]
        public static void InitOutputNameSelector()
        {
            OutputNodeEditor.nameSelector.Add(new NameSelectorEntry()
            {
                name = GraphConstants.BIOME_MASK_OUTPUT_NAME,
                slotType = typeof(MaskSlot)
            });
        }
    }
}
#endif

#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.Vista.Graph
{
    public static class INodeExtensions
    {
        public static ISlot GetFirstTextureBasedSlot(this INode node)
        {
            ISlot[] outputSlots = node.GetOutputSlots();
            ISlot firstTextureBasedSlot = null;
            for (int i = 0; i < outputSlots.Length; ++i)
            {
                System.Type slotType = outputSlots[i].GetAdapter().slotType;
                if (slotType.Equals(typeof(MaskSlot)) || slotType.Equals(typeof(ColorTextureSlot)))
                {
                    firstTextureBasedSlot = outputSlots[i];
                    break;
                }
            }
            return firstTextureBasedSlot;
        }
    }
}
#endif

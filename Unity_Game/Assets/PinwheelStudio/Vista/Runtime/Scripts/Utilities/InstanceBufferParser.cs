#if VISTA
using UnityEngine;
using System.Collections.Generic;
#if GRIFFIN
using Pinwheel.Griffin;
#endif

namespace Pinwheel.Vista
{
    public static class InstanceBufferParser
    {


#if GRIFFIN
        public static void Parse(List<GTreeInstance> instances, ComputeBuffer buffer, int prototypeIndex)
        {
            if (buffer.count % InstanceSample.SIZE != 0)
            {
                Debug.LogError("Cannot parse instance sample buffer");
                return;
            }

            InstanceSample[] data = new InstanceSample[buffer.count / InstanceSample.SIZE];
            buffer.GetData(data);

            foreach (InstanceSample t in data)
            {
                if (t.isValid <= 0)
                    continue;
                GTreeInstance tree = new GTreeInstance();
                tree.Position = t.position;
                tree.Rotation = Quaternion.Euler(0, t.rotationY, 0);
                tree.Scale = new Vector3(t.horizontalScale, t.verticalScale, t.horizontalScale);
                tree.PrototypeIndex = prototypeIndex;
                instances.Add(tree);
            }
        }

        public static void Parse(List<GGrassInstance> instances, ComputeBuffer buffer, int prototypeIndex)
        {
            if (buffer.count % InstanceSample.SIZE != 0)
            {
                Debug.LogError("Cannot parse instance sample buffer");
                return;
            }

            InstanceSample[] data = new InstanceSample[buffer.count / InstanceSample.SIZE];
            buffer.GetData(data);

            foreach (InstanceSample t in data)
            {
                if (t.isValid <= 0)
                    continue;
                GGrassInstance g = new GGrassInstance();
                g.Position = t.position;
                g.Rotation = Quaternion.Euler(0, t.rotationY, 0);
                g.Scale = new Vector3(t.horizontalScale, t.verticalScale, t.horizontalScale);
                g.PrototypeIndex = prototypeIndex;
                instances.Add(g);
            }
        }
#endif
    }
}
#endif

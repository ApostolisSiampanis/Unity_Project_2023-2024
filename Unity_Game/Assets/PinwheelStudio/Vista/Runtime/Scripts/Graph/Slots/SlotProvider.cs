#if VISTA
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Pinwheel.Vista.Graph
{
    public static class SlotProvider
    {
        private static List<Type> slotTypes { get; set; }

        private static void Init()
        {
            slotTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    if (typeof(ISlot).IsAssignableFrom(t))
                    {
                        if (!t.IsClass)
                            continue;
                        if (t.IsGenericType)
                            continue;
                        if (t.IsAbstract)
                            continue;
                        EditorBrowsableAttribute att = t.GetCustomAttribute<EditorBrowsableAttribute>();
                        if (att != null && att.State == EditorBrowsableState.Never)
                            continue;
                        slotTypes.Add(t);
                    }
                }
            }
        }

        public static List<Type> GetAllSlotTypes()
        {
            if (slotTypes == null)
            {
                Init();
            }
            return new List<Type>(slotTypes);
        }

        public static List<Type> GetTextureSlotTypes()
        {
            return new List<Type>()
            {
                typeof(MaskSlot),
                typeof(ColorTextureSlot)
            };
        }

        public static ISlot Create<T>(string name, SlotDirection direction, int id) where T : ISlot, new()
        {
            return Create(typeof(T), name, direction, id);
        }

        public static ISlot Create(Type t, string name, SlotDirection direction, int id)
        {
            ISlot slot = Activator.CreateInstance(t, new object[] { name, direction, id }) as ISlot;
            return slot;
        }
    }
}
#endif

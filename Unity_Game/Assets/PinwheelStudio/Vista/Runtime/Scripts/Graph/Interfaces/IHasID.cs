#if VISTA
using System.Reflection;

namespace Pinwheel.Vista.Graph
{
    public interface IHasID
    {
        string id { get; }
    }

    public static class IHasIdExtension
    {
        public static void SetId(object target, string newId)
        {
            FieldInfo idField = target.GetType().GetField("m_id", BindingFlags.Instance | BindingFlags.NonPublic);
            if (idField == null)
                throw new System.Exception("Element should declare a field [m_id] for copy/paste to work");
            idField.SetValue(target, newId);
        }

        public static string GenerateId()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}
#endif

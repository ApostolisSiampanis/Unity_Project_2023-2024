#if VISTA
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Pinwheel.Vista.ExposeProperty;

namespace Pinwheel.Vista.Graph
{
    public abstract partial class GraphAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        public delegate void ChangeHandler(GraphAsset sender);
        public static event ChangeHandler graphChanged;

        [System.NonSerialized]
        internal protected List<INode> m_nodes = new List<INode>();
        [SerializeField]
        protected List<Serializer.JsonObject> m_nodeData;

        [System.NonSerialized]
        internal protected List<IEdge> m_edges = new List<IEdge>();
        [SerializeField]
        protected List<Serializer.JsonObject> m_edgeData;

        [System.NonSerialized]
        internal protected List<IGroup> m_groups = new List<IGroup>();
        [SerializeField]
        protected List<Serializer.JsonObject> m_groupData;

        [System.NonSerialized]
        internal protected List<IStickyNote> m_stickyNotes = new List<IStickyNote>();
        [SerializeField]
        protected List<Serializer.JsonObject> m_stickyNoteData;

        [System.NonSerialized]
        internal protected List<IStickyImage> m_stickyImages = new List<IStickyImage>();
        [SerializeField]
        protected List<Serializer.JsonObject> m_stickyImageData;

        [SerializeField]
        protected List<ObjectReference> m_objectReferences;

        [SerializeField]
        [HideInInspector]
        internal List<PropertyDescriptor> m_exposedProperties;

        public bool HasExposedProperties
        {
            get
            {
                return m_exposedProperties != null && m_exposedProperties.Count > 0;
            }
        }

        public virtual void Reset()
        {
            m_nodes = new List<INode>();
            m_edges = new List<IEdge>();
            m_groups = new List<IGroup>();
            m_stickyNotes = new List<IStickyNote>();
            m_stickyImages = new List<IStickyImage>();
            m_nodeData = new List<Serializer.JsonObject>();
            m_edgeData = new List<Serializer.JsonObject>();
            m_groupData = new List<Serializer.JsonObject>();
            m_stickyNoteData = new List<Serializer.JsonObject>();
            m_stickyImageData = new List<Serializer.JsonObject>();
            m_objectReferences = new List<ObjectReference>();
            m_exposedProperties = new List<PropertyDescriptor>();
        }

        protected void Awake()
        {
            //Reset();
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        public virtual void OnBeforeSerialize()
        {
            using (Serializer.TargetScope s = new Serializer.TargetScope(this))
            {
                m_objectReferences = new List<ObjectReference>();
                if (m_nodes != null)
                {
                    foreach (INode node in m_nodes)
                    {
                        SerializeAssetReferences(m_objectReferences, node);

                        if (node is ISerializationCallbackReceiver serializeReceiver)
                        {
                            serializeReceiver.OnBeforeSerialize();
                        }
                    }
                    m_nodeData = Serializer.Serialize<INode>(m_nodes);
                }
                else
                {
                    m_nodeData = new List<Serializer.JsonObject>();
                }

                if (m_edges != null)
                {
                    m_edgeData = Serializer.Serialize<IEdge>(m_edges);
                }
                else
                {
                    m_edgeData = new List<Serializer.JsonObject>();
                }

                if (m_groups != null)
                {
                    m_groupData = Serializer.Serialize<IGroup>(m_groups);
                }
                else
                {
                    m_groupData = new List<Serializer.JsonObject>();
                }

                if (m_stickyNotes != null)
                {
                    m_stickyNoteData = Serializer.Serialize<IStickyNote>(m_stickyNotes);
                }
                else
                {
                    m_stickyNoteData = new List<Serializer.JsonObject>();
                }

                if (m_stickyImages != null)
                {
                    m_stickyImageData = Serializer.Serialize<IStickyImage>(m_stickyImages);
                }
                else
                {
                    m_stickyImageData = new List<Serializer.JsonObject>();
                }
            }
        }

        public virtual void OnAfterDeserialize()
        {
            using (Serializer.TargetScope s = new Serializer.TargetScope(this)) 
            {
                if (m_nodeData != null)
                {
                    m_nodes = Serializer.Deserialize<INode>(m_nodeData);
                }
                else
                {
                    m_nodes = new List<INode>();
                }

                foreach (INode node in m_nodes)
                {
                    DeserializeAssetReferences(m_objectReferences, node);
                    if (node is ISerializationCallbackReceiver serializeReceiver)
                    {
                        serializeReceiver.OnAfterDeserialize();
                    }
                }

                if (m_edgeData != null)
                {
                    m_edges = Serializer.Deserialize<IEdge>(m_edgeData);
                }
                else
                {
                    m_edges = new List<IEdge>();
                }

                if (m_groupData != null)
                {
                    m_groups = Serializer.Deserialize<IGroup>(m_groupData);
                }
                else
                {
                    m_groups = new List<IGroup>();
                }

                if (m_stickyNoteData != null)
                {
                    m_stickyNotes = Serializer.Deserialize<IStickyNote>(m_stickyNoteData);
                }
                else
                {
                    m_stickyNotes = new List<IStickyNote>();
                }

                if (m_stickyImageData != null)
                {
                    m_stickyImages = Serializer.Deserialize<IStickyImage>(m_stickyImageData);
                }
                else
                {
                    m_stickyImages = new List<IStickyImage>();
                }

                //Validate();
            }
        }

        internal static void SerializeAssetReferences(List<ObjectReference> referencesList, INode n)
        {
            IEnumerable<FieldInfo> serializedObjectFields = GetSerializedAssetFields(n.GetType());
            foreach (FieldInfo f in serializedObjectFields)
            {
                UnityEngine.Object target = f.GetValue(n) as UnityEngine.Object;
                if (target != null)
                {
                    string key = GetObjectSerializeKey(n.id, f.Name);
                    ObjectReference oRef = new ObjectReference(key, target);
                    referencesList.Add(oRef);
                }
            }
        }

        internal static void DeserializeAssetReferences(List<ObjectReference> referencesList, INode n)
        {
            IEnumerable<FieldInfo> serializedObjectFields = GetSerializedAssetFields(n.GetType());
            foreach (FieldInfo f in serializedObjectFields)
            {
                ObjectReference oRef = referencesList.Find(r =>
                {
                    return string.Equals(r.key, GetObjectSerializeKey(n.id, f.Name));
                });
                if (oRef.Equals(default))
                    continue;
                try
                {
                    f.SetValue(n, oRef.target);
                }
                catch (Exception)
                {
                    Debug.Log($"Failed to deserialize {n.GetType().Name}.{f.Name}. This happens when an asset referenced by the graph has been removed from the project.");
                }
            }
        }

        public static IEnumerable<FieldInfo> GetSerializedAssetFields(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> serializedObjectFields = new List<FieldInfo>();
            foreach (FieldInfo f in fields)
            {
                SerializeAssetAttribute serializeAttribute = f.GetCustomAttribute<SerializeAssetAttribute>();
                bool serializable = serializeAttribute != null;
                bool isObject = f.FieldType.IsSubclassOf(typeof(UnityEngine.Object));

                if (serializable && isObject)
                {
                    serializedObjectFields.Add(f);
                }
            }
            return serializedObjectFields;
        }

        private static string GetObjectSerializeKey(string elementGuid, string fieldName)
        {
            return $"{elementGuid}.{fieldName}";
        }

        public virtual bool Validate()
        {
            int count = 0;
            count += m_nodes.RemoveAll(n => n == null);
            count += m_edges.RemoveAll(e => e == null);
            count += m_edges.RemoveAll(e =>
            {
                SlotRef outputSlot = e.outputSlot;
                INode n0 = GetNode(outputSlot.nodeId);
                if (n0 == null)
                    return true; //point to non-exist node
                ISlot slot0 = n0.GetSlot(outputSlot.slotId);
                if (slot0 == null)
                    return true; //point to non-exist slot

                SlotRef inputSlot = e.inputSlot;
                INode n1 = GetNode(inputSlot.nodeId);
                if (n1 == null)
                    return true; //point to non-exist node
                ISlot slot1 = n1.GetSlot(inputSlot.slotId);
                if (slot1 == null)
                    return true; //point to non-exist slot

                if (outputSlot.nodeId == inputSlot.nodeId)
                    return true; //self connect, point to the same node

                if (slot0.direction == slot1.direction)
                    return true; //input to input; output to output

                ISlotAdapter a0 = slot0.GetAdapter();
                ISlotAdapter a1 = slot1.GetAdapter();
                if (!a0.CanConnectTo(a1))
                    return true; //incompatible slot types

                if (!a1.CanConnectTo(a0))
                    return true; //incompatible slot types

                return false;
            });
            count += m_groups.RemoveAll(g => g == null);
            count += m_stickyNotes.RemoveAll(n => n == null);

            //remove exposed properties of non-existing nodes
            if (HasExposedProperties)
            {
                count += m_exposedProperties.RemoveAll(p =>
                {
                    INode targetNode = GetNode(p.id.nodeId);
                    return targetNode == null;
                });
            }

            return count != 0;
        }

        public bool HasID(string id)
        {
            foreach (INode n in m_nodes)
            {
                if (n.id.Equals(id))
                {
                    return true;
                }
            }
            foreach (IEdge e in m_edges)
            {
                if (e.id.Equals(id))
                {
                    return true;
                }
            }
            foreach (IGroup g in m_groups)
            {
                if (g.id.Equals(id))
                {
                    return true;
                }
            }
            foreach (IStickyNote n in m_stickyNotes)
            {
                if (n.id.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void AddNode(INode n)
        {
            Type type = n.GetType();
            if (!AcceptNodeType(type))
            {
                throw new System.ArgumentException($"Node of type {type.Name} is not accepted");
            }
            if (HasID(n.id))
            {
                throw new System.ArgumentException($"Graph element with the id {n.id} is already exist.");
            }
            m_nodes.Add(n);
        }

        public virtual void AddEdge(IEdge e)
        {
            if (WillCreateRecursive(e))
            {
                throw new System.ArgumentException("New edge suppose to create a recursive connection.");
            }
            if (HasID(e.id))
            {
                throw new System.ArgumentException($"Graph element with the id {e.id} is already exist.");
            }
            m_edges.Add(e);
        }

        public virtual void AddGroup(IGroup g)
        {
            if (HasID(g.id))
            {
                throw new System.ArgumentException($"Graph element with the id {g.id} is already exist.");
            }
            m_groups.Add(g);
        }

        public virtual RemovedElements RemoveNode(string id)
        {
            RemovedElements result = new RemovedElements();
            INode node = GetNode(id);
            if (node != null)
            {
                m_nodes.Remove(node);
                result.node = node;

                List<IEdge> edgeToRemove = new List<IEdge>();

                List<ISlot> slots = new List<ISlot>();
                slots.AddRange(node.GetInputSlots());
                slots.AddRange(node.GetOutputSlots());
                foreach (ISlot slot in slots)
                {
                    List<IEdge> connectedEdges = GetEdges(node.id, slot.id);
                    foreach (IEdge edge in connectedEdges)
                    {
                        m_edges.Remove(edge);
                        edgeToRemove.Add(edge);
                    }
                }

                if (edgeToRemove.Count > 0)
                {
                    result.edges = edgeToRemove;
                }
            }

            return result;
        }

        public virtual RemovedElements RemoveEdge(string id)
        {
            RemovedElements result = new RemovedElements();
            IEdge edge = GetEdge(id);
            if (edge != null)
            {
                m_edges.Remove(edge);
                result.edges = new List<IEdge>() { edge };
            }
            return result;
        }

        public bool HasNode(string id)
        {
            return m_nodes.Exists(n => n.id.Equals(id));
        }

        public INode GetNode(string id)
        {
            return m_nodes.Find(n => n.id.Equals(id));
        }

        public List<INode> GetNodes()
        {
            return new List<INode>(m_nodes);
        }

        public IEdge GetEdge(string id)
        {
            return m_edges.Find(e => e.id.Equals(id));
        }

        public List<IEdge> GetEdges()
        {
            return new List<IEdge>(m_edges);
        }

        protected List<IEdge> GetEdges(string nodeId, int slotId)
        {
            List<IEdge> result = new List<IEdge>();
            for (int i = 0; i < m_edges.Count; ++i)
            {
                IEdge e = m_edges[i];
                if (e.inputSlot.nodeId.Equals(nodeId) && e.inputSlot.slotId.Equals(slotId))
                {
                    result.Add(e);
                }
                else if (e.outputSlot.nodeId.Equals(nodeId) && e.outputSlot.slotId.Equals(slotId))
                {
                    result.Add(e);
                }
            }
            return result;
        }

        public bool WillCreateRecursive(IEdge edge)
        {
            string startNodeId = edge.outputSlot.nodeId;
            Stack<IEdge> edgeStack = new Stack<IEdge>();
            edgeStack.Push(edge);

            while (edgeStack.Count > 0)
            {
                IEdge e = edgeStack.Pop();
                if (e.inputSlot.nodeId.Equals(startNodeId))
                {
                    return true;
                }
                else
                {
                    string currentNodeId = e.inputSlot.nodeId;
                    List<IEdge> nextConnections = m_edges.FindAll(e0 => e0.outputSlot.nodeId.Equals(currentNodeId));
                    for (int i = 0; i < nextConnections.Count; ++i)
                    {
                        edgeStack.Push(nextConnections[i]);
                    }
                }
            }

            return false;
        }

        public List<IGroup> GetGroups()
        {
            return new List<IGroup>(m_groups);
        }

        public IGroup GetGroup(string id)
        {
            return m_groups.Find(g => g.id.Equals(id));
        }

        public IGroup RemoveGroup(string id)
        {
            IGroup g = GetGroup(id);
            m_groups.Remove(g);
            return g;
        }

        public virtual void AddStickyNote(IStickyNote n)
        {
            if (HasID(n.id))
            {
                throw new System.ArgumentException($"Graph element with the id {n.id} is already exist.");
            }
            m_stickyNotes.Add(n);
        }

        public IStickyNote GetStickyNote(string id)
        {
            return m_stickyNotes.Find(n => n.id.Equals(id));
        }

        public List<IStickyNote> GetStickyNotes()
        {
            return new List<IStickyNote>(m_stickyNotes);
        }

        public IStickyNote RemoveStickyNote(string id)
        {
            IStickyNote note = GetStickyNote(id);
            m_stickyNotes.Remove(note);
            return note;
        }

        public virtual void AddStickyImage(IStickyImage i)
        {
            if (HasID(i.id))
            {
                throw new System.ArgumentException($"Graph element with the id {i.id} is already exist.");
            }
            m_stickyImages.Add(i);
        }

        public IStickyImage GetStickyImage(string id)
        {
            return m_stickyImages.Find(n => n.id.Equals(id));
        }

        public List<IStickyImage> GetStickyImages()
        {
            return new List<IStickyImage>(m_stickyImages);
        }

        public IStickyImage RemoveStickyImage(string id)
        {
            IStickyImage img = GetStickyImage(id);
            m_stickyImages.Remove(img);
            return img;
        }

        public List<INode> GetNodes<T>()
        {
            return GetNodes(typeof(T));
        }

        public List<INode> GetNodes(Type t)
        {
            List<INode> result = m_nodes.FindAll(n => t.IsAssignableFrom(n.GetType()));
            return result;
        }

        public List<T> GetNodesOfType<T>() where T : class, INode
        {
            List<T> result = m_nodes.FindAll(n => typeof(T).IsAssignableFrom(n.GetType())).ConvertAll(n => n as T);
            return result;
        }

        public INode GetNode<T>()
        {
            return GetNode(typeof(T));
        }

        public INode GetNode(Type t)
        {
            return m_nodes.Find(n => t.IsAssignableFrom(n.GetType()));
        }

        public void InvokeChangeEvent()
        {
            if (graphChanged != null)
            {
                graphChanged.Invoke(this);
            }
        }

        public virtual bool AcceptNodeType(Type t)
        {
            return true;
        }

        public virtual IEnumerable<GraphAsset> GetDependencySubGraphs()
        {
            return new GraphAsset[0];
        }
    }
}
#endif

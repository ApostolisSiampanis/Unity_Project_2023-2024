#if VISTA
using UnityEditor;
using Pinwheel.Vista.Graph;
using System.Collections.Generic;

namespace Pinwheel.VistaEditor.Graph
{
    public class TerrainGraphCommandHandler : GraphCommandHandlerBase<TerrainGraph>
    {
        protected override void CopyGraphData(TerrainGraph from, TerrainGraph to)
        {
            string toName = to.name;
            string json = EditorJsonUtility.ToJson(from);
            EditorJsonUtility.FromJsonOverwrite(json, to);
            to.name = toName;

            //List<INode> nodes = from.GetNodes();
            //List<IEdge> edges = from.GetEdges();
            //List<IGroup> groups = from.GetGroups();
            //List<IStickyNote> notes = from.GetStickyNotes();
            //List<IStickyImage> images = from.GetStickyImages();

            //to.Reset();
            //foreach (INode n in nodes)
            //{
            //    to.AddNode(n);
            //}
            //foreach (IEdge e in edges)
            //{
            //    to.AddEdge(e);
            //}
            //foreach (IGroup g in groups)
            //{
            //    to.AddGroup(g);
            //}
            //foreach (IStickyNote n in notes)
            //{
            //    to.AddStickyNote(n);
            //}
            //foreach (IStickyImage i in images)
            //{
            //    to.AddStickyImage(i);
            //}
            //to.debugConfigs = from.debugConfigs;
            //to.allowSplitExecution = from.allowSplitExecution;
        }
    }
}
#endif

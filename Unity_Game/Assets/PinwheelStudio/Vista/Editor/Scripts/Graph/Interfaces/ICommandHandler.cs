#if VISTA

namespace Pinwheel.VistaEditor.Graph
{
    public interface ICommandHandler
    {
        GraphEditorBase editor { get; set; }
        void Save();
        void SaveAs();
    }
}
#endif

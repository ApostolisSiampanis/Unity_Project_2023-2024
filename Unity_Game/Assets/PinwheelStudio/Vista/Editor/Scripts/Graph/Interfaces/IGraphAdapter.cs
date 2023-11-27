#if VISTA

namespace Pinwheel.VistaEditor.Graph
{
    public interface IGraphAdapter
    {
        ISearcherProvider searcherProvider { get; }
        ICommandHandler commandHandler { get; }
        IGraphPropertiesDrawer propertiesDrawer { get; }
    }
}
#endif

#if VISTA
using UnityEditor.Searcher;

namespace Pinwheel.VistaEditor.Graph
{
    public interface ISearcherProvider
    {
        string currentSearchText { get; set; }
        Searcher GetSearcher(SearcherFilter filter);
        SearcherItemData GetDataFromSelectedItem(SearcherItem item);
    }
}
#endif

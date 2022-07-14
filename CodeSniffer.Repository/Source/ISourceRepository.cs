using System.Text.Json.Nodes;

namespace CodeSniffer.Repository.Source
{
    public interface ISourceRepository
    {
        ValueTask<IReadOnlyList<ListSource>> ListSources();
        ValueTask<IReadOnlyList<ListSourceGroup>> ListSourceGroups();

        ValueTask<IReadOnlyList<CsStoredSource>> GetAllSources();
        ValueTask<IReadOnlyList<CsStoredSourceGroup>> GetAllSourceGroups();

        ValueTask<CsStoredSource> GetSourceDetails(string id);
        ValueTask<CsStoredSourceGroup> GetSourceGroupDetails(string id);

        ValueTask<string> InsertSource(CsSource newSource, string author);
        ValueTask UpdateSource(string id, CsSource newSource, string author);
        ValueTask RemoveSource(string id, string author);

        ValueTask<string> InsertSourceGroup(CsSourceGroup newSourceGroup, string author);
        ValueTask UpdateSourceGroup(string id, CsSourceGroup newSourceGroup, string author);
        ValueTask RemoveSourceGroup(string id, string author);
    }


    public class ListSource
    {
        public string Id { get; }
        public string Name { get; }


        public ListSource(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }


    public class ListSourceGroup
    {
        public string Id { get; }
        public string Name { get; }


        public ListSourceGroup(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }


    public class CsSource
    {
        public string Name { get; }
        public Guid PluginId { get; }
        public JsonObject Configuration { get; }


        public CsSource(string name, Guid pluginId, JsonObject configuration)
        {
            Name = name;
            PluginId = pluginId;
            Configuration = configuration;
        }
    }


    public class CsStoredSource : CsSource
    {
        public string Id { get; }
        public int Version { get; }
        public string Author { get; }


        public CsStoredSource(string id, string name, int version, string author, Guid pluginId, JsonObject configuration)
            : base(name, pluginId, configuration)
        {
            Id = id;
            Version = version;
            Author = author;
        }
    }


    public class CsSourceGroup
    {
        public string Name { get; }
        public IReadOnlyList<string> SourceIds { get; }


        public CsSourceGroup(string name, IReadOnlyList<string> sourceIds)
        {
            Name = name;
            SourceIds = sourceIds;
        }
    }


    public class CsStoredSourceGroup : CsSourceGroup
    {
        public string Id { get; }
        public int Version { get; }
        public string Author { get; }


        public CsStoredSourceGroup(string id, string name, int version, string author, IReadOnlyList<string> sourceIds)
            : base(name, sourceIds)
        {
            Id = id;
            Version = version;
            Author = author;
        }
    }
}

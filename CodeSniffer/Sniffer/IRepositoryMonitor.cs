using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Source;

namespace CodeSniffer.Sniffer
{
    public interface IRepositoryMonitor : IAsyncDisposable
    {
        ValueTask Initialize(IEnumerable<CsStoredSource> sources, IEnumerable<CsStoredSourceGroup> sourceGroups, IEnumerable<CsStoredDefinition> definitions);

        void DefinitionChanged(CsStoredDefinition newDefinition);
        void DefinitionRemoved(string id);

        ValueTask SourceChanged(string id, CsSource newSource);
        void SourceRemoved(string id);

        void SourceGroupChanged(string id, CsSourceGroup newSourceGroup);
        void SourceGroupRemoved(string id);
    }
}

using System;
using System.Collections.Generic;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Source;

namespace CodeSniffer.Sniffer
{
    public interface IRepositoryMonitor : IAsyncDisposable
    {
        void Initialize(IEnumerable<CsStoredSource> sources, IEnumerable<CsStoredSourceGroup> sourceGroups, IEnumerable<CsStoredDefinition> definitions);

        void DefinitionChanged(string id, CsDefinition newDefinition);
        void DefinitionRemoved(string id);

        void SourceChanged(string id, CsSource newSource);
        void SourceRemoved(string id);

        void SourceGroupChanged(string id, CsSourceGroup newSourceGroup);
        void SourceGroupRemoved(string id);
    }
}

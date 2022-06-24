using System;
using System.Collections.Generic;
using CodeSniffer.Repository.Checks;

namespace CodeSniffer.Sniffer
{
    public interface IRepositoryMonitor : IAsyncDisposable
    {
        void Initialize(IEnumerable<CsStoredDefinition> definitions);
        void DefinitionChanged(string id, CsDefinition newDefinition);
        void DefinitionRemoved(string id);
    }
}

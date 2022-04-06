using LiteDB;

namespace CodeSniffer.Repository.LiteDB
{
    public interface ILiteDbPooledConnection : IDisposable
    {
        /// <remarks>
        /// Do NOT dispose this instance! Dispose the ILiteDbPooledConnection instead.
        /// </remarks>
        ILiteDatabase Database { get; }
    }
}

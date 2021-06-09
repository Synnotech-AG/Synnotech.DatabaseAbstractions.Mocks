namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implements <see cref="IAsyncReadOnlySession" />.
    /// </summary>
    /// <typeparam name="T">Your subtype that derives from this class.</typeparam>
    public abstract class AsyncReadOnlySessionMock<T> : AsyncDisposableMock<T>, IAsyncReadOnlySession
        where T : AsyncReadOnlySessionMock<T> { }
}
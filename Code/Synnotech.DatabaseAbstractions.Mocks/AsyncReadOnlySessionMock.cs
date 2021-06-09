using System.Threading.Tasks;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implements <see cref="IAsyncReadOnlySession" />.
    /// </summary>
    /// <typeparam name="T">The subtype that derives from this class.</typeparam>
    public abstract class AsyncReadOnlySessionMock<T> : IAsyncReadOnlySession
        where T : AsyncReadOnlySessionMock<T>
    {
        /// <summary>
        /// Gets the number of times <see cref="Dispose" /> or <see cref="DisposeAsync" /> was called.
        /// </summary>
        public int DisposeCallCount { get; protected set; }

        /// <summary>
        /// Increments the <see cref="DisposeCallCount" />.
        /// </summary>
        public void Dispose() => DisposeCallCount++;

        /// <summary>
        /// Increments the <see cref="DisposeCallCount" />.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            DisposeCallCount++;
            return default;
        }

        /// <summary>
        /// Checks if this session was disposed (<see cref="DisposeCallCount" /> must be greater or equal to 1),
        /// or otherwise throws a <see cref="TestException" />.
        /// </summary>
        public T MustBeDisposed()
        {
            if (DisposeCallCount < 1)
                throw new TestException($"\"{GetType().Name}\" was not disposed");
            return (T) this;
        }
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implements <see cref="IAsyncSession" />.
    /// </summary>
    /// <typeparam name="T">The subtype that derives from this class.</typeparam>
    public abstract class AsyncSessionMock<T> : AsyncReadOnlySessionMock<T>, IAsyncSession
        where T : AsyncSessionMock<T>
    {
        /// <summary>
        /// Gets the number of times <see cref="SaveChangesAsync" /> was called.
        /// </summary>
        public int SaveChangesCallCount { get; protected set; }

        /// <summary>
        /// Increments the <see cref="SaveChangesCallCount" />.
        /// </summary>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if <see cref="SaveChangesAsync" /> has been called exactly once, or otherwise
        /// throws a <see cref="TestException" />.
        /// </summary>
        public T SaveChangesMustHaveBeenCalled()
        {
            if (SaveChangesCallCount != 1)
                throw new TestException($"SaveChangesAsync must have been called exactly once, but it was called {SaveChangesCallCount} times.");
            return (T) this;
        }
    }
}
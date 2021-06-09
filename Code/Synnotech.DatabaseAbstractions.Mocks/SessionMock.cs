namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implements <see cref="ISession" />.
    /// </summary>
    /// <typeparam name="T">The subtype that derives from this class.</typeparam>
    public abstract class SessionMock<T> : ReadOnlySessionMock<T>, ISession
        where T : SessionMock<T>
    {
        /// <summary>
        /// Gets the number of times <see cref="SaveChanges" /> was called.
        /// </summary>
        public int SaveChangesCallCount { get; protected set; }

        /// <summary>
        /// Increments the <see cref="SaveChangesCallCount" />.
        /// </summary>
        public void SaveChanges()
        {
            checked { SaveChangesCallCount++; }
        }

        /// <summary>
        /// Checks if <see cref="SaveChanges" /> has been called exactly once, or otherwise
        /// throws a <see cref="TestException" />.
        /// </summary>
        public T SaveChangesMustHaveBeenCalled()
        {
            if (SaveChangesCallCount != 1)
                throw new TestException($"SaveChanges must have been called exactly once, but it was called {SaveChangesCallCount} times.");
            return (T) this;
        }
    }
}
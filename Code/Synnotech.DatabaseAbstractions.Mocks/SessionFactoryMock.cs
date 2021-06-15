using System;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a mock that implements <see cref="ISessionFactory{TSessionAbstraction}" />.
    /// </summary>
    /// <typeparam name="T">The abstraction that represents your database session.</typeparam>
    public sealed class SessionFactoryMock<T> : ISessionFactory<T>
        where T : IAsyncSession
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SessionFactoryMock{T}" />.
        /// </summary>
        /// <param name="session">The session that will be returned when <see cref="OpenSessionAsync" /> is called.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        public SessionFactoryMock(T session) =>
            Session = session.MustNotBeNullReference(nameof(session));

        /// <summary>
        /// Gets the session that will be returned when <see name="OpenSessionAsync" /> is called.
        /// </summary>
        public T Session { get; }

        /// <summary>
        /// Gets the number of calls to <see cref="OpenSessionAsync" />.
        /// </summary>
        public int OpenSessionCallCount { get; private set; }

        /// <summary>
        /// Returns the <see cref="Session" /> to the client and increments the <see cref="OpenSessionCallCount" />.
        /// </summary>
        public Task<T> OpenSessionAsync(CancellationToken cancellationToken = default)
        {
            checked { OpenSessionCallCount++; }

            return Task.FromResult(Session);
        }

        /// <summary>
        /// Checks if <see cref="OpenSessionAsync" /> has never been called, or otherwise throws a <see cref="TestException" />.
        /// </summary>
        public SessionFactoryMock<T> OpenSessionMustNotHaveBeenCalled()
        {
            if (OpenSessionCallCount != 0)
                throw new TestException($"OpenSessionAsync must not have been called, but it was actually called {OpenSessionCallCount} {(OpenSessionCallCount == 1 ? "time" : "times")}.");
            return this;
        }

        /// <summary>
        /// Checks if <see cref="OpenSessionAsync" /> was called exactly once, or otherwise throws a <see cref="TestException" />.
        /// </summary>
        public SessionFactoryMock<T> OpenSessionMustHaveBeenCalled()
        {
            if (OpenSessionCallCount == 0)
                throw new TestException("OpenSessionAsync must have been called exactly once, but it was actually never called.");
            if (OpenSessionCallCount > 1)
                throw new TestException($"OpenSessionAsync must have been called exactly once, but it was actually called {OpenSessionCallCount} times.");
            return this;
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a mock that implements <see cref="ISessionFactory{TSessionAbstraction}" />.
    /// </summary>
    /// <typeparam name="T">The abstraction that represents your database session.</typeparam>
    public sealed class SessionFactoryMock<T> : BaseSessionFactoryMock<T, SessionFactoryMock<T>>, ISessionFactory<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SessionFactoryMock{T}" />.
        /// </summary>
        /// <param name="session">The session that will be returned when <see cref="OpenSessionAsync" /> is called.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        public SessionFactoryMock(T session) : base(session) { }

        /// <summary>
        /// Returns the session to the client and increments the OpenSessionCallCount.
        /// </summary>
        public ValueTask<T> OpenSessionAsync(CancellationToken cancellationToken = default)
        {
            IncrementOpenSessionCallCount();
            return new ValueTask<T>(Session);
        }
    }
}
using System;

namespace Synnotech.DatabaseAbstractions.Mocks;

/// <summary>
/// Represents a mock that can be used to track calls to delegate that creates session instances (Func&lt;ISession>).
/// </summary>
/// <typeparam name="T">The type of your session. Usually, the abstract type is used.</typeparam>
public sealed class DelegateSessionFactoryMock<T> : BaseSessionFactoryMock<T, DelegateSessionFactoryMock<T>>
{
    /// <summary>
    /// Initializes a new instance of <see cref="DelegateSessionFactoryMock{T}" />.
    /// </summary>
    /// <param name="session">The session instance that will be returned when a new session is requested.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
    public DelegateSessionFactoryMock(T session) : base(session) =>
        CreateSession = CreateSessionInternal;

    /// <summary>
    /// Gets the delegate that can be used to create new session instances.
    /// </summary>
    public Func<T> CreateSession { get; }

    private T CreateSessionInternal()
    {
        IncrementOpenSessionCallCount();
        return Session;
    }
}
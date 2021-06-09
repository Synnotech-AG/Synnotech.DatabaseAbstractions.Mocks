using System;

namespace Synnotech.DatabaseAbstractions.Mocks
{
    /// <summary>
    /// Represents a base class for mocks that implement <see cref="IDisposable"/>
    /// </summary>
    /// <typeparam name="T">The subtype that derives from this class.</typeparam>
    public abstract class ReadOnlySessionMock<T> : DisposableMock<T>
        where T : ReadOnlySessionMock<T> { }
}
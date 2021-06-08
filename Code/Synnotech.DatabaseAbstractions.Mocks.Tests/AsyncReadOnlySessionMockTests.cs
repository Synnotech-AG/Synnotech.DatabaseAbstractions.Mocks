using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class AsyncReadOnlySessionMockTests
    {
        [Fact]
        public static void MustBeAbstractClass() =>
            typeof(AsyncReadOnlySessionMock<>).Should().BeAbstract();

        [Fact]
        public static void ThrowExceptionWhenNotDisposed()
        {
            var session = new SessionMock();

            Action act = () => session.MustBeDisposed();

            act.Should().Throw<TestException>()
               .And.Message.Should().Be("\"SessionMock\" was not disposed");
        }

        [Fact]
        public static void NoExceptionWhenDisposed()
        {
            var session = new SessionMock();

            session.Dispose();

            session.EnsureNoExceptionIsThrown();
        }

        [Fact]
        public static async Task NoExceptionWhenDisposedAsync()
        {
            var session = new SessionMock();

            await session.DisposeAsync();

            session.EnsureNoExceptionIsThrown();
        }

        private static void EnsureNoExceptionIsThrown(this SessionMock session) =>
            session.MustBeDisposed().Should().BeSameAs(session);

        private sealed class SessionMock : AsyncReadOnlySessionMock<SessionMock> { }
    }
}
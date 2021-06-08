using System;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class ReadOnlySessionMockTests
    {
        [Fact]
        public static void MustBeAbstractClass() =>
            typeof(ReadOnlySessionMock<>).Should().BeAbstract();

        [Fact]
        public static void ThrowExceptionWHenNotDisposed()
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

            session.MustBeDisposed().Should().BeSameAs(session);
        }

        private sealed class SessionMock : ReadOnlySessionMock<SessionMock> { }
    }
}
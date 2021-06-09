using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class AsyncSessionMockTests
    {
        [Fact]
        public static void MustBeAbstract() =>
            typeof(AsyncSessionMock<>).Should().BeAbstract();

        [Fact]
        public static void MustDeriveFromAsyncReadOnlySessionMock() =>
            typeof(AsyncSessionMock<>).Should().BeDerivedFrom(typeof(AsyncReadOnlySessionMock<>));

        [Fact]
        public static void ExceptionWhenSaveChangesWasNotCalled()
        {
            var session = new SessionMock();

            session.CheckException(0);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(23)]
        public static async Task ExceptionWhenSaveChangesAsyncWasCalledTooOften(int numberOfCalls)
        {
            var session = new SessionMock();

            for (var i = 0; i < numberOfCalls; i++)
            {
                await session.SaveChangesAsync();
            }

            session.CheckException(numberOfCalls);
        }

        private static void CheckException(this SessionMock session, int numberOfCalls)
        {
            Action act = () => session.SaveChangesMustHaveBeenCalled();

            act.Should().Throw<TestException>()
               .And.Message.Should().Be($"SaveChangesAsync must have been called exactly once, but it was called {numberOfCalls} times.");
        }

        [Fact]
        public static async Task NoExceptionWhenSaveChangesWasCalledExactlyOnce()
        {
            var session = new SessionMock();

            await session.SaveChangesAsync();

            session.SaveChangesMustHaveBeenCalled().Should().BeSameAs(session);
        }

        private sealed class SessionMock : AsyncSessionMock<SessionMock> { }
    }
}
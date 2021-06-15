using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests
{
    public static class SessionFactoryMockTests
    {
        [Fact]
        public static void MustImplementISessionFactory() =>
            typeof(SessionFactoryMock<Session>).Should().Implement(typeof(ISessionFactory<Session>));

        [Fact]
        public static void SessionMustBeRetrievable()
        {
            var session = new Session();
            var sessionFactory = CreateSessionFactory(session);

            sessionFactory.Session.Should().BeSameAs(session);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(9)]
        [InlineData(21)]
        public static async Task SessionMustBeReturnedOnOpenAsync(int numberOfCalls)
        {
            var session = new Session();
            var sessionFactory = CreateSessionFactory(session);

            for (var i = 0; i < numberOfCalls; i++)
            {
                var createdSession = await sessionFactory.OpenSessionAsync();
                createdSession.Should().BeSameAs(session);
            }
        }

        public static class WhenOpenSessionMustNotHaveBeenCalled
        {
            [Fact]
            public static void NoExceptionWhenSessionWasNotCreated()
            {
                var sessionFactory = CreateSessionFactory();

                sessionFactory.OpenSessionMustNotHaveBeenCalled().Should().BeSameAs(sessionFactory);
            }

            [Fact]
            public static async Task ExceptionWhenOpenSessionWasCalledOnce()
            {
                var sessionFactory = CreateSessionFactory();
                await sessionFactory.OpenSessionAsync();

                Action act = () => sessionFactory.OpenSessionMustNotHaveBeenCalled();

                act.Should().Throw<TestException>()
                   .And.Message.Should().Be("OpenSessionAsync must not have been called, but it was actually called 1 time.");
            }

            [Theory]
            [InlineData(2)]
            [InlineData(3)]
            [InlineData(11)]
            public static async Task ExceptionWhenOpenSessionWasCalledMultipleTimes(int numberOfCalls)
            {
                var sessionFactory = CreateSessionFactory();
                for (var i = 0; i < numberOfCalls; i++)
                {
                    await sessionFactory.OpenSessionAsync();
                }

                Action act = () => sessionFactory.OpenSessionMustNotHaveBeenCalled();

                act.Should().Throw<TestException>()
                   .And.Message.Should().Be($"OpenSessionAsync must not have been called, but it was actually called {numberOfCalls} times.");
            }
        }

        public static class WhenOpenSessionMustHaveBeenCalled
        {
            [Fact]
            public static void ExceptionWhenOpenSessionWasNotCalled()
            {
                var sessionFactory = CreateSessionFactory();

                Action act = () => sessionFactory.OpenSessionMustHaveBeenCalled();

                act.Should().Throw<TestException>()
                   .And.Message.Should().Be("OpenSessionAsync must have been called exactly once, but it was actually never called.");
            }

            [Fact]
            public static async Task NoExceptionWhenOpenSessionIsCalledExactlyOnce()
            {
                var sessionFactory = CreateSessionFactory();

                await sessionFactory.OpenSessionAsync();

                sessionFactory.OpenSessionMustHaveBeenCalled().Should().BeSameAs(sessionFactory);
            }

            [Theory]
            [InlineData(5)]
            [InlineData(2)]
            [InlineData(28)]
            public static async Task ExceptionWhenOpenSessionWasCalledSeveralTimes(int numberOfCalls)
            {
                var sessionFactory = CreateSessionFactory();
                for (var i = 0; i < numberOfCalls; i++)
                {
                    await sessionFactory.OpenSessionAsync();
                }

                Action act = () => sessionFactory.OpenSessionMustHaveBeenCalled();

                act.Should().Throw<TestException>()
                   .And.Message.Should().Be($"OpenSessionAsync must have been called exactly once, but it was actually called {numberOfCalls} times.");
            }
        }

        private static SessionFactoryMock<IAsyncSession> CreateSessionFactory(Session? session = null) =>
            new (session ?? new Session());

        private sealed class Session : AsyncSessionMock { }
    }
}
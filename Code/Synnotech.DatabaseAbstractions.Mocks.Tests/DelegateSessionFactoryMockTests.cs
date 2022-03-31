using FluentAssertions;
using Xunit;

namespace Synnotech.DatabaseAbstractions.Mocks.Tests;

public static class DelegateSessionFactoryMockTests
{
    [Fact]
    public static void MustDeriveFromBaseSessionFactoryMock()
    {
        typeof(DelegateSessionFactoryMock<>).Should().BeDerivedFrom(typeof(BaseSessionFactoryMock<,>));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(17)]
    public static void CreateSessionShouldReturnThePassedInSessionObject(int numberOfCalls)
    {
        var session = new Session();
        var sessionFactory = new DelegateSessionFactoryMock<Session>(session);

        for (var i = 0; i < numberOfCalls; i++)
        {
            var createdSession = sessionFactory.CreateSession();
            createdSession.Should().BeSameAs(session);
            sessionFactory.OpenSessionCallCount.Should().Be(i + 1);
        }
    }

    private sealed class Session : AsyncSessionMock { }
}
# Synnotech.DatabaseAbstractions.Mocks

*Provides types for mocking the abstractions of [Synnotech.DatabaseAbstractions](https://github.com/Synnotech-AG/Synnotech.DatabaseAbstractions).*

[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.DatabaseAbstractions.Mocks/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-2.0.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Synnotech.DatabaseAbstractions.Mocks/)

# How to install

Synnotech.DatabaseAbstractions.Mocks is compiled against [.NET Standard 2.0 and 2.1](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) and thus supports all major plattforms like .NET 5, .NET Core, .NET Framework 4.6.1 or newer, Mono, Xamarin, UWP, or Unity.

Synnotech.DatabaseAbstractions.Mocks is available as a [NuGet package](https://www.nuget.org/packages/Synnotech.DatabaseAbstractions.Mocks/) and can be installed via:

- **Package Reference in csproj**: `<PackageReference Include="Synnotech.DatabaseAbstractions.Mocks" Version="2.0.0" />`
- **dotnet CLI**: `dotnet add package Synnotech.DatabaseAbstractions.Mocks`
- **Visual Studio Package Manager Console**: `Install-Package Synnotech.DatabaseAbstractions.Mocks`

# What does Synnotech.DatabaseAbstractions.Mocks offer you?

With Synnotech.DatabaseAbstractions.Mocks, you can easily create mock sessions for the different abstractions of [Synnotech.DatabaseAbstractions](https://github.com/Synnotech-AG/Synnotech.DatabaseAbstractions). This libary provides base classes for you that allow you to easily check that a session was correctly disposed, that changes were saved, and transaction committed.

## Mocking read-only sessions

Read-only sessions are those sessions that only read data from your database, usually not requiring a transaction. You can derive from the `AsyncReadOnlySessionMock` or `ReadOnlySessionMock` to mock the `IAsyncReadOnlySession` or `IReadOnlySession` interfaces. The following example shows this for an asynchronous use case:

```csharp
public interface IGetContactSession : IAsyncReadOnlySession
{
    Task<Contact?> GetContactAsync(int id);
}
```

Consider the following ASP.NET Core controller that uses this session:

```csharp
[ApiController]
[Route("api/contacts")]
public sealed class GetContactController : ControllerBase
{
    public GetContactController(Func<IGetContactSession> createSession) =>
        CreateSession = createSession;

    private Func<IGetContactSession> CreateSession { get; }

    [HttpGet("{id}")]
    public async Task<ActionResult<Contact>> GetContact(int id)
    {
        if (id < 1)
        {
            ModelState.AddModelError("id", "The id must at least be 1");
            return ValidationProblem();
        }

        await using var session = CreateSession();
        var contact = await session.GetContactAsync(id);
        if (contact == null)
            return NotFound();
        return contact;
    }
}
```

You could then test your controller with the following code in xunit:

```csharp
public sealed class GetContactControllerTests
{
    public GetContactControllerTests()
    {
        Session = new GetContactSessionMock();
        Controller = new GetContactController(() => Session);
    }

    private GetContactSessionMock Session { get; }
    private GetContactController Controller { get; }

    [Fact]
    public async Task MustReturnContactWhenIdIsValid()
    {
        Session.Contact = new Contact();

        var result = await Controller.GetContact(42);

        Assert.Equal(Session.Contact, result.Value);
        Session.MustBeDisposed(); // Use this to check if your controller properly disposed the session
    }

    [Fact]
    public async Task MustReturnNotFoundWhenIdIsNotExisting()
    {
        var result = await Controller.GetContact(13);

        Assert.IsType<NotFoundResult>(result.Result);
        Session.MustBeDisposed();
    }

    // AsyncReadOnlySessionMock automatically implements IAsyncReadOnlySession for you
    private sealed class GetContactSessionMock : AsyncReadOnlySessionMock, IGetContactSession
    {
        public Contact? Contact { get; set; }

        public Task<Contact?> GetContactAsync(int id) => Task.FromResult(Contact);
    }
}
```

In the above unit tests, the `GetContactSessionMock` derives from `AsyncReadOnlySessionMock` which automatically implements `IAsyncReadOnlySession` and tracks proper disposal of the session. You can use the `MustBeDisposed` method to check that the controller properly closed the session.

## Mocking sessions

If your session manipulates data and thus implements `IAsyncSession` or `ISession` for transactional support, you can derive your mocks from the `AsyncSessionMock` or `SessionMock` interfaces. The following example for updating an existing contact shows an asynchronous use case:

```csharp
public interface IUpdateContactSession : IAsyncSession
{
    Task<Contact?> GetContactAsync(int id);
}
```

The controller that uses this session might look like this:

```csharp
[ApiController]
[Route("api/contacts/update")]
public sealed class UpdateContactController : ControllerBase
{
    public UpdateContactController(Func<IUpdateContactSession> createSession,
                                   UpdateContactDtoValidator validator)
    {
        CreateSession = createSession;
        Validator = validator;
    }

    private Func<IUpdateContactSession> CreateSession { get; }
    private UpdateContactDtoValidator Validator { get; }

    [HttpPut]
    public async Task<IActionResult> UpdateContact(UpdateContactDto dto)
    {
        if (this.CheckForErrors(dto, Validator, out var badResult))
            return badResult;
        
        await using var session = CreateSession();
        var contact = await session.GetContactAsync(dto.ContactId);
        if (contact == null)
            return NotFound();
        dto.UpdateContact(contact);
        await session.SaveChangesAsync();
        return NoContent();
    }
}
```

To test this controller, we might write the following unit tests in xunit:

```csharp
public sealed class UpdateContactControllerTests
{
    public UpdateContactControllerTests()
    {
        Session = new UpdateContactSessionMock();
        Controller = new UpdateContactController(() => Session, new UpdateContactDtoValidator())
    }

    private UpdateContactSessionMock Session { get; }
    private UpdateContactController Controller { get; }

    [Fact]
    public async Task UpdateEntityWhenIdIsValid()
    {
        var contact = new Contact { Id = 1, Name = "John Doe" };
        Session.Contact = contact
        var dto = new UpdateContactDto(1, "Jane Doe");

        var result = await Controller.UpdateContact(dto);

        Assert.Equal("Jane Doe", contact.Name);
        Assert.IsType<NoContentResult>(result.Result);
        Session.SaveChangesMustHaveBeenCalled() // Use this method to ensure SaveChangesAsync was called
               .MustBeDisposed();
    }

    [Fact]
    public async Task UpdateEntityWhenIdIsNonExisting()
    {
        var dto = new UpdateContactDto(42, "Buzz Greenfield");

        var result = await Controller.UpdateContact(dto);

        Assert.IsType<NotFoundResult>(result.Result);
        Session.SaveChangesMustNotHaveBeenCalled() // Use this method to ensure that SaveChangesAsync was NOT called
               .MustBeDisposed();
    }

    private sealed class UpdateContactSessionMock : AsyncSessionMock, IUpdateContactSession
    {
        public Contact? Contact { get; set; }

        public Task<Contact?> GetContactAsync(int id) => Task.FromResult(Contact);
    }
}
```

In the above unit test, `UpdateContactSessionMock` derives from `AsyncSessionMock` which implement `IAsyncSession` and tracks calls to `SaveChangesAsync` and `DiposeAsync`. The methods `SaveChangesMustHaveBeenCalled` and `SaveChangesMustNotHaveBeenCalled` are used to ensure that `SaveChangesAsync` is properly called by the `UpdateContactController`.

## Mocking transactional sessions

*To be completed*
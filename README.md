# Unity Meetgard.Auth

This package contains an auth system based on Meetgard.

# Install

This package is not available in any UPM server. You must install it in your project like this:

1. In Unity, with your project open, open the Package Manager.
2. Either refer this GitHub project: https://github.com/AlephVault/unity-meetgard-auth.git or clone it locally and refer it from disk.
3. Also, the following packages are dependencies you need to install accordingly (in the same way and also ensuring all the recursive dependencies are satisfied):

     - https://github.com/AlephVault/unity-support-generic.git
     - https://github.com/AlephVault/unity-meetgard.git

# Usage

The first thing to know is that this is a Meetgard-based package, so take a look at the [Meetgard's documentation](https://github.com/AlephVault/unity-meetgard.git) first.

The second thing is that once you have your Meetgard project configured, you won't do direct configurations related to authentication, but leverage the power of this library.

This library offers two sets of configurations:

1. A set of boilerplates and base classes to do authentication.
2. An optional set of boilerplates and base classes to do registering.

While the code using this package will mainly make use of authentication (into which **several** methods can be defined), registering is optional and often more like _trivial_.

This documentation will first describe the boilerplates (both in terms of code and UI), and then it will describe the involved code (since users MUST implement some code).

## Generating the core pieces

There are two menu actions that matter most here: to generate the register-related pieces, and to generate the login/auth-related pieces.

### Generating the Login/Auth related pieces

This library offers what will be called as a Simple Authentication Protocol (what "simple" stands for here is: it's a simple handshake where the user sends a request and the server responds).

The menu action to help the users to create their protocols is this one:

```
Assets/Create/Aleph Vault/Meetgard.Auth/Boilerplates/Create Simple Auth Protocol
```

Once clicked, a Window will open, prompting the users to fill the following fields:

1. The base name of the protocol. It must be a `PascalCase` name and by default it's filled with `MySimpleAuth`. The involved classes and definitions will have suffixes: `ProtocolDefinition`, `ProtocolServerSide` and `ProtocolClientSide`. So in this example they'd become: `MySimpleAuthProtocolDefinition`, `MySimpleAuthProtocolServerSide` and `MySimpleAuthProtocolClientSide`. Also, a `UI`-suffixed class named `MySimpleAuthUI` will be generated.
2. The name of a first login message. It must also be a `PascalCase` name and by default it's filled with `Login`. An `ISerializable` (from `unity-binary` package) class named `Login` will be defined, with default `Username` and `Password`. This is the default login method and can totally be customized if the developer is careful enough. Also, more methods can be manually implemented later.
3. The name of the "login failed" message. This is the class that will tell when a login attempt has failed. It must also be a `PascalCase` name and by default it will be `LoginFailed`.
4. The name of the "kicked" message. This is the class that will be used when the server kicks (for whatever reason) a client. It must also be a `PascalCase` name and by default it will be `Kicked`.
5. The name of the "Account ID Type". This is the name of a class / type used for the ID of the underlying account model, if any. **this type must already exist - it will not be generated and does not need to be ISerializable**.
6. The name of the "Account Preview Data type". This is, again, a `PascalCase` name and by default will be `AccountPreviewData`. This class will be generated as an `ISerializable` class, like `Login`, `LoginFailed` and `Kicked`. This preview data will be sent to the client.
7. The name of the "Account Data type". This is, again, a `PascalCase` name and by default will be `AccountData`. This class will be generated (**not** as an `ISerializable` type since this one will not be transmitted to the client).

In order to understand the inners of these protocols, let's follow an example with these default names:

1. Base name with `ExampleSimpleAuth`.
2. Login name with `ExampleLogin`.
3. LoginFailed name with `ExampleLoginFailed`.
4. Kicked name with `ExampleKicked`.
5. Account ID type with `string`.
6. Account Preview Data type: `ExampleAccountPreviewData`.
7. Account Data type: `ExampleAccountData`.

The generated files will be detailed here and explained one by one but, first, it must be understood that the generated code involves a username/password login. Users can later add more complex implementations, however.

Also, the user must understand that the generated files _will be located at the Meetgard's by-convention paths_, so it's crucial to understand that convention from the Meetgard's documentation.

#### The protocol definition

It will be located at `Assets/Scripts/Protocols/ExampleMySimpleAuthProtocolDefinition.cs` with the contents:

```csharp
using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;

namespace Protocols {
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Types;

    public class ExampleMySimpleAuthProtocolDefinition : SimpleAuthProtocolDefinition<Nothing, Messages.ExampleLoginFailed, Messages.ExampleKicked>
    {
        // Please note: The Nothing type is used when no data is needed.
        // This means that by default there is no need for any content
        // in the successful login response.
        //
        // Typically, that is the case. If you need a custom type, feel
        // free to use any of the concrete types in AlephVault.Unity.Binary
        // or create your own type implementing ISerializable interface,
        // pretty much as it occurs in the ExampleLoginFailed class.

        /// <summary>
        ///   Defines the login messages.
        /// </summary>
        protected override void DefineLoginMessages()
        {
            // Define many alternative login messages. Typically,
            // only one will be needed, however.
            DefineLoginMessage<Messages.ExampleLogin>("Default");
        }
    }
}
```

There are some details to understand here:

1. The parent class will be not just `ProtocolDefinition` but a particular subclass: `AlephVault.Unity.Meetgard.Auth.Protocols.Simple.SimpleAuthProtocolDefinition<Nothing, Messages.ExampleLoginFailed, Messages.ExampleKicked>`.
2. The first type parameter represents _what data is sent to the user when the login is successful_. In this case, the `Nothing` type (defined in `unity-meetgard`) will be used as no further data is needed.
3. The second and third parameters are brand-new classes (they were generated in the same menu option) for the `LoginFailed` and `Kicked` respectively.
4. Notice how there's no `DefineMessages` but, instead, `DefineLoginMessages`. This is because `DefineMessages` is already overridden by this `SimpleAuthProtocolDefinition`. Users only need to override the new method instead. The purpose of this method is to define _new and alternate login options_.
5. Also notice the call of `DefineLoginMessage` instead of `DefineClientMessage`. Actually, `DefineLoginMessage` makes use of `DefineClientMessage` but the defined message ("Default" in this case) is properly namespaced under the hoods.

This is enough to define a single alternative for login. In this case, again: the data in `Messages.ExampleLogin` will be used and the user must later define how to work with that implementation (this will be detailed later).

#### The message types

They will be located at `Assets/Scripts/Protocols/Messages/{Class}.cs` with the contents:

##### The `ExampleLogin` message:

```csharp
namespace Protocols.Messages
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;

    public class ExampleLogin : ISerializable
    {
        /**
         * A typical approach to a login class is to use
         * some sort of username and password authentication
         * mechanism. As always, ensure the Meetgard server
         * runs by enabling SSL, regardless on how you design
         * this class.
         *
         * Most of the scalar types are supported for this
         * serialization mechanism (which occurs by reference).
         * Also, custom types (also implementing ISerializable)
         * can be defined and nested here (typically as a
         * readonly value) and invoke their .Serialize method
         * as part of this class' .Serialize method.
         */
         
        public string Username;
        public string Password;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref Username);
            serializer.Serialize(ref Password);
        }
    }
}
```

This default Login message messes with Username and Password. Users can change this, but provided they also change the protocol's implementations.

##### The `ExampleKicked` message:

```csharp
using System;
using AlephVault.Unity.Meetgard.Auth.Types;

namespace Protocols.Messages
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;

    public class ExampleKicked : IKickMessage<ExampleKicked>
    {
        /**
         * A kicked message has some contexts that must
         * be defined, in order to give proper answers
         * when a kick was needed.
         */
         
        public string Reason;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref Reason);
        }

        public ExampleKicked WithAccountLoadErrorReason()
        {
            Reason = "An error has occurred while trying to load the account";
            return this;
        }

        public ExampleKicked WithGhostedReason()
        {
            Reason = "The same account logged in from another client connection";
            return this;
        }

        public ExampleKicked WithLoginTimeoutReason()
        {
            Reason = "Login timeout - the client took too much to login";
            return this;
        }

        public ExampleKicked WithNonGracefulDisconnectionErrorReason(Exception reason)
        {
            Reason = $"Exception of type {reason?.GetType()?.FullName ?? "<graceful>"} on disconnection: {reason?.Message ?? "graceful"}";
            return this;
        }

        public ExampleKicked WithSessionInitializationErrorReason()
        {
            Reason = $"An error has occurred while trying to initialize the session";
            return this;
        }
    }
}
```

Notice how `ExampleKicked` implements a self-referencing interface. This forces the implementation of the methods to self-reference.

There's something important here: Here, the `Reason` is set to a long string. The user can modify the concept of `Reason` as they please but, still, a Reason needs to be serialized.

For example, the user can send short strings with representative meanings rather than these very long strings, and also serialize more arguments (e.g. for exception's type name and message).

But, still, somehow the server must be able to invoke these chained `Kicked` calls to initialize as certain message in particular. The client must only be able to decode the meaning in front-end.

##### The `ExampleLoginFailed` message:

```csharp
namespace Protocols.Messages
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;

    public class ExampleLoginFailed : ISerializable
    {
        /**
         * A typical approach to this class is to define
         * the fields for a somewhat short message on why
         * did the login attempt failed.
         */
         
        public string Reason;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref Reason);
        }
        
        public ExampleLoginFailed WithNotImplementedReason()
        {
            Reason = "Login is not yet implemented";
            return this;
        }
    }
}
```

This class will represent any message of the user's convenience related to a failed login.

In this case, there's also a `WithNotImplementedReason` method. This one also initializes the message in a chained method, but this time this method is NOT part of a mandatory interface: instead, it's a boilerplate-only method responding to a boilerplate-only implementation.

##### The `ExampleAccountPreviewData` message:

```csharp
namespace Protocols.Messages
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;

    public class ExampleAccountPreviewData : ISerializable
    {
        /**
         * The account preview stands for a small part
         * of the account data to send as preliminary.
         *
         * By default, this implementation covers only
         * the username, but can be changed according
         * to the game needs.
         */
         
        public string Username;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref Username);
        }
    }
}
```

This message only holds information about the current username. It's used to reflect the logged user's front-available data to the client.

Again: This can be customized to extend more data, but the implementing code in the server-side must also be customized accordingly.

#### The `ExampleAccountData` class:

This is a server-side only type. It's used to track the account data for each user already logged in. This data is **not** sent to the clients.

It is located at: `Assets/Scripts/Server/Authoring/Types/ExampleAccountData.cs` and looks like this:

```csharp
namespace Server.Authoring.Types
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Auth.Types;
    using Protocols.Messages;

    public class ExampleAccountData : IRecordWithPreview<string, ExampleAccountPreviewData>
    {
        /**
         * The account full data contains the data that is relevant
         * to the game, but is not needed in client front-end at all.
         *
         * This data typically holds the same data as the preview,
         * and also more data, depending on the needs. It also has
         * an ID (for internal purposes), typically related to some
         * sort of external storage.
         */
         
        public string Username;

        public string GetID()
        {
            // This method NEEDS to be implemented!
            return default(string);
        }

        public ExampleAccountPreviewData GetProfileDisplayData()
        {
            // This method can be changed (actually, it must depend
            // on the changes applied to ExampleAccountPreviewData).
            return new ExampleAccountPreviewData() { Username = Username };
        }
    }
}
```

This class must be completely implemented. For example, implementing `GetID` might  return an internal from-database ID or the same Username.

Alternatively, it may be changed entirely if the underlying implementation of the server-side protocol will also be changed.

#### The protocol implementation: Client side

This one is located at: `Assets/Scripts/Client/Authoring/Behaviours/Protocols/ExampleMySimpleAuthProtocolClientSide.cs` and looks like this:

```csharp
using System;
using System.Threading.Tasks;
using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using Protocols;
using Protocols.Messages;

namespace Client.Authoring.Behaviours.Protocols
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Types;

    public class ExampleMySimpleAuthProtocolClientSide : SimpleAuthProtocolClientSide<ExampleMySimpleAuthProtocolDefinition, Nothing, ExampleLoginFailed, ExampleKicked>
    {
        // Please note: The Nothing type is used when no data is needed.
        // This means that by default there is no need for any content
        // in the successful login response. See the ExampleMySimpleAuthProtocolDefinition
        // type for more details.

        // A default login sender.
        public Func<ExampleLogin, Task> DefaultLoginSender { get; private set; }

        /// <summary>
        ///   Makes the senders for the login messages.
        /// </summary>
        protected override void MakeLoginRequestSenders()
        {
            // For each defined login message in the protocol definition,
            // the sender must be created in this method. Since one login
            // message was defined in the protocol definition, one sender
            // is being created in this method.
            DefaultLoginSender = MakeLoginRequestSender<ExampleLogin>("Default");
        }
                
        /**
         * This class has the following events that can be listened for. They are:
         *
         * - Handshake.OnWelcome = async () => { ... }; for when this client received
         *   from the server the first message. This client should, as immediately as
         *   possible, send the login message with some pre-fetched data.
         *
         * - Handshake.OnTimeout = async () => { ... }; for when this client received
         *   from the server a timeout message. This client should know the server will
         *   disconnect it immediately and also render a message, since the server did
         *   not receive, in certain threshold time, a login message.
         *
         * - OnLoginOK = async (ok) => { ... }; for when this client received a
         *   message telling the login was successful. The client should expect
         *   more messages from the server (e.g. account being set up and things
         *   needing a render in the client side).
         *
         * - OnLoginFailed = async (reason) => { ... }; for when this client
         *   received a message telling the login attempt was unsuccessful. This
         *   also implies that the client should consider this connection to be
         *   terminated automatically.
         *
         * - OnKicked = async (reason) => { ... }; for when this client received
         *   a message telling it was kicked. The reason is attached. This also
         *   implies that the client should consider this connection to be also
         *   terminated, and things should be cleared from the scene accordingly.
         *
         * - OnLoggedOut = async () => { ... }; for when this client received a
         *   logged out message. This means: the server processed and accepted
         *   a logout request from the client, cleared everything and terminated
         *   the connection. This implies that the client should consider the
         *   connection to be terminated, and things should be cleared from the
         *   scene accordingly.
         *
         * - OnAlreadyLoggedIn = async () => { ... }; for when the client received
         *   a message from the server telling that it is already logged in (with
         *   the same or another account). The client seems to be in some sort of
         *   locally inconsistent state to reach this message. It should fix its
         *   local state accordingly.
         *
         * - OnAccountAlreadyInUse = async () => { ... }; for when the client
         *   received a message from the server telling that the account is already
         *   in use. This looks pretty much as a login failure message, so clients
         *   receiving this message should consider that the connection terminated
         *   when receiving this message.
         *
         * - OnNotLoggedIn = async () => { ... }; for when the client received a
         *   message from the server telling that whatever message was sent by the
         *   client, it was not authorized since the client did not perform any
         *   kind of login yet.
         *
         * - OnForbidden = async () => { ... }; for when the client received a
         *   message from the server telling that whatever message was sent by
         *   the client, despite the client was logged in, it was not authorized
         *   to send that request.
         *
         * Additionally, there is a Logout() method to send a logout to the server.
         *
         * At any moment, it may be invoked as _ = Logout() or await Logout(). 
         *
         * Finally, given a typed or void sender, a wrapped version of that sender
         * can be created by invoking `MakeLoginRequiredSender`. This wrapper function
         * creates a new sender which does not send the actual command if the client
         * is not logged in. In such cases, it triggers the OnNotLoggedIn command by
         * itself (with no server interaction). If a second argument is given as `true`
         * then instead of triggering the OnNotLoggedIn event an exception of type
         * `ExampleMySimpleAuthProtocolClientSide.NotLoggedInException` will be thrown.
         */
    }
}
```

Pay attention to the implementation:

1. First, a public sender named `DefaultLoginSender()` will be provided. This one stands for the default method, exactly as that underlying message being defined in the protocol definition. The user will be totally free to choose when to invoke that method.
2. It is mandatory to have this method: `protected override void MakeLoginRequestSenders()`, but it might also define **more than just one login sender**, provided it is defined in the definition and implemented in the server-side.
3. There are many callbacks properly described in the code. All of them are async (void) `Task`-returning callbacks.
   1. The `ok` argument is of `Nothing` type, matching the second type parameter of the class.
   2. The `reason` argument for the `OnLoginFailed` callback is of `Messages.LoginFailed` type, matching the third type parameter of the class.
   3. The `reason` argument for the `OnKicked` callback is of `Messages.Kicked` type, matching the fourth parameter type of the class.

The generated code is self-documented on when each callback is triggered.

Also, there's a `Logout()` method. It sends a disconnection message and then, after it was sent, closes the connection on the client side.

Finally, the `MakeLoginRequiredSender` can be invoked for both typed and void senders (those functions created with `MakeSender` and `MakeSender<T>`):

1. `sender = loginProtocolComponent.MakeLoginRequiredSender(sender);` wraps it with a function that, when invoked, it will only send the command if it's already logged in. If not logged in, it will directly trigger the `OnNotLoggedIn` event.
2. `sender = loginProtocolComponent.MakeLoginRequiredSender(sender, true);` wraps it with a function that, when invoked, it will only send the command if it's already logged in. If not logged in, it will raise the `ExampleMySimpleAuthProtocolClientSide.NotLoggedInException` exception.

#### The protocol implementation: Server side

This one is located at: `Assets/Scripts/Server/Authoring/Behaviours/Protocols/ExampleMySimpleAuthProtocolServerSide.cs` and looks like this:

```csharp
using System;
using System.Threading.Tasks;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using Protocols;
using Protocols.Messages;
using Server.Authoring.Types;

namespace Server.Authoring.Behaviours.Protocols
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Types;

    public class ExampleMySimpleAuthProtocolServerSide : SimpleAuthProtocolServerSide<
        ExampleMySimpleAuthProtocolDefinition, Nothing, ExampleLoginFailed, ExampleKicked,
        string, ExampleAccountPreviewData, ExampleAccountData
    >
    {
        // Please note: The Nothing type is used when no data is needed.
        // This means that by default there is no need for any content
        // in the successful login response. See the ExampleMySimpleAuthProtocolDefinition
        // type for more details.
    
        /// <summary>
        ///   Makes the handlers for the login messages.
        /// </summary>
        protected override void SetLoginMessageHandlers()
        {
            // For each defined login message in the protocol definition,
            // the handler must be created in this method. Since one login
            // message was defined in the protocol definition, one handler
            // is being created in this method.
            AddLoginMessageHandler<ExampleLogin>("Default", async (login) => {
                // This method requires a totally custom implementation
                // from the user.
                //
                // Given the login details, they are either valid or
                // invalid. If the login is valid, then it must return:
                // - A successful response. By default, the successful
                //   response is defined of type Nothing, so the value
                //   will be Nothing.Instance.
                // - The account id, of type: string.
                // 
                // return AcceptLogin(successfulReason, accountId);
                //
                // Otherwise, for invalid login attempts, a rejection
                // reason must be generated, of type: ExampleLoginFailed.
                //
                // return RejectLogin(unsuccessfulReason);

                // WARNING: EVERY CALL TO AN EXTERNAL API OR USING A GAME OBJECT
                //          OR BEHAVIOUR MUST BE DONE IN THE CONTEXT OF A CALL TO
                //          RunInMainThread OR IT WILL SILENTLY FAIL.
                
                return RejectLogin(new ExampleLoginFailed().WithNotImplementedReason());
            });
        }
        
        /// <summary>
        ///   Retrieves the "full" account data, to store it in the session.
        /// </summary>
        protected override async Task<ExampleAccountData> FindAccount(string id)
        {
            // This doesn't mean that the returned data is actually the FULL
            // data of an account, but at least some sort of "long" representation
            // of the data that is sensible to retrieve when successfully doing
            // a login operation.
            //
            // This requires a mandatory implementation, for returning the default
            // value will cause the protocol to raise an exception and terminate
            // the session (& connection) abruptly and without even starting.

            // WARNING: EVERY CALL TO AN EXTERNAL API OR USING A GAME OBJECT
            //          OR BEHAVIOUR MUST BE DONE IN THE CONTEXT OF A CALL TO
            //          RunInMainThread OR IT WILL SILENTLY FAIL.

            return default(ExampleAccountData);
        }
        
        /// <summary>
        ///   Retrieves what happens when an account tries to login (from another
        ///   connection) considering that it is already logged in in one connection.
        /// </summary>
        protected override AccountAlreadyLoggedManagementMode IfAccountAlreadyLoggedIn()
        {
            // The values are:
            // - AccountAlreadyLoggedManagementMode.Ghost, to kick the previous connection.
            // - AccountAlreadyLoggedManagementMode.Reject, to reject the new connection.
            // - AccountAlreadyLoggedManagementMode.AllowAll, to allow all the connections.
            return AccountAlreadyLoggedManagementMode.Ghost;
        }
        
        /// <summary>
        ///   Handles an error that occurred in the session management.
        /// </summary>
        protected override async Task OnSessionError(ulong clientId, SessionStage stage, System.Exception error)
        {
            // The stage value can be:
            // - SessionStage.AccountLoad: The account data was being loaded.
            // - SessionStage.Initialization: The session was being initialized.
            // - SessionStage.PermissionCheck: The session was checking a permission for a LoginRequired message.
            // - SessionStage.Termination: The session was terminating.

            // WARNING: EVERY CALL TO AN EXTERNAL API OR USING A GAME OBJECT
            //          OR BEHAVIOUR MUST BE DONE IN THE CONTEXT OF A CALL TO
            //          RunInMainThread OR IT WILL SILENTLY FAIL.
        }
        
        /**
         * This class has the following events that can be listened for. They are:
         *
         * - OnSessionStarting = async (connection, fullAccountData) => { ... }; for when
         *   the login was successful and the session is starting. The session should
         *   store some data by grabbing it from the fullAccountData.
         *
         * - OnSessionTerminating = async (connection, reason) => { ... }; for when the account
         *   is being disconnected and the session is terminating for it. However, for
         *   that moment the session data still exists, and this is a last opportunity
         *   to read data from it and do something with it.
         *
         * This class also defines some public methods that can be used from this or
         * other protocols. They're:
         *
         * - await Kick(accountId, reason) to force disconnecting an account.
         * - SessionExists(accountId) to know whether an account is logged in.
         * - (SomeType)GetSessionData(connection, key) to get some data from the session.
         * - TryGetSessionData(connection, key, out val) ? (SomeType)val : default(SomeType)
         *   to get some data from the session without risking a KeyNotFoundException.
         * - SetSessionData(connection, key, val) to set data in the session.
         * - SessionContainsKey(connection, key) to know whether that key is set in the session.
         * - RemoveSessionData(connection, key) to clear a key from the session.
         * - ClearSessionUserData(connection) to clear the custom session data.
         *   ClearSessionUserData(connection, true) to include the internal session's core keys.
         *
         * Finally, this class has methods that are of interest of a login protocol: those for
         * actually restricting requests from non-logged users. These methods are:
         *
         * - handler = LoginRequired([async (connection) => { ... return whether connection is allowed}, ]handler)
         *   Checks whether the connection is logged in and allowed to perform the action.
         *   A new handler is returned, which typically becomes the value for a call to
         *   AddIncomingMessageHandler in another protocol.
         *   - This method has TWO versions: one for untyped handlers, and one for typed ones.
         *   - The first parameter is optional. If not set, then no allowance check is done.
         * - handler = LogoutRequired(handler)
         *   Checks whether the connection is NOT logged in.
         *   A new handler is returned, which typically becomes the value for a call to
         *   AddIncomingMessageHandler in another protocol.
         *   - This method has TWO versions: one for untyped handlers, and one for typed ones.
         * - handler = LogoutRequired(handler)
         *   Checks whether the connection is logged out and allowed to perform the action.
         *   This is the exact inverse of LoginRequired.
         */
    }
}
```

This one is the most complex class to understand:

1. A mandatory method `protected override void SetLoginMessageHandlers()` must implement the login methods that are defined in the protocol definition.
   1. Each login method must have a logic that invokes either `RejectLogin(response)` where `response` is of type `ExampleLoginFailed` (matching the third type parameter) or `AcceptLogin` which accepts a `Nothing` successful response (matching the second type parameter) and a `string` value (matching the fifth type parameter).
   2. It has its own documentation and recommendations. Users must pay attention to it.
2. A mandatory method `protected override async Task<ExampleAccountData> FindAccount(string id)` is used to retrieve the user information given its id. Notice how `string` and `ExampleAccountData` match the proper type parameters in the class. On failure, the method must return `default`.
3. A mandatory method `protected override AccountAlreadyLoggedManagementMode IfAccountAlreadyLoggedIn()` telling what to do (described in the comments) when an account happens to simultaneously log in again.
4. A mandatory method `protected override async Task OnSessionError(ulong clientId, SessionStage stage, System.Exception error)` telling what to do when a session life-cycle incurs in error. After this callback, the session will be closed.

This class also has callbacks (to be used by the other protocols that require them) described there.

1. The `connection` argument is of type `ulong`.
2. The `fullAccountData` argument is of type `ExampleAccountData`.
3. The `reason` in `OnSessionTerminating` is either `null` (for graceful connection) or an `ExampleKicked` object.
4. These all match the proper type parameters.

There are also a bunch of methods that are described there:

1. `public async Task Kick(ulong connection, ExampleKicked reason)` kicks a user, closing its connection and sending the reason.
2. `public bool SessionExists(string accountId)` tells whether a user (by its id, of `string` type in this class) is logged or not.
3. `public object GetSessionData(ulong connection, string key)` retrieves a specific string key (always a string) from transient session data.
4. `public bool TryGetSessionData(ulonh connection, string key, out object val)` is similar but without exception triggering.
5. `public void SetSessionData(ulong connection, string key, object val)` is the complementary function.
6. `public void RemoveSessionData(ulong connection, string key)` clears a key.
7. `public void ClearSessionUserData(ulong connection[, bool everything = false])` clears the entire session data. If `everything` is `true` then also clear the system-related keys, not just user-defined keys.
8. `public bool SessionContainsKey(ulong connection, string key)` tells whether this session has a value at that key.

Here, keys are always string. There's no type parameter for that (it does not make sense).

Finally, there's a public `LoginRequired` method. It takes a callback users would typically pass to `AddIncomingMessageHandler`, and returns a callback users would typically pass to `AddIncomingMessageHandler`.

> The difference is that the generated callback fails (and sends a proper message to the client, which is attended by the callback `OnNotLoggedIn`).

There are many versions of `LoginRequired` which may be used by different protocols. Take a look at that method in the code:

1. `callback = loginProtocolComponent.LoginRequired(async (...) => {})` only checks if the user is logged in.
2. `callback = loginProtocolComponent.LoginRequired(async (ulong connection) => { /* return true if the connection is allowed for this command, or false if not */ }, async (...) => {})` also checks if the connection is allowed to execute this command. If the check callback returns false, the user is not allowed and the client will have the `OnForbidden` callback triggered.

#### Finally, the UI component

A `UI` component is also generated: `ExampleMySimpleAuthUI`. It's located at: `Assets/Scripts/Client/Authoring/Behaviours/UI/ExampleMySimpleAuthUI.cs`. It looks like this:

```csharp
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using Protocols.Messages;

namespace Client.Authoring.Behaviours.UI
{
    using Protocols;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class ExampleMySimpleAuthUI : MonoBehaviour
    {
        /// <summary>
        ///   The involved network client.
        /// </summary>
        [SerializeField]
        private NetworkClient client;
        
        /// <summary>
        ///   The address to connect to.
        /// </summary>
        [SerializeField]
        private string address = "localhost";

        /// <summary>
        ///   The port to connect to.
        /// </summary>
        [SerializeField]
        private ushort port = 6777;

        /// <summary>
        ///   The username field.
        /// </summary>
        [SerializeField]
        private InputField username;

        /// <summary>
        ///   The password field.
        /// </summary>
        [SerializeField]
        private InputField password;
        
        /// <summary>
        ///   A status label.
        /// </summary>
        [SerializeField]
        private Text statusLabel;
        
        /// <summary>
        ///   The submit button.
        /// </summary>
        [SerializeField]
        private Button submit;
        
        /// <summary>
        ///   The UI to show when online.
        /// </summary>
        [SerializeField]
        private GameObject onlineUI;
        
        /// <summary>
        ///   The UI to show when offline.
        /// </summary>
        [SerializeField]
        private GameObject offlineUI;
        
        // The client's register protocol.
        private ExampleMySimpleAuthProtocolClientSide protocol;
                 
        private void Awake()
        {
            if (!client)
            {
                throw new Exception("No network client is referenced in this object!");
            }
            protocol = client.GetComponent<ExampleMySimpleAuthProtocolClientSide>();
            if (!protocol)
            {
                throw new Exception("The network protocol does not have a behaviour of instance " +
                                    "ExampleMySimpleAuthProtocolClientSide attached to it");
            }
            
            if (!username || !password)
            {
                throw new Exception("The register form fields are not properly initialized!");
            }
            
            if (!submit)
            {
                throw new Exception("The submit button is not properly initialized!");
            }
        }
        
        private Task UseCanvas(bool online)
        {
            return protocol.RunInMainThread(() => {
                onlineUI.SetActive(online);
                offlineUI.SetActive(!online);
            });
        }
        
        private void Start()
        {
            submit.onClick.AddListener(OnSubmitClick);
            client.OnConnected += OnClientConnected;
            client.OnDisconnected += OnClientDisconnected;            
            protocol.OnLoginOK += OnLoginOK;
            protocol.OnLoginFailed += OnLoginFailed;
        }
        
        private void OnDestroy()
        {
            submit.onClick.RemoveListener(OnSubmitClick);
            client.OnConnected -= OnClientConnected;
            client.OnDisconnected -= OnClientDisconnected;            
            protocol.OnLoginOK -= OnLoginOK;
            protocol.OnLoginFailed -= OnLoginFailed;
        }
        
        private void OnClientConnected()
        {
            submit.interactable = false;
        }
        
        private void OnClientDisconnected()
        {
            protocol.Handshake.OnWelcome -= OnWelcome;
            protocol.Handshake.OnTimeout -= OnTimeout;
            submit.interactable = true;
            UseCanvas(false);
        }
        
        private async Task OnLoginOK(Nothing _)
        {
            // Please note: The argument type must match the Auth protocol definition!
            SetStatus("Login was successful!");
            await UseCanvas(true);
        }
        
        private async Task OnLoginFailed(ExampleLoginFailed reason)
        {
            // Please note: The argument type must match the Auth protocol definition!
            SetStatus(reason.Reason);
        }
        
        private void OnSubmitClick()
        {
            submit.interactable = false;
            try
            {
                SetStatus("Connecting...");
                protocol.Handshake.OnWelcome += OnWelcome;
                protocol.Handshake.OnTimeout += OnTimeout;
                client.Connect(address, port);
            }
            catch(System.Exception)
            {
                SetStatus("Connection error!");
                protocol.Handshake.OnWelcome -= OnWelcome;
                protocol.Handshake.OnTimeout -= OnTimeout;
                submit.interactable = true;
            }
        }
        
        private async Task OnWelcome()
        {
            SetStatus("Logging in...");
            protocol.Handshake.OnWelcome -= OnWelcome;
            await protocol.DefaultLoginSender(new ExampleLogin() {
                Username = username.text,
                Password = password.text
            });
        }
        
        private async Task OnTimeout()
        {
            SetStatus("Handshake timeout!");            
        }
        
        private void SetStatus(string value)
        {
            if (statusLabel) {
                protocol.RunInMainThread(() => {
                    statusLabel.text = value;
                });
            }
        }
    }
}
```

This is **not** a protocol behaviour but a regular Canvas object which connects to a `NetworkClient` object that must be assigned by the user at design time.

Notice how, aside of the host/port fields, there are other fields which are related to the `ExampleLogin` class:

1. A `username` field, to read the username.
2. A `password` field, to read the password.
3. A submit button, to send the login attempt.
4. A status label, telling the status of the login attempt and perhaps any error. Optional but highly recommended.
5. An object to show when the connection is established, and an object to show when the connection is offline (typically, this one has the input elements 1, 2, 3 and 4).

Once all those elements are set, this behaviour performs everything, hides everything, disables/enables everything, ... the user needs no further logic.

### Generating the Register related pieces

This one is simpler and involves less code being generated and understood. This involves a similar code generation and a new menu option:

```
Assets/Create/Aleph Vault/Meetgard.Auth/Boilerplates/Create Simple Register Protocol
```

Similarly, a window will open prompting for data with these fields:

1. The base name, similar to the auth case, this time being by default `MySimpleRegister`. It will be the base name of the protocol (e.g. `MySimpleRegisterProtocolDefinition`, `MySimpleRegisterProtocolServerSize`, `MySimpleRegisterProtocolClientSide`).
2. The name of a register message, sent by the client to the server, with the register payload. Also `CamelCase`, by default `Register`.
3. The name of a "register failed" message, sent by the server to the client, with the failure details when a register attempt was not valid. Also `CamelCase`, by default `RegisterFailed`.

In order to understand the inners of these protocols, let's follow an example with these default names:

1. Base name with `ExampleMySimpleRegister`.
2. Login name with `ExampleRegister`.
3. LoginFailed name with `ExampleRegisterFailed`.

The generated files will be detailed here and explained one by one but, first, it must be understood that the generated code involves a username/password registration. Users can later add more complex implementations, however.

Also, the user must understand that the generated files _will be located at the Meetgard's by-convention paths_, so it's crucial to understand that convention from the Meetgard's documentation.

#### The protocol definition

It will be located at `Assets/Scripts/Protocols/ExampleMySimpleRegisterProtocolDefinition.cs` with the contents:

```csharp
using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;

namespace Protocols {
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Types;

    public class ExampleMySimpleRegisterProtocolDefinition : SimpleRegisterProtocolDefinition<Nothing, Messages.ExampleRegisterFailed>
    {
        // Please note: The Nothing type is used when no data is needed.
        // This means that by default there is no need for any content
        // in the successful register response.
        //
        // Typically, that is the case. If you need a custom type, feel
        // free to use any of the concrete types in AlephVault.Unity.Binary
        // or create your own type implementing ISerializable interface,
        // pretty much as it occurs in the ExampleRegisterFailed class.

        /// <summary>
        ///   Defines the register messages.
        /// </summary>
        protected override void DefineRegisterMessages()
        {
            // Define many alternative register messages. Typically,
            // only one will be needed, however.
            DefineRegisterMessage<Messages.ExampleRegister>("Default");
        }
    }
}
```

Similar to the Authentication protocol, this one offers a `DefineRegisterMessages` into which the users must call one or more `DefineRegisterMessage` with a name ("Default" is also given by default here) and a proper serializable type.

> While more alternatives can be defined, typically only one is defined.

The rejection message is of type `ExampleRegisterFailed` and proper default messages are already defined for this.

The acceptance message is, in this implementation (it can be changed), the `Nothing` type. It means no data will be sent when everything is OK.

#### The message types

As with the Authentication protocol, some messages will be defined here at `Assets/Scripts/Protocols/Messages/{Class}.cs` with their contents:

##### The `ExampleRegister` message:

```csharp
namespace Protocols.Messages
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;

    public class ExampleRegister : ISerializable
    {
        /**
         * A typical approach to a register class is to use
         * some sort of username and password authentication
         * mechanism. As always, ensure the Meetgard server
         * runs by enabling SSL, regardless on how you design
         * this class.
         *
         * Most of the scalar types are supported for this
         * serialization mechanism (which occurs by reference).
         * Also, custom types (also implementing ISerializable)
         * can be defined and nested here (typically as a
         * readonly value) and invoke their .Serialize method
         * as part of this class' .Serialize method.
         */
         
        public string Username;
        public string Password;
        public string PasswordConfirm;
        // Uncomment this one if your account uses a Display Name,
        // as part of the account instead of a separate data type.
        //
        // public string DisplayName;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref Username);
            serializer.Serialize(ref Password);
            serializer.Serialize(ref PasswordConfirm);
            // Uncomment this one if your account uses a Display Name,
            // as part of the account instead of a separate data type.
            //
            // serializer.Serialize(ref DisplayName);
        }
    }
}
```

This one is related to the default implementation: Only registering username and password, with also a password confirm. It is suggested that the user can add a `DisplayName` class for registering a public name.

This implementation can entirely be customized, but again: it requires customizing the entire logic interacting with it.

##### The `ExampleRegisterFailed` message:

```csharp
namespace Protocols.Messages
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;

    public class ExampleRegisterFailed : ISerializable
    {
        /**
         * A typical approach to this class is to define
         * the fields for a somewhat short message on why
         * did the register attempt failed.
         */
         
        public string Reason;

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref Reason);
        }
        
        public ExampleRegisterFailed WithPasswordMismatchError()
        {
            Reason = "Password and confirmation do not match";
            return this;
        }
        
        public ExampleRegisterFailed WithValidationError()
        {
            Reason = "Invalid username and/or password";
            return this;
        }
        
        public ExampleRegisterFailed WithUnknownError()
        {
            Reason = "An unknown error has occurred. Try again later";
            return this;
        }

        public ExampleRegisterFailed WithNotImplementedReason()
        {
            Reason = "Register is not yet implemented";
            return this;
        }
    }
}
```

Notice how these methods are implemented: They're not part of a mandatory interface, but instead they're used in the default logic for registering a user/password account.

Also, notice how validation is NOT implemented here. **Be careful implementing the validation since big response payloads might not fit in Meetgard's sent/received messages**.

#### The protocol implementation: Client side

This one is located at: `Assets/Scripts/Client/Authoring/Behaviours/Protocols/ExampleMySimpleRegisterProtocolClientSide.cs` and looks like this:

```csharp
using System;
using System.Threading.Tasks;
using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using Protocols;
using Protocols.Messages;

namespace Client.Authoring.Behaviours.Protocols
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Types;

    public class ExampleMySimpleRegisterProtocolClientSide : SimpleRegisterProtocolClientSide<ExampleMySimpleRegisterProtocolDefinition, Nothing, ExampleRegisterFailed>
    {
        // Please note: The Nothing type is used when no data is needed.
        // This means that by default there is no need for any content
        // in the successful register response. See the ExampleMySimpleRegisterProtocolDefinition
        // type for more details.

        // A default register sender.
        public Func<ExampleRegister, Task> DefaultRegisterSender { get; private set; }

        /// <summary>
        ///   Makes the senders for the register messages.
        /// </summary>
        protected override void MakeRegisterRequestSenders()
        {
            // For each defined register message in the protocol definition,
            // the sender must be created in this method. Since one register
            // message was defined in the protocol definition, one sender
            // is being created in this method.
            DefaultRegisterSender = MakeRegisterRequestSender<ExampleRegister>("Default");
        }
        
        /**
         * This class has the following events that can be listened for. They are:
         *
         * - Handshake.OnWelcome = async () => { ... }; for when this client received
         *   from the server the first message. This client should, as immediately as
         *   possible, send the register message with some pre-fetched data.
         *
         * - Handshake.OnTimeout = async () => { ... }; for when this client received
         *   from the server a timeout message. This client should know the server will
         *   disconnect it immediately and also render a message, since the server did
         *   not receive, in certain threshold time, a register message.
         *
         * - OnRegisterOK = async (ok) => { ... }; for when this client received a
         *   message telling the register was successful. The client should expect
         *   more messages from the server (e.g. account being set up and things
         *   needing a render in the client side).
         *
         * - OnRegisterFailed = async (reason) => { ... }; for when this client
         *   received a message telling the register attempt was unsuccessful.
         *   This also implies that the client should consider this connection
         *   to be terminated automatically.
         */
    }
}
```

Notice how there's a public method: `DefaultRegisterSender(some ExampleRegister message)` which will attempt a register.

Also, there are some callbacks properly documented, considering:

1. The `ok` will be of `Nothing` type, matching the second type parameter.
2. The `reason` will be of `ExampleRegisterFailed` type, matching the third type parameter.

Pay attention to the callbacks and properly implement them for each received message.

#### The protocol implementation: Server side

This one is located at: `Assets/Scripts/Server/Authoring/Behaviours/Protocols/ExampleMySimpleRegisterProtocolServerSide.cs` and looks like this:

```csharp
using System;
using System.Threading.Tasks;
using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using Protocols;
using Protocols.Messages;

namespace Server.Authoring.Behaviours.Protocols
{
    using AlephVault.Unity.Binary;
    using AlephVault.Unity.Binary.Wrappers;
    using AlephVault.Unity.Meetgard.Types;

    public class ExampleMySimpleRegisterProtocolServerSide : SimpleRegisterProtocolServerSide<ExampleMySimpleRegisterProtocolDefinition, Nothing, ExampleRegisterFailed>
    {
        /// <summary>
        ///   Makes the handlers for the register messages.
        /// </summary>
        protected override void SetRegisterMessageHandlers()
        {
            // For each defined register message in the protocol definition,
            // the handler must be created in this method. Since one register
            // message was defined in the protocol definition, one handler
            // is being created in this method.
            //
            // Typically, the handler to add would be for non-logged users.
            //
            // If this component is attached to the same object that has an
            // authentication protocol, you might want to wrap this handler
            // using .LogoutRequired from that component.
            AddRegisterMessageHandler<ExampleRegister>("Default", async (register) => {
                // This method requires a totally custom implementation
                // from the user.
                //
                // Given the login details, they are either valid or
                // invalid. If the register is valid, then it must return:
                // - A successful response. By default, the successful
                //   response is defined of type Nothing, so the value
                //   will be Nothing.Instance.
                // 
                // return AcceptRegister(successfulReason);
                //
                // Otherwise, for invalid register attempts, a rejection
                // reason must be generated, of type: ExampleRegisterFailed.
                //
                // return RejectRegister(unsuccessfulReason);
                
                // WARNING: EVERY CALL TO AN EXTERNAL API OR USING A GAME OBJECT
                //          OR BEHAVIOUR MUST BE DONE IN THE CONTEXT OF A CALL TO
                //          RunInMainThread OR IT WILL SILENTLY FAIL.

                if (register.Password != register.PasswordConfirm)
                {
                    return RejectRegister(new ExampleRegisterFailed().WithPasswordMismatchError());
                }

                return RejectRegister(new ExampleRegisterFailed().WithNotImplementedReason());
            });
        }
    }
}
```

The idea is analogous to the authentication protocol:

1. Use `return AcceptRegister(Nothing value)` passing a `Nothing` instance (unless it's needed to use another `RegisterOK` type for the entire protocol, but by default it's `Nothing`) when the register is OK.
2. Use `return RejectRegister(ExampleRegisterFailed reason)` passing a proper `ExampleRegisterFailed` instance when something went wrong. It's up to the user to tell the reasons and make them make sense in client side.

#### Finally, the UI component

A `UI` component is also generated: `ExampleMySimpleAuthUI`. It's located at: `Assets/Scripts/Client/Authoring/Behaviours/UI/ExampleMySimpleRegisterUI.cs`. It looks like this:

```csharp
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using Protocols.Messages;

namespace Client.Authoring.Behaviours.UI
{
    using Protocols;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class ExampleMySimpleRegisterUI : MonoBehaviour
    {
        /// <summary>
        ///   The involved network client.
        /// </summary>
        [SerializeField]
        private NetworkClient client;
        
        /// <summary>
        ///   The address to connect to.
        /// </summary>
        [SerializeField]
        private string address = "localhost";

        /// <summary>
        ///   The port to connect to.
        /// </summary>
        [SerializeField]
        private ushort port = 6777;

        /// <summary>
        ///   The username field.
        /// </summary>
        [SerializeField]
        private InputField username;

        /// <summary>
        ///   The password field.
        /// </summary>
        [SerializeField]
        private InputField password;

        /// <summary>
        ///   The password confirm field.
        /// </summary>
        [SerializeField]
        private InputField passwordConfirm;
        
        /// <summary>
        ///   A status label.
        /// </summary>
        [SerializeField]
        private Text statusLabel;

        /**
         * Uncomment this field if the register involves some sort
         * of display name for the registered accounts, instead of
         * a separate concept / data type.
         *
         * /// <summary>
         * ///   The display name field.
         * /// </summary>
         * [SerializeField]
         * private InputField displayName;
         */
        
        /// <summary>
        ///   The submit button.
        /// </summary>
        [SerializeField]
        private Button submit;
        
        // The client's register protocol.
        private ExampleMySimpleRegisterProtocolClientSide protocol;
                 
        private void Awake()
        {
            if (!client)
            {
                throw new Exception("No network client is referenced in this object!");
            }
            protocol = client.GetComponent<ExampleMySimpleRegisterProtocolClientSide>();
            if (!protocol)
            {
                throw new Exception("The network protocol does not have a behaviour of instance " +
                                    "ExampleMySimpleRegisterProtocolClientSide attached to it");
            }
            
            // Remove the comment on the displayName variable if it is used by this form.
            if (!username || !password || !passwordConfirm /* || displayName */)
            {
                throw new Exception("The register form fields are not properly initialized!");
            }
            
            if (!submit)
            {
                throw new Exception("The submit button is not properly initialized!");
            }
        }
        
        private void Start()
        {
            submit.onClick.AddListener(OnSubmitClick);
            client.OnConnected += OnClientConnected;
            client.OnDisconnected += OnClientDisconnected;            
            protocol.OnRegisterOK += OnRegisterOK;
            protocol.OnRegisterFailed += OnRegisterFailed;
        }
        
        private void OnDestroy()
        {
            submit.onClick.RemoveListener(OnSubmitClick);
            client.OnConnected -= OnClientConnected;
            client.OnDisconnected -= OnClientDisconnected;            
            protocol.OnRegisterOK -= OnRegisterOK;
            protocol.OnRegisterFailed -= OnRegisterFailed;
        }
        
        private void OnClientConnected()
        {
            submit.interactable = false;
        }
        
        private void OnClientDisconnected()
        {
            protocol.Handshake.OnWelcome -= OnWelcome;
            protocol.Handshake.OnTimeout -= OnTimeout;
            submit.interactable = true;
        }
        
        private async Task OnRegisterOK(Nothing _)
        {
            // Please note: The argument type must match the Register protocol definition!
            SetStatus("Register was successful!");
        }
        
        private async Task OnRegisterFailed(ExampleRegisterFailed reason)
        {
            // Please note: The argument type must match the Register protocol definition!
            SetStatus(reason.Reason);
        }
        
        private void OnSubmitClick()
        {
            submit.interactable = false;
            try
            {
                SetStatus("Connecting...");
                protocol.Handshake.OnWelcome += OnWelcome;
                protocol.Handshake.OnTimeout += OnTimeout;
                client.Connect(address, port);
            }
            catch(System.Exception)
            {
                SetStatus("Connection error!");
                protocol.Handshake.OnWelcome -= OnWelcome;
                protocol.Handshake.OnTimeout -= OnTimeout;
                submit.interactable = true;
            }
        }
        
        private async Task OnWelcome()
        {
            SetStatus("Registering...");
            protocol.Handshake.OnWelcome -= OnWelcome;
            await protocol.DefaultRegisterSender(new ExampleRegister() {
                Username = username.text,
                Password = password.text,
                PasswordConfirm = passwordConfirm.text,
                // Uncomment the following line if your register makes use of DisplayName.
                /* DisplayName = displayName.text */
            });
        }
        
        private async Task OnTimeout()
        {
            SetStatus("Handshake timeout!");            
        }
        
        private void SetStatus(string value)
        {
            if (statusLabel) {
                protocol.RunInMainThread(() => {
                    statusLabel.text = value;
                });
            }
        }
    }
}
```

The idea is very similar to the Authentication UI, but there's no on-line/offline components switching, and there's also a `passwordConfirm` to set.

Similar to the Authenticator UI, this provides a full implementation for the username/password/passwordConfirm registering method.

# Final remarks

Again, take into account that these implementations can be completely changed once generated, as long as they're changed consistently, that more authentication / register methods can be added, and that, aside from what's described in this document, no more details are needed for the end user.
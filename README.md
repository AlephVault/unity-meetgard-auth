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

It will be located at `Assets/Scripts/Protocols/ExampleMySimpleAuthProtocolDefinition` with the contents:

```
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

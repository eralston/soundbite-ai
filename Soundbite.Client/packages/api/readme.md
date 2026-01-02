# Soundbite API
The @soundbite/api package contains models and services that allow developers to interact with the Soundbite Platform.

For more information on **Soundbite.AI** please visit our website at http://www.soundbite.ai

## HTTP Client
At Soundbite we understand that developers have strong opinions when it comes to which HTTP clients to use on a project. To ensure flexibilty, the @soundbite/api package contains an **IHttpAdapter** interface that allows developers to implement HTTP calls with the client of their choosing.  We also provide out of the box implementations for some common HTTP clients:

- Axios (see the @soundbite/api-axios package)
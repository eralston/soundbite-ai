This explains the approach to building a vendor agnostic LDAP provider sync system. This is used by Soundbite to periodically synchronize the groups and users of Soundbite customers with Azure Active Directory (AAD) and Okta.

As of this writing, the entire codebase for Directory Sync is in a single project alongisde the Azure Function that runs the process. In the future, this code should be split into one or more DLL projects that would enable easy implementation of new providers without disturbing structural code around interfaces and core database entities.

# Data Elements
In order to support enterprise-wide deployment and leverage current customer investments in the management of identity, Soundbite integrates with directory services to synchronize users and groups. Directory information includes the following data elements:

## Users
A limit set of information for each active user.
- Universal ID
- First Name
- Last Name
- Display Name
- Title
- Email
- Phone Number

## Groups
A limited set of information for each active group:
- Universal ID
- Name
- Description
- Group membership information

# Abstractions & Data Model
The system providers a high-level set of interfaces that provide a means of abstracting directory providers. This is accomplished via the following code:
- DirectoryUniversalId - A property in synchronized models (EG, organization in Masticore.Entity)
- IDirectory*.cs in the root folder - Declares the format for the engine that runs the sync process and a factory to make them.
- /DirectorySyncProviders/Config - Configuration models for the running the sync process
- /Models/* - Database models that persist the results of sync runs, enabling them to be displayed in the front-end.

# Sync Process
The Soundbite.AzFunc.DirectorySync contains several Azure Functions that each accomplish a different area of the sync process. They include:
- SyncAllOrgsTimerStart - The top-level function that runs every night (UTC Time). It's a durable function that spins up the SyncAllOrgs function.
- SyncAllOrgs - Pulls the list of organizations to sync from the database via GetOrgsToSync and loads up list of them to run the SyncOrg process over.
- GetOrgsToSync - Queries the database using the DirectorySyncService class, returning the list of all orgs that will sync in this run.
- SyncOrg - Which instatiates the appropriate sync provider for the given configuration and does the work to access the directory via credentials in the sync configuration

# Getting Started
In order to start developing with the project, one must set up the following. This assumes one wants to link up their local application to the Soundbite AAD instance - the real one for the whole organization.

## Local.Settings.Json
The following is the contents of a default Local.Settings.Json file, which is NOT stored in ADO and must be created locally at the root of the project by each developer.

```JavaScript
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  },
  "ConnectionStrings": {
    "SbDb": "Server=(localdb)\\MSSQLLocalDB; Database=Soundbite; Trusted_Connection=True;"
  }
}
```

## Directory Sync Configuration
To setup a new entry for the AD sync in the database, one must add a configuration JSON object to the relevant organization that needs syncing.

Here is an example of a basic, full sync mode JSON:

```SQL
update Organizations set
DirectorySyncProviderType='AAD',
DirectorySyncConfigJson='{
    "TenantId": "[AMS TENANT ID]",
    "AppId": "[CLIENT ID]", // If left null, it will fallback to environment variables (Project Debug settings)
    "SecretKey": <SecretKeyHere> // If left null, it will fallback to environment variables + Key vault in Azure
    "ImportAllGroups": true,
    "ImportAllUsers": true,
    "Groups": null,
    "Users": null,
    "DeltaLinkUsers": null,
    "DeltaLinkGroups": null,
    "NextLinkGroups": null
}' where id = <Target Org ID>
```
Where id = the Org ID.

To run the process over again when testing, one must remember to reset the delta values in the config to ensure it runs from scratch rather than incrementally.

## AAD Sync Diagram
The following architecture diagram illustrates the relationship between customer AD tenants and the Soundbite AD sync agent:

![AD Sync Architecture.png](/.attachments/AD%20Sync%20Architecture-3cd311f2-7ce4-45e4-b9a9-5ccfc6165413.png)

## Credentials
When running the sync, the AAD implementation relies upon an AppId + Client Secret combination to connect to the customer's AD instance.
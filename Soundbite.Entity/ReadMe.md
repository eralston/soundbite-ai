# Soundbite.Entity
Library for Entity Framework schema for Soundbite

## Depends On
Masticore.Entity provides the foundation for the SbDb context

## Getting Started
To get started with the project, you can open the "Package Manager Console" and run the following:

```
update-database
```

Then run the app and sign in, making sure you get your user record as #1 in the database.

Then, copy-paste the "Mock-1.sql" script into a script editor for the database, then run the script to populate the mock data for yourself.

If you're successful, then you can reload the app and you'll see new test organizations.

## Migrations

### Add and Apply Migrations
To generate new migrations:

```
add-migration [Identifier]
```
Then run it:
```
update-database
```

And roll it back with...
```
update-database -migration [target]
```
Where target = 0 for deleting all schema

### Reset and Generate Latest
When preparing for a release, it's helpful to generate a consolidated migration with all work thus far. This is done by take a project with updated models, then rolling back migrations until you're at the same version as the live database, then generating a single migration.

First, remove the previous migration with...
```
remove-migration
```

Then generate a new migration by running `add-migration` for the project, applying with an `update-database` per above. This will make a single migration out of the current state and ensure your releases follow a single step-by-step path rather than one script per change each developer made.

Consumers of the project must either reset their databases entirely and fast forward with the new change OR down migrate using old code to get back to the pre-release database, then run the consolidated script to fast forward to the latest schema. In either case, the environment will likely lose data along the way.


### Script Generation for Manual Deployments
If you're doing a manual deployment to Azure, you must generate a SQL file to be run by hand by connecting to the Azure database.

To make a migration for deployment, you can create a SQL file as follows:

```
Script-Migration [Source Migration] [Target Migration]
```


### Project Context
For context, these must be run with the Soundbite.Entity project providing the structure and the Soundbite.Api structure providing connectivity (IE, append `add-migration [ID] -StartupProject Soundbite.Api -Project Soundbite.Entity` to the command). 

For instance:

```
add-migration [ID] -StartupProject Soundbite.Api -Project Soundbite.Entity

update-database -StartupProject Soundbite.Api -Project Soundbite.Entity

# Reset
update-database -StartupProject Soundbite.Api -Project Soundbite.Entity -migration 0

remove-migration -StartupProject Soundbite.Api -Project Soundbite.Entity

script-migration [FROM ID] [TO ID] -StartupProject Soundbite.Api -Project Soundbite.Entity
```
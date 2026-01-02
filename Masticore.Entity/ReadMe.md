# Masticore.Entity
A utility library for working with Entity Framework Core and a starting point for user & organization management.

## Models
To consume the models in this package, you need to make your own DbContext class that utilizes the classes in Masticore.Entity.Models.

## Migrations
To manage your migrations, you will likely want to follow this lifecycle. 

These instructions use the "package manager console", which is the original way of managing migrations. One can find equivalents between [Visual Studio and dotnet CLI here](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=vs).

### Creating Migration
To create a new layer of scripts to capture current changes, run the following:
```
add-migration [name]
```
Where [name] is a new identifier for the migration

### Migrate Up
To apply all current migrations, run the following:
```
update-database
```

### Migrate Down
To rollback changes, run the following:
```
update-database -migration [target]
```
Where [target] is the name of the desired previous migration

### Delete Last Migration
To delete the last migration script, run the following:
```
remove-migration
```

### Generate Script
To generate a new script for the current database, assuming you're starting with an empty database:
```
Script-Migration
```

To generate script that is the diff between a given migration and the latest, use the following
```
Script-Migration [migration name]
```
Where [migration name] is the start point for the migration

### Squash Migrations
To "squash" migrations, you can use the following sequence of commands:
```
update-database -migration [start migration]
```
Where [start migration] is the migration prior to your set to squash. If something terrible happens, then you may need to blow away your whole database using [this script](https://gist.github.com/eralston/11264520).

For each migration you wish to remove, run the following:
```
remove-migration
```
Then add a new migration with the new delta:
```
add-migration [new name]
```
Where [new name] is the new migration name

Then you can migrate up and generate script normally to apply the migration.

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
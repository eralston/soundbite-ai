ALTER TABLE [Clips] ADD [MediaOperationsJson] nvarchar(max) NULL;

GO

CREATE TABLE [ClipOperations] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(21) NOT NULL,
    [OperationType] int NOT NULL,
    [OperationState] int NOT NULL,
    [OperationDataJson] ntext NULL,
    [ErrorDetails] nvarchar(max) NULL,
    [CompletedDate] datetime2 NULL,
    [BillingCode] nvarchar(max) NULL,
    [ExternalId] nvarchar(max) NULL,
    [ClipId] int NOT NULL,
    CONSTRAINT [PK_ClipOperations] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_ClipOperations_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_ClipOperations_Clips_ClipId] FOREIGN KEY ([ClipId]) REFERENCES [Clips] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClipOperations_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_ClipOperations_ClipId] ON [ClipOperations] ([ClipId]);

GO

CREATE INDEX [IX_ClipOperations_CreatedById] ON [ClipOperations] ([CreatedById]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230519153922_15', N'3.1.19');

GO


ALTER TABLE [Participants] ADD [IsDirectParticipant] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

CREATE TABLE [ParticipantGroupMembers] (
    [ParticipantId] int NOT NULL,
    [ParticipantGroupId] int NOT NULL,
    CONSTRAINT [PK_ParticipantGroupMembers] PRIMARY KEY ([ParticipantId], [ParticipantGroupId]),
    CONSTRAINT [FK_ParticipantGroupMembers_ParticipantGroups_ParticipantGroupId] FOREIGN KEY ([ParticipantGroupId]) REFERENCES [ParticipantGroups] ([Id]),
    CONSTRAINT [FK_ParticipantGroupMembers_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id])
);

GO

CREATE INDEX [IX_ParticipantGroupMembers_ParticipantGroupId] ON [ParticipantGroupMembers] ([ParticipantGroupId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221012172956_11', N'3.1.19');

GO


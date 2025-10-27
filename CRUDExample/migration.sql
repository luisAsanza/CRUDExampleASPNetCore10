IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Countries] (
    [CountryId] uniqueidentifier NOT NULL,
    [CountryName] nvarchar(max) NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryId])
);

CREATE TABLE [Persons] (
    [PersonId] uniqueidentifier NOT NULL,
    [PersonName] nvarchar(40) NULL,
    [Email] nvarchar(40) NULL,
    [DateOfBirth] date NULL,
    [Gender] nvarchar(10) NULL,
    [CountryId] uniqueidentifier NULL,
    [Address] nvarchar(200) NULL,
    [ReceiveNewsLetters] bit NOT NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY ([PersonId])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryId', N'CountryName') AND [object_id] = OBJECT_ID(N'[Countries]'))
    SET IDENTITY_INSERT [Countries] ON;
INSERT INTO [Countries] ([CountryId], [CountryName])
VALUES ('08852737-942a-48e2-8622-6654af313a09', N'China'),
('3c16db02-611f-40cd-ba34-43cc493ac676', N'Chile'),
('ad914740-7d42-4374-83a0-6b939e2f1f05', N'Brazil'),
('ef684ba0-0ad5-41d1-8223-4122c149f9da', N'Argentina'),
('f6564641-ad62-41d0-9778-f289d6d434be', N'Canada');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryId', N'CountryName') AND [object_id] = OBJECT_ID(N'[Countries]'))
    SET IDENTITY_INSERT [Countries] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PersonId', N'Address', N'CountryId', N'DateOfBirth', N'Email', N'Gender', N'PersonName', N'ReceiveNewsLetters') AND [object_id] = OBJECT_ID(N'[Persons]'))
    SET IDENTITY_INSERT [Persons] ON;
INSERT INTO [Persons] ([PersonId], [Address], [CountryId], [DateOfBirth], [Email], [Gender], [PersonName], [ReceiveNewsLetters])
VALUES ('210780c5-945e-4a1c-9287-f5dee8e67e53', N'688 Main St, Cityville', '5ca1618f-97c6-46b0-8a72-6c312a339954', '2005-08-29', N'carlos.rivera@example.com', N'Female', N'Carlos Rivera', CAST(1 AS bit)),
('9403db58-adc5-494b-a455-b075aa7cfc46', N'913 Main St, Cityville', '08852737-942a-48e2-8622-6654af313a09', '1962-11-25', N'alice.johnson@example.com', N'Other', N'Alice Johnson', CAST(1 AS bit)),
('cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea', N'332 Main St, Cityville', '9a1f995b-fbab-4612-9e9c-358048815dba', '2004-03-05', N'bob.smith@example.com', N'Male', N'Bob Smith', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PersonId', N'Address', N'CountryId', N'DateOfBirth', N'Email', N'Gender', N'PersonName', N'ReceiveNewsLetters') AND [object_id] = OBJECT_ID(N'[Persons]'))
    SET IDENTITY_INSERT [Persons] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251004040756_Initial', N'9.0.9');


            CREATE PROCEDURE [dbo].[GetAllPersons]
            AS BEGIN
                SELECT PersonId, PersonName, Email, DateOfBirth, Gender, CountryId, Address, ReceiveNewsLetters
                FROM [dbo].[Persons]
            END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251005013328_GetPersons_StoredProcedure', N'9.0.9');

ALTER TABLE [Persons] ADD [TIN] nvarchar(max) NULL;

UPDATE [Persons] SET [TIN] = NULL
WHERE [PersonId] = '210780c5-945e-4a1c-9287-f5dee8e67e53';
SELECT @@ROWCOUNT;


UPDATE [Persons] SET [TIN] = NULL
WHERE [PersonId] = '9403db58-adc5-494b-a455-b075aa7cfc46';
SELECT @@ROWCOUNT;


UPDATE [Persons] SET [TIN] = NULL
WHERE [PersonId] = 'cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251005020943_TINColumn', N'9.0.9');

EXEC sp_rename N'[Persons].[TIN]', N'TaxIdentificationNumber', 'COLUMN';

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'TaxIdentificationNumber');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Persons] ALTER COLUMN [TaxIdentificationNumber] varchar(8) NULL;
ALTER TABLE [Persons] ADD DEFAULT 'ABC12345' FOR [TaxIdentificationNumber];

UPDATE [Persons] SET [TaxIdentificationNumber] = 'ABC12345'
WHERE [PersonId] = '210780c5-945e-4a1c-9287-f5dee8e67e53';
SELECT @@ROWCOUNT;


UPDATE [Persons] SET [TaxIdentificationNumber] = 'ABC12345'
WHERE [PersonId] = '9403db58-adc5-494b-a455-b075aa7cfc46';
SELECT @@ROWCOUNT;


UPDATE [Persons] SET [TaxIdentificationNumber] = 'ABC12345'
WHERE [PersonId] = 'cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251005132819_TIN_Updated', N'9.0.9');

ALTER TABLE [Persons] ADD CONSTRAINT [CHK_TIN] CHECK (len([TaxIdentificationNumber]) = 8);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251005183653_TIN_Updated_CHK', N'9.0.9');

ALTER TABLE [Persons] DROP CONSTRAINT [CHK_TIN];

ALTER TABLE [Persons] ADD CONSTRAINT [CHK_TIN] CHECK (len([TaxIdentificationNumber]) = 8);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251005184117_TIN_Updated_CHK_Fix', N'9.0.9');

COMMIT;
GO


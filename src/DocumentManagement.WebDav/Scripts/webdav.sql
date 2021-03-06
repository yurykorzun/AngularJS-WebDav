USE [TestDB]
GO

Drop table LockToken;
Drop table Lock;
Drop table [File];
Drop table Folder;


CREATE TABLE [dbo].[Folder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentFolderId] [int] NULL,
	[FolderName] [varchar](255) NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[UpdatedUser] [varchar](40) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedUser] [varchar](40) NOT NULL,
	[VersionSeq] [int] NOT NULL,
 CONSTRAINT [PK_Folders] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Folder]  WITH CHECK ADD  CONSTRAINT [FK__DBO.Folder_ParentFolderId_REFS__DBO.Folder_Id] FOREIGN KEY([ParentFolderId])
REFERENCES [Folder] ([Id])

GO

CREATE TABLE [dbo].[File](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentFolderId] [int] NOT NULL,
	[FileName] [varchar](255) NULL,
	[ContentType] [varchar](255) NULL,
	[FileData] [image] NULL,
	[FileDataSize] [bigint] NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[UpdatedUser] [varchar](40) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedUser] [varchar](40) NOT NULL,
	[VersionSeq] [int] NOT NULL,
 CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[File]  WITH CHECK ADD  CONSTRAINT [FK__DBO.File_ParentFolderId_REFS__DBO.Folder_Id] FOREIGN KEY([ParentFolderId])
REFERENCES [Folder] ([Id])

GO



CREATE TABLE [dbo].[Lock](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileId] [int] NOT NULL,
	[LockType] [int] NOT NULL,
	[ResType] [int] NOT NULL,
	[LockScope] [int] NOT NULL,
	[LockDepth] [int] NOT NULL,
	[LockOwner] [varchar](255) NOT NULL,
	[LockOwnerType] [int] NOT NULL,
	[Timeout] [int] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[UpdatedUser] [varchar](40) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedUser] [varchar](40) NOT NULL,
	[VersionSeq] [int] NOT NULL,
 CONSTRAINT [PK_Locks] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Lock]  WITH CHECK ADD  CONSTRAINT [FK__DBO.Lock_FileId_REFS__DBO.File_Id] FOREIGN KEY([FileId])
REFERENCES [File] ([Id])

GO

CREATE TABLE [dbo].[LockToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LockID] [int] NOT NULL,
	[Token] [varchar](255) NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[UpdatedUser] [varchar](40) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedUser] [varchar](40) NOT NULL,
	[VersionSeq] [int] NOT NULL,
 CONSTRAINT [PK_Locks_Tokens] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET IDENTITY_INSERT [dbo].[Folder] ON 

INSERT [dbo].[Folder] ([Id], [ParentFolderId], [FolderName], [UpdatedDate], [UpdatedUser], [CreatedDate], [CreatedUser], [VersionSeq]) VALUES (1, null, N'root', CAST(0x0000A43200000000 AS DateTime), N'PBHC\Yury.Korzun', CAST(0x0000A43200000000 AS DateTime), N'PBHC\Yury.Korzun', 1)
INSERT [dbo].[Folder] ([Id], [ParentFolderId], [FolderName], [UpdatedDate], [UpdatedUser], [CreatedDate], [CreatedUser], [VersionSeq]) VALUES (3, 1, N'test', CAST(0x0000A43A00F9E047 AS DateTime), N'PBHC\Yury.Korzun', CAST(0x0000A43A00F9DF7A AS DateTime), N'PBHC\Yury.Korzun', 0)
SET IDENTITY_INSERT [dbo].[Folder] OFF

Insert into [File] (ParentFolderId, FileName, ContentType, FileData, FileDataSize, UpdatedDate, UpdatedUser, CreatedDate, CreatedUser, VersionSeq)
SELECT 1, 'test.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', BulkColumn, 15919, getDate(), 'PBHC\Yury.Korzun', getDate(), 'PBHC\Yury.Korzun', 0  FROM OPENROWSET(BULK N'C:\temp\test.docx', SINGLE_BLOB) rs

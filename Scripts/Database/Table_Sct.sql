USE [OnSteroids]
GO
/****** Object:  Table [dbo].[ApiLogs]    Script Date: 10/26/2026 7:57:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApiLogs](
	[Id] [uniqueidentifier] NOT NULL,
	[TraceId] [nvarchar](100) NULL,
	[Method] [nvarchar](20) NULL,
	[Path] [nvarchar](255) NULL,
	[RequestBody] [nvarchar](max) NULL,
	[StatusCode] [int] NULL,
	[ResponseBody] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Capsule]    Script Date: 10/26/2026 7:57:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Capsule](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[AutoAuthEnabled] [nvarchar](50) NULL,
	[AutoAuthEndpointId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Capsule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Request]    Script Date: 10/26/2026 7:57:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Request](
	[Id] [uniqueidentifier] NOT NULL,
	[CapsuleId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Url] [nvarchar](2048) NOT NULL,
	[Method] [nvarchar](10) NOT NULL,
	[PayloadType] [nvarchar](20) NOT NULL,
	[BodyType] [nvarchar](20) NOT NULL,
	[RawType] [nvarchar](20) NOT NULL,
	[RawBody] [nvarchar](max) NULL,
	[RawBodyJson] [nvarchar](max) NULL,
	[RawBodyXml] [nvarchar](max) NULL,
	[AuthType] [nvarchar](10) NOT NULL,
	[AuthToken] [nvarchar](max) NULL,
	[PreRequestScript] [nvarchar](max) NULL,
	[PostResponseScript] [nvarchar](max) NULL,
	[EncryptionAlgorithm] [nvarchar](20) NOT NULL,
	[EncryptionKey] [nvarchar](max) NULL,
	[AutoEncryptBody] [bit] NOT NULL,
	[AutoEncryptHeaders] [bit] NOT NULL,
	[EncryptedHeaders] [nvarchar](max) NULL,
	[EncryptedBodyPaths] [nvarchar](max) NULL,
	[EncryptionScript] [nvarchar](max) NULL,
	[FollowRedirects] [bit] NOT NULL,
	[VerifySsl] [bit] NOT NULL,
	[EnableCookies] [bit] NOT NULL,
	[BypassCors] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[TestScript] [nvarchar](max) NULL,
	[TestScriptEnabled] [bit] NOT NULL,
	[PostTriggerTabId] [nvarchar](100) NULL,
	[AutoAuthEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_Request] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestFormData]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestFormData](
	[Id] [uniqueidentifier] NOT NULL,
	[RequestId] [uniqueidentifier] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[FieldKey] [nvarchar](500) NOT NULL,
	[FieldValue] [nvarchar](max) NOT NULL,
	[FieldType] [nvarchar](10) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_RequestFormData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestHeader]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestHeader](
	[Id] [uniqueidentifier] NOT NULL,
	[RequestId] [uniqueidentifier] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[HeaderKey] [nvarchar](500) NOT NULL,
	[HeaderValue] [nvarchar](max) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_RequestHeader] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestHistory]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestHistory](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Method] [nvarchar](10) NOT NULL,
	[Url] [nvarchar](2048) NOT NULL,
	[RequestSnapshot] [nvarchar](max) NULL,
	[ResponseStatus] [int] NULL,
	[ResponseTime] [int] NULL,
	[ResponseSize] [bigint] NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_RequestHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestParam]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestParam](
	[Id] [uniqueidentifier] NOT NULL,
	[RequestId] [uniqueidentifier] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[ParamKey] [nvarchar](500) NOT NULL,
	[ParamValue] [nvarchar](max) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_RequestParam] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Script]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Script](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[OriginalContent] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAuth]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAuth](
	[Id] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](320) NOT NULL,
	[Otp] [nvarchar](10) NULL,
	[IsAuthenticated] [bit] NOT NULL,
	[OtpExpiresAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[RefreshToken] [nvarchar](256) NULL,
	[RefreshTokenExpiryTime] [datetime2](7) NULL,
	[PasswordHash] [nvarchar](512) NULL,
	[IsOtpVerified] [bit] NOT NULL,
 CONSTRAINT [PK_UserAuth] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_UserAuth_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserWorkspaceState]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserWorkspaceState](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ActiveCapsuleId] [nvarchar](100) NULL,
	[ActiveCapsuleName] [nvarchar](255) NULL,
	[ActiveTabId] [nvarchar](100) NULL,
	[OpenTabIds] [nvarchar](max) NULL,
	[TabStates] [nvarchar](max) NULL,
	[Responses] [nvarchar](max) NULL,
	[AutoAuthEnabled] [nvarchar](50) NULL,
	[AutoAuthEndpointId] [nvarchar](100) NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[AutoSaveEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_UserWorkspaceState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_UserWorkspaceState_UserId] UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Variable]    Script Date: 10/26/2026 7:57:59 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Variable](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[VariableKey] [nvarchar](255) NOT NULL,
	[VariableValue] [nvarchar](max) NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[CapsuleId] [uniqueidentifier] NULL,
	[Type] [nvarchar](50) NULL,
 CONSTRAINT [PK_Variable] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ApiLogs] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[ApiLogs] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Capsule] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[Capsule] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Capsule] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Capsule] ADD  DEFAULT ('off') FOR [AutoAuthEnabled]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'New Request') FOR [Name]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'') FOR [Url]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'GET') FOR [Method]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'params') FOR [PayloadType]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'none') FOR [BodyType]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'JSON') FOR [RawType]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'none') FOR [AuthType]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (N'none') FOR [EncryptionAlgorithm]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((0)) FOR [AutoEncryptBody]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((0)) FOR [AutoEncryptHeaders]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((1)) FOR [FollowRedirects]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((1)) FOR [VerifySsl]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((1)) FOR [EnableCookies]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((1)) FOR [BypassCors]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((0)) FOR [TestScriptEnabled]
GO
ALTER TABLE [dbo].[Request] ADD  DEFAULT ((0)) FOR [AutoAuthEnabled]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT (N'') FOR [FieldKey]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT (N'') FOR [FieldValue]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT (N'text') FOR [FieldType]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RequestFormData] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT (N'') FOR [HeaderKey]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT (N'') FOR [HeaderValue]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RequestHeader] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[RequestHistory] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[RequestHistory] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RequestHistory] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT (N'') FOR [ParamKey]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT (N'') FOR [ParamValue]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RequestParam] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Script] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Script] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[Script] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Script] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[UserAuth] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[UserAuth] ADD  DEFAULT ((0)) FOR [IsAuthenticated]
GO
ALTER TABLE [dbo].[UserAuth] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserAuth] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[UserAuth] ADD  CONSTRAINT [DF_UserAuth_IsOtpVerified]  DEFAULT ((0)) FOR [IsOtpVerified]
GO
ALTER TABLE [dbo].[UserWorkspaceState] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[UserWorkspaceState] ADD  DEFAULT ('off') FOR [AutoAuthEnabled]
GO
ALTER TABLE [dbo].[UserWorkspaceState] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[UserWorkspaceState] ADD  DEFAULT ((0)) FOR [AutoSaveEnabled]
GO
ALTER TABLE [dbo].[Variable] ADD  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[Variable] ADD  DEFAULT (N'') FOR [VariableValue]
GO
ALTER TABLE [dbo].[Variable] ADD  DEFAULT ((1)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[Variable] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Variable] ADD  DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Variable] ADD  DEFAULT ('global') FOR [Type]
GO
ALTER TABLE [dbo].[Capsule]  WITH CHECK ADD  CONSTRAINT [FK_Capsule_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserAuth] ([Id])
GO
ALTER TABLE [dbo].[Capsule] CHECK CONSTRAINT [FK_Capsule_User]
GO
ALTER TABLE [dbo].[Request]  WITH CHECK ADD  CONSTRAINT [FK_Request_Capsule] FOREIGN KEY([CapsuleId])
REFERENCES [dbo].[Capsule] ([Id])
GO
ALTER TABLE [dbo].[Request] CHECK CONSTRAINT [FK_Request_Capsule]
GO
ALTER TABLE [dbo].[Request]  WITH CHECK ADD  CONSTRAINT [FK_Request_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserAuth] ([Id])
GO
ALTER TABLE [dbo].[Request] CHECK CONSTRAINT [FK_Request_User]
GO
ALTER TABLE [dbo].[RequestFormData]  WITH CHECK ADD  CONSTRAINT [FK_RequestFormData_Request] FOREIGN KEY([RequestId])
REFERENCES [dbo].[Request] ([Id])
GO
ALTER TABLE [dbo].[RequestFormData] CHECK CONSTRAINT [FK_RequestFormData_Request]
GO
ALTER TABLE [dbo].[RequestHeader]  WITH CHECK ADD  CONSTRAINT [FK_RequestHeader_Request] FOREIGN KEY([RequestId])
REFERENCES [dbo].[Request] ([Id])
GO
ALTER TABLE [dbo].[RequestHeader] CHECK CONSTRAINT [FK_RequestHeader_Request]
GO
ALTER TABLE [dbo].[RequestHistory]  WITH CHECK ADD  CONSTRAINT [FK_RequestHistory_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserAuth] ([Id])
GO
ALTER TABLE [dbo].[RequestHistory] CHECK CONSTRAINT [FK_RequestHistory_User]
GO
ALTER TABLE [dbo].[RequestParam]  WITH CHECK ADD  CONSTRAINT [FK_RequestParam_Request] FOREIGN KEY([RequestId])
REFERENCES [dbo].[Request] ([Id])
GO
ALTER TABLE [dbo].[RequestParam] CHECK CONSTRAINT [FK_RequestParam_Request]
GO
ALTER TABLE [dbo].[Script]  WITH CHECK ADD  CONSTRAINT [FK_Script_UserAuth] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserAuth] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Script] CHECK CONSTRAINT [FK_Script_UserAuth]
GO
ALTER TABLE [dbo].[Variable]  WITH CHECK ADD  CONSTRAINT [FK_Variable_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserAuth] ([Id])
GO
ALTER TABLE [dbo].[Variable] CHECK CONSTRAINT [FK_Variable_User]
GO

/****** Object:  Table [dbo].[Bracket]    Script Date: 5/9/2025 8:01:09 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Bracket](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[UserId] [int] NOT NULL,
	[MinUsers] [smallint] NOT NULL,
	[MaxUsers] [smallint] NOT NULL,
	[Private] [bit] NOT NULL,
	[Cutoff] [datetimeoffset](7) NOT NULL,
	[Created] [datetimeoffset](7) NOT NULL,
	[LastModified] [datetimeoffset](7) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CurrentRound] [int] NOT NULL,
 CONSTRAINT [PK_Bracket] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Bracket] ADD  CONSTRAINT [DF_Bracket_MinUsers]  DEFAULT ((2)) FOR [MinUsers]
GO

ALTER TABLE [dbo].[Bracket] ADD  CONSTRAINT [DF_Bracket_MaxUsers]  DEFAULT ((-1)) FOR [MaxUsers]
GO

ALTER TABLE [dbo].[Bracket] ADD  CONSTRAINT [DF_Bracket_Created]  DEFAULT (sysdatetimeoffset()) FOR [Created]
GO

ALTER TABLE [dbo].[Bracket] ADD  CONSTRAINT [DF_Bracket_LastModified]  DEFAULT (sysdatetimeoffset()) FOR [LastModified]
GO

ALTER TABLE [dbo].[Bracket] ADD  CONSTRAINT [DF_Bracket_Status]  DEFAULT ((1)) FOR [Status]
GO

ALTER TABLE [dbo].[Bracket] ADD  CONSTRAINT [DF_Bracket_CurrentRound]  DEFAULT ((0)) FOR [CurrentRound]
GO

/****** Object:  Table [dbo].[BracketMatch]    Script Date: 5/9/2025 8:01:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BracketMatch](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Round] [int] NOT NULL,
	[GroupKey] [int] NOT NULL,
	[MatchIndex] [int] NOT NULL,
	[BracketId] [bigint] NOT NULL,
	[ChaosMonkey] [bigint] NULL,
	[Opponent1ParentMatchId] [bigint] NULL,
	[Opponent1Id] [bigint] NULL,
	[Opponent1Score] [float] NULL,
	[Opponent2ParentMatchId] [bigint] NULL,
	[Opponent2Id] [bigint] NULL,
	[Opponent2Score] [float] NULL,
	[Opponent3ParentMatchId] [bigint] NULL,
	[Opponent3Id] [bigint] NULL,
	[Opponent3Score] [float] NULL,
	[Created] [datetimeoffset](7) NOT NULL,
	[Complete] [datetimeoffset](7) NULL,
	[Cutoff] [datetimeoffset](7) NULL,
	[Winner] [bigint] NULL,
 CONSTRAINT [PK_BracketMatch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BracketMatch] ADD  CONSTRAINT [DF_BracketMatch_GroupKey]  DEFAULT ((0)) FOR [GroupKey]
GO

ALTER TABLE [dbo].[BracketMatch] ADD  CONSTRAINT [DF_BracketMatch_Created]  DEFAULT (sysdatetimeoffset()) FOR [Created]
GO

ALTER TABLE [dbo].[BracketMatch]  WITH CHECK ADD  CONSTRAINT [FK_BracketMatch_Bracket] FOREIGN KEY([BracketId])
REFERENCES [dbo].[Bracket] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BracketMatch] CHECK CONSTRAINT [FK_BracketMatch_Bracket]
GO

/****** Object:  Table [dbo].[BracketMatchVote]    Script Date: 5/9/2025 8:01:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BracketMatchVote](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BracketId] [bigint] NOT NULL,
	[MatchId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[OpponentId] [int] NOT NULL,
	[Created] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_BracketMatchVote] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BracketMatchVote] ADD  CONSTRAINT [DF_BracketMatchVote_Created]  DEFAULT (sysdatetimeoffset()) FOR [Created]
GO

ALTER TABLE [dbo].[BracketMatchVote]  WITH CHECK ADD  CONSTRAINT [FK_BracketMatchVote_BracketMatch] FOREIGN KEY([MatchId])
REFERENCES [dbo].[BracketMatch] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BracketMatchVote] CHECK CONSTRAINT [FK_BracketMatchVote_BracketMatch]
GO

/****** Object:  Table [dbo].[BracketOpponents]    Script Date: 5/9/2025 8:02:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BracketOpponents](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BracketId] [bigint] NOT NULL,
	[Name] [nvarchar](1024) NOT NULL,
	[Rank] [smallint] NOT NULL,
	[Url] [varchar](1024) NULL,
 CONSTRAINT [PK_BracketOpponents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BracketOpponents]  WITH CHECK ADD  CONSTRAINT [FK_BracketOpponents_Bracket] FOREIGN KEY([BracketId])
REFERENCES [dbo].[Bracket] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BracketOpponents] CHECK CONSTRAINT [FK_BracketOpponents_Bracket]
GO

/****** Object:  Table [dbo].[BracketParticipant]    Script Date: 5/9/2025 8:02:13 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BracketParticipant](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[BracketId] [bigint] NOT NULL,
	[Created] [datetimeoffset](7) NOT NULL,
	[CurrentRound] [int] NOT NULL,
	[CurrentGroupKey] [int] NOT NULL,
	[Evicted] [datetimeoffset](7) NULL,
	[FinalRank] [int] NULL,
 CONSTRAINT [PK_BracketParticipant] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BracketParticipant] ADD  CONSTRAINT [DF_BracketParticipant_Created]  DEFAULT (sysdatetimeoffset()) FOR [Created]
GO

ALTER TABLE [dbo].[BracketParticipant] ADD  CONSTRAINT [DF_BracketParticipant_RoundCompleted]  DEFAULT ((0)) FOR [CurrentRound]
GO

ALTER TABLE [dbo].[BracketParticipant] ADD  CONSTRAINT [DF_BracketParticipant_CurrentGroupKey]  DEFAULT ((0)) FOR [CurrentGroupKey]
GO

ALTER TABLE [dbo].[BracketParticipant]  WITH CHECK ADD  CONSTRAINT [FK_BracketParticipant_Bracket] FOREIGN KEY([BracketId])
REFERENCES [dbo].[Bracket] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BracketParticipant] CHECK CONSTRAINT [FK_BracketParticipant_Bracket]
GO


/****** Object:  Table [dbo].[BracketUser]    Script Date: 5/9/2025 4:19:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BracketUser](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[user_name] [nvarchar](256) NOT NULL,
	[user_email] [nvarchar](512) NOT NULL,
	[user_password] [varchar](1024) NOT NULL,
	[user_created] [datetimeoffset](7) NULL,
	[user_IsAdmin] [bit] NULL,
 CONSTRAINT [PK_BracketUser] PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BracketUser] ADD  CONSTRAINT [DF_BracketUser_user_created]  DEFAULT (sysdatetimeoffset()) FOR [user_created]
GO

ALTER TABLE [dbo].[BracketUser] ADD  CONSTRAINT [DF_BracketUser_user_IsAdmin]  DEFAULT ((0)) FOR [user_IsAdmin]
GO


/****** Object:  StoredProcedure [dbo].[usp_CompleteBracket]    Script Date: 5/9/2025 4:19:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Description: 
--	  Accepts @bracketId as input.
--    Updates each BracketParticipant.FinalRank based on correct votes, weighted by (match.Round + 1).
--    Sets the Bracket.Status to 8 (completed) for the specified bracket.
-- =============================================
CREATE PROCEDURE [dbo].[usp_CompleteBracket]
(
    @bracketId bigint
)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

    -- Step 1: Calculate score per participant for correct votes
    ;WITH CorrectVotes AS (
        SELECT
            v.UserId,
            v.BracketId,
            SUM(m.Round + 1) AS Score
        FROM BracketMatchVote v
        JOIN BracketMatch m ON v.MatchId = m.Id
        WHERE v.BracketId = @bracketId
          AND v.OpponentId = m.Winner
          AND m.Winner IS NOT NULL
        GROUP BY v.UserId, v.BracketId
    )

    -- Step 2: Update participant FinalRank
    UPDATE bp
    SET bp.FinalRank = cv.Score
    FROM BracketParticipant bp
    JOIN CorrectVotes cv
        ON bp.UserId = cv.UserId
       AND bp.BracketId = cv.BracketId
    WHERE bp.BracketId = @bracketId;

    -- Step 3: Optionally set FinalRank = 0 for those without any correct votes
    UPDATE bp
    SET bp.FinalRank = 0
    WHERE bp.BracketId = @bracketId
      AND bp.FinalRank IS NULL;

    -- Step 4: Update bracket status to 8 (complete)
    UPDATE Bracket
    SET Status = 8
    WHERE Id = @bracketId;
END
GO

/****** Object:  StoredProcedure [dbo].[usp_UpdateVoteCounts]    Script Date: 5/9/2025 4:20:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- This gets called after a participant votes for matches.
CREATE PROCEDURE [dbo].[usp_UpdateVoteCounts]
(
    @bracketId bigint
)
AS
BEGIN
    SET NOCOUNT ON

	-- add chaos monkey to bracket matches if odd number of participants.

	DECLARE @UseChaosMonkey int = 0
	SELECT @UseChaosMonkey = (CASE WHEN COUNT(*) % 2 = 0 THEN 1 ELSE 0 END) FROM BracketParticipant [user] WHERE [user].BracketId=@bracketId AND [user].Evicted IS NULL
	IF (@UseChaosMonkey = 1) 
	BEGIN
		UPDATE BracketMatch 
			SET ChaosMonkey=(SELECT id FROM BracketParticipant [user] WHERE [user].BracketId=@bracketId AND [user].Evicted IS NULL ORDER BY NEWID() OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) 
			FROM BracketMatch WHERE BracketMatch.BracketId=@bracketId AND ChaosMonkey IS NULL

	END

	--DECLARE @matches int = 

	-- update the scores from the votes
    UPDATE BracketMatch 
		SET Opponent1Score=(SELECT COUNT(*) FROM BracketMatchVote [vote] WHERE [vote].BracketId=[match].[bracketId] AND [match].Id=[vote].MatchId AND [vote].OpponentId=[match].Opponent1Id) 
				+ (CASE WHEN [chaos].OpponentId=[match].Opponent1Id THEN 1 ELSE 0 END),
			Opponent2Score=(SELECT COUNT(*) FROM BracketMatchVote [vote] WHERE [vote].BracketId=[match].[bracketId] AND [match].Id=[vote].MatchId AND [vote].OpponentId=[match].Opponent2Id)
				+ (CASE WHEN [chaos].OpponentId=[match].Opponent2Id THEN 1 ELSE 0 END),
			Opponent3Score=(SELECT COUNT(*) FROM BracketMatchVote [vote] WHERE [vote].BracketId=[match].[bracketId] AND [match].Id=[vote].MatchId AND [vote].OpponentId=[match].Opponent3Id)
				+ (CASE WHEN [chaos].OpponentId=[match].Opponent3Id THEN 1 ELSE 0 END),

			Complete=CASE 
				WHEN -- total votes on match is greater than the total non-evicted participants (plus chaos monkey) - set the completed date
					(SELECT COUNT(*) FROM BracketMatchVote [vote] WHERE [vote].BracketId=[match].[bracketId] AND [match].Id=[vote].MatchId) 
					>= (SELECT COUNT(*)+@UseChaosMonkey FROM BracketParticipant [user] WHERE [user].BracketId=[match].[bracketId] AND [user].Evicted IS NULL) 
					THEN sysdatetimeoffset() 
				ELSE 
					NULL 
			END

		FROM BracketMatch [match] 
			LEFT OUTER JOIN BracketMatchVote [chaos] ON [chaos].UserId = [match].ChaosMonkey
			JOIN Bracket [bracket] ON [match].BracketId=[bracket].Id
		WHERE [bracket].Id=@bracketId AND [match].[Round]=[bracket].[CurrentRound];

	UPDATE BracketMatch SET Winner = 
		CASE 
			WHEN Opponent1Score = IIF(Opponent2Score>Opponent3Score, Opponent2Score, Opponent3Score) THEN NULL
			WHEN Opponent1Score > IIF(Opponent2Score>Opponent3Score, Opponent2Score, Opponent3Score) THEN Opponent1Id
			WHEN Opponent2Score > IIF(Opponent1Score>Opponent3Score, Opponent1Score, Opponent3Score) THEN Opponent2Id
			WHEN Opponent3Score > IIF(Opponent2Score>Opponent1Score, Opponent2Score, Opponent1Score) THEN Opponent3Id
			ELSE NULL
		END
		FROM BracketMatch [match] 
		JOIN Bracket [bracket] ON [match].BracketId=[bracket].Id AND [match].[Round]=[bracket].[CurrentRound]
		WHERE [bracket].Id=@bracketId AND [match].Complete IS NOT NULL AND [match].Winner IS NULL;
		
	DECLARE @completed int
	DECLARE @total int
	SELECT @total = count(*), @completed = SUM(CASE WHEN Complete IS NOT NULL THEN 1 ELSE 0 END) FROM BracketMatch [match] JOIN Bracket [bracket] ON [match].BracketId=[bracket].Id AND [match].[Round]=[bracket].[CurrentRound] WHERE BracketId=@bracketId

	SELECT CASE WHEN (@total = @completed) THEN 1 ELSE 0 END AS RoundComplete;
END
GO


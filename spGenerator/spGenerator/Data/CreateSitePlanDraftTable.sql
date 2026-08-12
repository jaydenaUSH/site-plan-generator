
CREATE TABLE SitePlanDraft(
Id INT IDENTITY(1,1) PRIMARY KEY,
SitePlanRequestId INT NOT NULL,
SiteOverview NVARCHAR(MAX),
RecommendedLayout NVARCHAR(MAX),
VolunteerFlow NVARCHAR(MAX),
SupplyFlow NVARCHAR(MAX),
Timeline NVARCHAR(MAX),
Risks NVARCHAR(MAX),
PMReviewChecklist NVARCHAR(MAX),
CreatedAt DATETIME DEFAULT GETUTCDATE(),
Reviewer VARCHAR(500),
AdditionalNotes NVARCHAR(MAX),
finalVersion INT,
FOREIGN KEY (finalVersion) REFERENCES SitePlanFinal(Id),
FOREIGN KEY (SitePlanRequestId) REFERENCES SitePlanRequest(Id)
)

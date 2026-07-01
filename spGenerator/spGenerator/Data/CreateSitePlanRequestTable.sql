CREATE TABLE SitePlanRequest(
Id INT IDENTITY(1,1) PRIMARY KEY,
VenueName NVARCHAR(500),
VenueAddress NVARCHAR(500),
VolunteerCount INT,
NumberofLines INT,
MealPackageGoal INT,
RoomSpaceNotes NVARCHAR(MAX),
LoadingNotes NVARCHAR(MAX),
AvaialableEquipment NVARCHAR(500), 
Deadline DATETIME,
SpecialConstraints NVARCHAR(MAX),
ProjectType NVARCHAR(MAX),
AdditionalNotes NVARCHAR(MAX),
RoomBlueprintFilePath NVARCHAR(500),
CreatedAtUtc DATETIME DEFAULT GETUTCDATE()
)
GO
-- Changes to SitePlanRequest Table
ALTER TABLE SitePlanRequest
DROP COLUMN VolunteerCount;
ALTER TABLE SitePlanRequest
DROP COLUMN MealPackageGoal;
ALTER TABLE SitePlanRequest
DROP COLUMN AvailableEquipment;
ALTER TABLE SitePlanRequest
ADD NumberofPalettes INT;
ALTER TABLE SitePlanRequest
ADD ClientName NVARCHAR(500);
ALTER TABLE SitePlanRequest
ADD TableSizes INT;
EXEC sp_rename 'SitePlanRequest.RoomSpaceNotes', 'RoomDimensions', 'COLUMN';


-- Changes to SitePlanDraft Table
ALTER TABLE SitePlanDraft
DROP COLUMN VolunteerFlow;
ALTER TABLE SitePlanDraft
DROP COLUMN SupplyFlow;
GO
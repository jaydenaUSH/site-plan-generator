INSERT INTO SitePlanRequest
    (VenueName, VenueAddress, VolunteerCount, NumberofLines, MealPackageGoal,
     RoomSpaceNotes, LoadingNotes, AvaialableEquipment, Deadline,
     SpecialConstraints, ProjectType, AdditionalNotes, RoomBlueprintFilePath)
VALUES
    ('First Baptist Church Gym', '123 Main St Orlando FL 32801', 25, 4, 5000,
     'Open gym floor approx 80x100 ft with high ceilings',
     'Loading dock on east side accessible by box truck',
     '6 folding tables and 2 pallet jacks', '2026-08-15 00:00:00',
     'Must vacate by 6pm for evening service', 'Packaging Event',
     'Coordinate with facilities manager one week prior',
     '/blueprints/fbc_gym_layout.pdf')
     GO
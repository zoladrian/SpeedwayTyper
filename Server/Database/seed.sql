-- Reset the league data before inserting a new season snapshot
TRUNCATE TABLE "Predictions" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Matches" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Teams" RESTART IDENTITY CASCADE;

-- Base teams participating in every season
INSERT INTO "Teams" ("Name") VALUES
    ('Apator Toruń'),
    ('Betard Sparta Wrocław'),
    ('Platinum Motor Lublin'),
    ('Orlen Oil Motor Gdańsk'),
    ('Cellfast Wilki Krosno'),
    ('Eltrox Włókniarz Częstochowa'),
    ('For Nature Solutions KS Apator'),
    ('Zielona-energia.com Włókniarz');

-- 2024 season rounds
INSERT INTO "Matches" ("Date", "Round", "HostTeamId", "GuestTeamId", "IsCompleted") VALUES
    ('2024-04-07T15:00:00Z', 1, 1, 2, FALSE),
    ('2024-04-07T17:30:00Z', 1, 3, 4, FALSE),
    ('2024-04-08T17:00:00Z', 1, 5, 6, FALSE),
    ('2024-04-09T17:00:00Z', 1, 7, 8, FALSE),
    ('2024-04-14T15:00:00Z', 2, 2, 3, FALSE),
    ('2024-04-14T17:30:00Z', 2, 4, 5, FALSE),
    ('2024-04-15T17:00:00Z', 2, 6, 7, FALSE),
    ('2024-04-16T17:00:00Z', 2, 8, 1, FALSE),
    ('2024-04-21T15:00:00Z', 3, 1, 3, FALSE),
    ('2024-04-21T17:30:00Z', 3, 2, 4, FALSE),
    ('2024-04-22T17:00:00Z', 3, 5, 7, FALSE),
    ('2024-04-23T17:00:00Z', 3, 6, 8, FALSE);

-- 2025 season opening rounds
INSERT INTO "Matches" ("Date", "Round", "HostTeamId", "GuestTeamId", "IsCompleted") VALUES
    ('2025-04-06T15:00:00Z', 1, 8, 7, FALSE),
    ('2025-04-06T17:30:00Z', 1, 6, 5, FALSE),
    ('2025-04-07T17:00:00Z', 1, 4, 3, FALSE),
    ('2025-04-08T17:00:00Z', 1, 2, 1, FALSE),
    ('2025-04-13T15:00:00Z', 2, 3, 2, FALSE),
    ('2025-04-13T17:30:00Z', 2, 5, 4, FALSE),
    ('2025-04-14T17:00:00Z', 2, 7, 6, FALSE),
    ('2025-04-15T17:00:00Z', 2, 1, 8, FALSE);

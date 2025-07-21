-- Check if column exists first
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Preferences' 
    AND COLUMN_NAME = 'TrialStartDate'
)
BEGIN
    -- Add the column if it doesn't exist
    ALTER TABLE Preferences ADD TrialStartDate datetime2 NULL;
    PRINT 'Column TrialStartDate added to Preferences table';
END
ELSE
BEGIN
    PRINT 'Column TrialStartDate already exists';
END

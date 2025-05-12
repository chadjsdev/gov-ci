
-- dotnet ef migrations add Initial-Create --context EcfrContext
-- dotnet ef database update --context EcfrContext
-- dotnet ef migrations add AddIndexes --context EcfrContext
CREATE INDEX IX_Corrections_ErrorCorrected ON Corrections(ErrorCorrected);
CREATE INDEX IX_CorrectionReferences_Title_Part_Subpart_Section ON CorrectionReferences(Title, Part, Subpart, Section);
CREATE INDEX IX_CfrReferences_Title_Part_Subpart_Section ON CfrReferences(Title, Part, Subpart, Section);
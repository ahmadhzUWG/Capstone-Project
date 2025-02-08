/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/


MERGE INTO [User] AS Target
USING (VALUES ('admin', 'admin', 'Admin')) AS Source (Username, Password, Role)
ON Target.Username = Source.Username
WHEN MATCHED THEN
    UPDATE SET Password = Source.Password, Role = Source.Role
WHEN NOT MATCHED THEN
    INSERT (Username, Password, Role)
    VALUES (Source.Username, Source.Password, Source.Role); 



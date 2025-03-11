SET IDENTITY_INSERT AspNetUsers ON;

INSERT INTO AspNetUsers (ID, UserName, NormalizedUserName, Email, NormalizedEmail, AccessFailedCount, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled)
VALUES (20, 'Employee4', 'EMPLOYEE4', 'employee4@gmail.com', 'EMPLOYEE4@GMAIL.COM', 0, 'True', 'False', 'False', 'True');

SET IDENTITY_INSERT AspNetUsers OFF;

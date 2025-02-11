Capstone Project Group 4

The purpose of this project is to create a task/ticket system that could be utilized to streamline and tackle everyday tasks for an organization.

To setup this project, you must install EntityFramworkCore(EFC), EFC.Abstractions, EFC.Anaylzers, EFC.Design, EFC.InMemory, EFC.SqlServer, EFC.Tools, xunit, xunit.assert, xunit.core, ReSharper's DotCover
For testing we utilize xunit and EFC.inMemory to mock the database, For code coverage, we utilize Resharper's DotCover
You also need to run the following command in cmd as an administrator to allow connection to the localhost port

cd "C:\Program Files (x86)\IIS Express"

IisExpressAdminCmd.exe setupsslUrl -url:https://localhost:44367/ -UseSelfSigned

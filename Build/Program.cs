using System;
using WixSharp;

namespace Build
{
    public class Program
    {
        private static readonly string projectName = "Sapper";
        public static readonly string version = "1.0.1";
        static void Main(string[] args)
        {
            string currentDictionary = @"E:\C#\RevitSapper";
            var project = new Project
            {
                Name = projectName,
                UI = WUI.WixUI_ProgressOnly,
                OutDir = "output",
                GUID = new Guid("cfc42999-0087-40fc-af02-e6c254658599"),
                MajorUpgrade = MajorUpgrade.Default,
                Codepage = "1251",
                ControlPanelInfo =
                {
                    Manufacturer = Environment.UserName,
                },
                Dirs = new Dir[]
                {
                    new InstallDir(@"%AppDataFolder%\Autodesk\Revit\Addins\2022\",
                        new WixSharp.File($@"{currentDictionary}\ADDIN\Sapper.addin"),
                        new Dir(@"Sapper",
                        new DirFiles($@"{currentDictionary}\RevitSapper\Resources\*.*"),
                        new DirFiles($@"{currentDictionary}\WindowSapper\bin\Debug\net8.0-windows\*.*"),
                        new DirFiles($@"{currentDictionary}\RevitSapper\bin\Debug\*.*")))
                },
                Version = new Version(version),
            };

            project.BuildMsi();
        }
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitSapper
{
    internal class RevitApplication : IExternalApplication
    {
        private readonly string assemblyPath = Assembly.GetExecutingAssembly().Location;
        private readonly string settingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Autodesk\setting.db");
        private Dictionary<string, List<ElementId>> dictElementId = new Dictionary<string, List<ElementId>>();
        private DateTime lastUpdated = DateTime.MinValue;
        public Result OnShutdown(UIControlledApplication application)
        {
            application.Idling -= OnIdling;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Задаем имя основной вкладки
            string tabName = "Сапер";
            application.CreateRibbonTab(tabName);
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Как запустить?");
            string uri1 = @"/RevitSapper;component/Resources/free.png";
            PushButtonData buttondata1 = ButtonData("Информация", assemblyPath, "RevitSapper.RevitSapper");
            AddButton(ribbonPanel, buttondata1, "Нажмите и ознакомьтесь с расположением файла приложения", uri1);
            application.Idling += OnIdling;
            return Result.Succeeded;
        }
        private void OnIdling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            try
            {
                if (!(sender is UIApplication uiApp))
                    return;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                if (uiDoc == null)
                    return;
                Document doc = uiDoc.Document;
                RevitSapper gameSapper = new RevitSapper();
                if (!File.Exists(settingPath))
                {
                    if (dictElementId.Count > 0)
                        gameSapper.ClearDictionary(doc, dictElementId);
                    dictElementId.Clear();
                    return;
                }
                if (File.GetLastWriteTime(settingPath) == lastUpdated)
                    return;
                lastUpdated = File.GetLastWriteTime(settingPath);
                gameSapper.LoadFamily(doc);
                gameSapper.CreatePicture(doc, dictElementId);
            }
            catch { }
        }
        private PushButtonData ButtonData(string buttonName, string path, string linkToCommand)
        {
            return new PushButtonData(
                buttonName,
                buttonName,
                path,
                linkToCommand);
        }
        private PushButton AddButton(RibbonPanel ribbonPanel, PushButtonData buttonData, string toolTip, string uri)
        {
            PushButton Button = ribbonPanel.AddItem(buttonData) as PushButton;
            Button.ToolTip = toolTip;
            Button.LargeImage = (ImageSource)new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
            return Button;
        }
    }
}

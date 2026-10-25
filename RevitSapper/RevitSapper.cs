using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace RevitSapper
{
    [Transaction(TransactionMode.Manual)]
    public class RevitSapper : IExternalCommand
    {
        private string pathFamily = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                 @"Autodesk\Revit\Addins\2022\Sapper\");
        public string pathSetting = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                 @"Autodesk\setting.db");
        public Dictionary<string, FamilySymbol> dictSymbol = new Dictionary<string, FamilySymbol>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show($"Приложение находится по адресу {pathFamily}WindowSapper.exe. Запустите его и поле отобразится в Revit.");
            return Result.Succeeded;
        }
        public void LoadFamily(Document doc)
        {
            var listSymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();
            using (Transaction trans = new Transaction(doc, "Загрузка семейств"))
            {
                trans.Start();
                for (int i = -1; i < 10; i++)
                {
                    if (i == 0)
                    {
                        dictSymbol.Add(i.ToString(), null);
                        continue;
                    }
                    if (listSymbol.FirstOrDefault(x => x.Name == $"Семейство{i}") == null)
                    {
                        bool isLoaded = doc.LoadFamily(Path.Combine(pathFamily, $"Семейство{i}.rfa"), out Family family);

                        if (isLoaded && family != null)
                        {
                            var symbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                            if (!symbol.IsActive)
                            {
                                symbol.Activate();
                            }
                            dictSymbol.Add(i.ToString(), symbol);
                        }
                        else
                        {
                            TaskDialog.Show("Ошибка", $"Не удалось загрузить семейство {i}");
                        }
                    }
                    else
                    {
                        dictSymbol.Add(i.ToString(), listSymbol.FirstOrDefault(x => x.Name == $"Семейство{i}"));
                    }
                }
                trans.Commit();
            }
        }
        public void CreatePicture(Document doc, Dictionary<string, List<ElementId>> dictElementId)
        {
            using (Transaction trans = new Transaction(doc, "Sapper"))
            {
                trans.Start();

                Level level = new FilteredElementCollector(doc).OfClass(typeof(Level)).First() as Level;
                if (!dictElementId.Keys.Contains("wall"))
                {
                    var wallIds = new List<ElementId>();
                    for (int i = 0; i <= 100; i += 10)
                    {
                        var line1 = Line.CreateBound(new XYZ(0, i, 0), new XYZ(100, i, 0));
                        var line2 = Line.CreateBound(new XYZ(i, 0, 0), new XYZ(i, 100, 0));
                        Wall wall1 = Wall.Create(doc, line1, level.Id, true);
                        Wall wall2 = Wall.Create(doc, line2, level.Id, true);
                        wall1.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(1);
                        wall2.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(1);
                        wallIds.AddRange(new ElementId[] { wall1.Id, wall2.Id });
                    }
                    dictElementId.Add("wall", wallIds);
                }
                if (dictElementId.Keys.Contains("elem"))
                {
                    doc.Delete(dictElementId["elem"]);
                    dictElementId.Remove("elem");
                }
                var str = File.ReadAllText(pathSetting);
                string[] array = str.Split(',');
                var elemIds = new List<ElementId>();
                for (int i = 0; i < 100; i++)
                {
                    if (dictSymbol[array[i]] == null)
                        continue;
                    var famInst = doc.Create.NewFamilyInstance(new XYZ(i % 10 * 10 + 5, 100 - (i / 10 * 10 + 5), 0),
                                                  dictSymbol[array[i]],
                                                  level,
                                                  Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    elemIds.Add(famInst.Id);
                }
                dictElementId.Add("elem", elemIds);
                trans.Commit();
            }
        }
        public void ClearDictionary(Document doc, Dictionary<string, List<ElementId>> dictElementId)
        {
            using (Transaction trans = new Transaction(doc, "Sapper"))
            {
                trans.Start();

                foreach (var delname in dictElementId.Keys)
                {
                    doc.Delete(dictElementId[delname]);
                }

                trans.Commit();
            }
        }
    }
}

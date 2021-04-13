using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.IO;

namespace ExplodeRebarSet
{
    public class App : IExternalApplication
    {
        public static string assemblyPath = "";
        public static string iconsPath = "";
        public static string username = "";

        public Result OnStartup(UIControlledApplication application)
        {
            assemblyPath = typeof(App).Assembly.Location;
            iconsPath = Path.GetDirectoryName(assemblyPath);

            string tabName = "Weandrevit";
            try { application.CreateRibbonTab(tabName); }
            catch { }

            RibbonPanel panel = null;
            List<RibbonPanel> panels = application.GetRibbonPanels(tabName)
                .Where(i => i.Name == "Армирование")
                .ToList();
            if (panels.Count == 1)
            {
                panel = panels.First();
            }
            else
            {
                panel = application.CreateRibbonPanel(tabName, "Армирование");
            }

            PushButton btnAllRebarAsBody = panel.AddItem(new PushButtonData(
                "ExplodeRebarSer",
                "Взорвать",
                assemblyPath,
                "ExplodeRebarSet.CommandExplode")
                ) as PushButton;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }


    }
}

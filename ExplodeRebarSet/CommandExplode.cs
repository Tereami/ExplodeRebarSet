﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace ExplodeRebarSet
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CommandExplode : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            if(sel.GetElementIds().Count == 0)
            {
                message = "Выберите арматурные стержни";
                return Result.Failed;
            }

            Rebar bar = doc.GetElement(sel.GetElementIds().First()) as Rebar;
            if(bar == null)
            {
                message = "Выберите арматурные стержни";
                return Result.Failed;
            }
#if R2017
            XYZ normal = bar.Normal;
#else
            RebarShapeDrivenAccessor acc = bar.GetShapeDrivenAccessor();
            XYZ normal = acc.Normal;
#endif
            RebarBarType barType = doc.GetElement(bar.GetTypeId()) as RebarBarType;

            int rebarStyleNumber = bar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_STYLE).AsInteger();
            RebarStyle rebarStyle = (RebarStyle)rebarStyleNumber;

            RebarHookType hookTypeStart = null;
            ElementId hookStartTypeId = bar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            if (hookStartTypeId != null)
            {
                hookTypeStart = doc.GetElement(hookStartTypeId) as RebarHookType;
            }

            RebarHookType hookTypeEnd = null;
            ElementId hookEndTypeId = bar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            if (hookEndTypeId != null)
            {
                hookTypeEnd = doc.GetElement(hookEndTypeId) as RebarHookType;
            }


            RebarBendData rbd = bar.GetBendData();
            RebarHookOrientation hookOrient0 = rbd.HookOrient0;
            RebarHookOrientation hookOrient1 = rbd.HookOrient1;

            Element host = doc.GetElement(bar.GetHostId());

            List<Curve> curves = bar.GetCenterlineCurves(false, true, true, MultiplanarOption.IncludeOnlyPlanarCurves, 0).ToList();
            int barsCount = bar.NumberOfBarPositions;

            List<ElementId> newRebarIds = new List<ElementId>();
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Explode rebar set");
                for (int i = 0; i < barsCount; i++)
                {
#if R2017
                    Transform barOffset = bar.GetBarPositionTransform(i);
#else
                    Transform barOffset = acc.GetBarPositionTransform(i);
#endif
                    XYZ offset = barOffset.Origin;

                    Rebar newRebar = Rebar.CreateFromCurves(doc, rebarStyle, barType, hookTypeStart, hookTypeEnd, host, normal, curves,
                                                            hookOrient0, hookOrient1, true, false);
                    doc.Regenerate();
                    ElementTransformUtils.MoveElement(doc, newRebar.Id, offset);
                    newRebarIds.Add(newRebar.Id);
                }

                doc.Delete(bar.Id);

                tr.Commit();
            }

            sel.SetElementIds(newRebarIds);

            return Result.Succeeded;

        }
    }
}

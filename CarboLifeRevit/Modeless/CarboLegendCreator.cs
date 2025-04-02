using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using System;
using System.Linq;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.CodeDom;
using System.Collections.Generic;
using Autodesk.Revit.DB.Structure;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB.Visual;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Media.Media3D;

namespace CarboLifeRevit
{
    internal static class CarboLegendCreator
    {
        internal static bool CreateALegendForData(CarboGraphResult resultData, Document doc)
        {
            bool result = false;

            string legendName = resultData.ColourLegendName;
            if (legendName == "")
                legendName = "CLC_ColourLegend";

            try { 
                
                View clcLegendView = getOrCreateView(doc,legendName);

                if (clcLegendView != null)
                {
                    createLegendData(resultData, doc, clcLegendView, null);
                    
                    result = true;

                }
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        internal static bool CreateResultsView(CarboProject targetProject, Document doc, string viewname)
        {
            bool result = false;

            try
            {
                if (viewname == "")
                    viewname = "CLC_Results";
                //Get or make the view
                View resultView = getOrCreateView(doc, viewname);

                if (resultView != null)
                {
                    createLegendData(null, doc, resultView, targetProject);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show(ex.Message);
            }
            //Get or make the text
            //Build the table

            return result;

        }

        private static void createLegendData(CarboGraphResult resultData, Document doc, View clcLegendView, CarboProject project)
        {

            #region COLLECT CLC_Hatch

            string hatchName = "CLC_Hatch";
            FilledRegionType clc_HatchType = null;
            bool exists = false;

            //Get the filled Regions;
            FilteredElementCollector fillRegionTypes = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType));

            foreach (FilledRegionType frt in fillRegionTypes)
            {
                if (frt.Name.Equals(hatchName))
                {
                    clc_HatchType = frt;
                    exists = true;
                    break;
                }
            }

            if (exists == false)
            {

                FilledRegionType myPattern = fillRegionTypes.FirstOrDefault() as FilledRegionType;
                clc_HatchType = myPattern.Duplicate("CLC_Hatch") as FilledRegionType;
            }


            FilteredElementCollector fillPatterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement));
            FillPatternElement solid = null;

            foreach (FillPatternElement fillp in fillPatterns)
            {
                if (fillp.Name == "<Solid fill>")
                    solid = fillp;
            }
            #endregion

            #region COLLECT LINES

            FilteredElementCollector linePatterns = new FilteredElementCollector(doc).OfClass(typeof(GraphicsStyle));
            GraphicsStyle invisible = null;

            foreach (GraphicsStyle linPat in linePatterns)
            {
                if (linPat.Name == "<Invisible lines>")
                    invisible = linPat;
            }

            //SET THE PROPERTIES OF THE DEFAULT HATCh
            if (solid != null)
            {
                clc_HatchType.BackgroundPatternId = solid.Id;
                clc_HatchType.ForegroundPatternId = solid.Id;
                clc_HatchType.BackgroundPatternColor = new Color(0, 0, 0);
            }
            #endregion

            #region COLLECT CLC_Text

            FilteredElementCollector textTypesTypes = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType));
            //
            IEnumerable<Element> textTypesTypes2 = new FilteredElementCollector(doc, clcLegendView.Id).OfCategory(BuiltInCategory.OST_TextNotes).WhereElementIsNotElementType().ToElements();

            TextNoteType clc_TextType = null;
            TextNoteType clc_TextTitleType = null;

            string textName = "CLC_Text";
            string textTitleName = "CLC_TitleText";

            foreach (TextNoteType txtT in textTypesTypes)
            {
                if (txtT.Name == textName)
                {
                    clc_TextType = txtT;
                    break;
                }
            }

            foreach (TextNoteType txtT in textTypesTypes)
            {
                if (txtT.Name == textTitleName)
                {
                    clc_TextTitleType = txtT;
                    break;
                }
            }

            if (clc_TextType == null)
            {
                //Create a text type
                TextNoteType text = textTypesTypes.FirstOrDefault() as TextNoteType;
                if(text != null)
                {
                    clc_TextType = text.Duplicate(textName) as TextNoteType;

                    try
                    {
                        int txtColor = 0;
                        int txtLineWeight = 1;
                        int txtBackground = 1;// 0 = Opaque :: 1 = Transparent
                        int txtShowBorder = 0; // 0 = Off :: 1 = On
                        double txtLdrBordOffset = .1 / 304.8;
                        string txtFont = "Arial Narrow";
                        double txtSize = 2.5 / 304.8; //(2.5mm)
                        double txtTabSize = ((3.0 / 32.0) * (1.0 / 12.0));
                        int txtBold = 0;
                        int txtItalic = 0;
                        int txtUnderline = 0;
                        double txtWidth = 1;

                        // create color using Color.FromArgb with RGB inputs
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(0, 0, 0);
                        // convert color into an integer
                        int colorInt = System.Drawing.ColorTranslator.ToWin32(color);
                        txtColor = colorInt;


                        Parameter colourParam = clc_TextType.LookupParameter("Color");
                        if(colourParam != null )
                        {
                            colourParam.Set(0);
                        }
                        /*
                        Parameter sizeParam = clc_TextType.LookupParameter("Text Size");
                        if (sizeParam != null)
                        {
                            sizeParam.Set(txtSize);
                        }
                        */
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_SIZE).Set(2.5 / 304.8);
                        clc_TextType.get_Parameter(BuiltInParameter.LINE_PEN).Set(txtLineWeight);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_BACKGROUND).Set(txtBackground);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_BOX_VISIBILITY).Set(txtShowBorder);
                        clc_TextType.get_Parameter(BuiltInParameter.LEADER_OFFSET_SHEET).Set(txtLdrBordOffset);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_FONT).Set(txtFont);
                        //clc_TextType.get_Parameter(BuiltInParameter.TEXT_SIZE).Set(txtSize);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_TAB_SIZE).Set(txtTabSize);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD).Set(txtBold);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC).Set(txtItalic);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE).Set(txtUnderline);
                        clc_TextType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).Set(txtWidth);

                    }
                    catch (Exception ex)
                    { 
                        string error = ex.Message;
                    }
                    //clc_TextType.
                }
            }

            //Create Title from BaseClass
            if (clc_TextTitleType == null)
            {
                if (clc_TextType != null)
                {
                    clc_TextTitleType = clc_TextType.Duplicate(textTitleName) as TextNoteType;
                }
                else
                {
                    //Create a text type
                    TextNoteType text = textTypesTypes.FirstOrDefault() as TextNoteType;
                    if (text != null)
                    {
                        clc_TextTitleType = text.Duplicate(textTitleName) as TextNoteType;
                    }
                }
                if (clc_TextTitleType != null)
                {
                    clc_TextTitleType.get_Parameter(BuiltInParameter.TEXT_SIZE).Set(5.0 / 304.8);
                }
            }
            #endregion

            if(project == null)
                createColourLegend(resultData, doc, clcLegendView, clc_TextTitleType, clc_TextType, clc_HatchType, invisible);
            else
                createResultLegend(project, doc, clcLegendView, clc_TextTitleType, clc_TextType, clc_HatchType, invisible);

        }

        private static void createResultLegend(CarboProject project, Document doc, View clcLegendView, TextNoteType clc_TextTitleType, TextNoteType clc_TextType, FilledRegionType clc_HatchType, GraphicsStyle invisibleline)
        {
            double liney = 0.0;

            TextNoteOptions clc_TitleTextOptions = new TextNoteOptions();
            clc_TitleTextOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
            clc_TitleTextOptions.TypeId = clc_TextTitleType.Id;

            //Create the annotation;
            TextNoteOptions clc_TextOptions = new TextNoteOptions();
            clc_TextOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
            clc_TextOptions.TypeId = clc_TextType.Id;


            TextNote.Create(doc, clcLegendView.Id, new XYZ(0.0, (5.0 / 304.8), 0.0), "RESULTS", clc_TitleTextOptions);
            liney = liney - (5.0 / 304.8);

            string text = getResultText(project);

            TextNote resultText = TextNote.Create(doc, clcLegendView.Id, new XYZ(0, liney - (1.25 / 308.4), 0.0), text,clc_TextOptions);
            resultText.Width = (200 / 304.8);
            doc.Regenerate();

            if(resultText.Height == 0)
                liney = liney - (50 / 304.8);
            else
                liney = liney - resultText.Height;

            liney = liney - (5.0 / 304.8);

            writeResultTable(liney, project, doc, clcLegendView, clc_TitleTextOptions, clc_TextOptions, clc_HatchType, invisibleline);



        }

        private static void writeResultTable(double y, CarboProject project, Document doc, View clcLegendView, TextNoteOptions clc_TitleTextOptions, TextNoteOptions clc_TextOptions, FilledRegionType clc_HatchType, GraphicsStyle invisibleline)
        {

            TextNote.Create(doc, clcLegendView.Id, new XYZ(0.0, y, 0.0), "MATERIAL QUANTITIES AND EMBODIED CARBON SCHEDULE", clc_TitleTextOptions);
            y = y - (10.0 / 304.8);

            double x1 = 0.0;
            double x2 = x1 + (50 / 304.8);
            double x3 = x2 + (50 / 304.8);
            double x4 = x3 + (50 / 304.8);

            double x5 = x4 + (20 / 304.8);
            double x6 = x5 + (20 / 304.8);
            double x7 = x6 + (20 / 304.8);
            double x8 = x7 + (20 / 304.8);

            double x9 = x8 + (20 / 304.8);
            double x10 = x9 + (20 / 304.8);
            double x11 = x10 + (20 / 304.8);
            double x12 = x11 + (20 / 304.8);
            double x13 = x12 + (20 / 304.8);
            double x14 = x13 + (20 / 304.8);
            double x15 = x14 + (20 / 304.8);
            double x16 = x15 + (20 / 304.8);
            double x17 = x16 + (20 / 304.8);
            double x18 = x17 + (20 / 304.8);
            double x19 = x18 + (20 / 304.8);
            double x20 = x19 + (20 / 304.8);


            try
            {
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x1, y, 0.0), "Category", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x2, y, 0.0), "Material", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x3, y, 0.0), "Description", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x4, y, 0.0), "Volume", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x5, y, 0.0), "Total Volume", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x6, y, 0.0), "Density", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x7, y, 0.0), "Mass", clc_TextOptions);

                TextNote.Create(doc, clcLegendView.Id, new XYZ(x8, y, 0.0), "ECI", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x9, y, 0.0), "ECI", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x10, y, 0.0), "EC", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x11, y, 0.0), "Total", clc_TextOptions);

                TextNote.Create(doc, clcLegendView.Id, new XYZ(x12, y, 0.0), "A1-A3", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x13, y, 0.0), "A4", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x14, y, 0.0), "A5", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x15, y, 0.0), "B1-B7", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x16, y, 0.0), "C1-C4", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x17, y, 0.0), "D", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x18, y, 0.0), "Added", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x19, y, 0.0), "Sequestration", clc_TextOptions);


                y = y - (3.5 / 304.8);

                //Units
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x4, y, 0.0), "m³", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x5, y, 0.0), "m³", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x6, y, 0.0), "kg/m³", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x7, y, 0.0), "kg", clc_TextOptions);

                TextNote.Create(doc, clcLegendView.Id, new XYZ(x8, y, 0.0), "kgCO₂/kg", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x9, y, 0.0), "kgCO₂/m³", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x10, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x11, y, 0.0), "%", clc_TextOptions);

                TextNote.Create(doc, clcLegendView.Id, new XYZ(x12, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x13, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x14, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x15, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x16, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x17, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x18, y, 0.0), "tCO₂e", clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x19, y, 0.0), "tCO₂e", clc_TextOptions);


                Line L1 = Line.CreateBound(new XYZ(x1, y - (5.0 / 304.8), 0.0), new XYZ(x20, y - (5.0 / 304.8), 0.0));
                doc.Create.NewDetailCurve(clcLegendView, L1);

                y = y - (5.5 / 304.8);


                ObservableCollection<CarboGroup> cglist = project.getGroupList;
                cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

                string material = "";

                double totalA1 = 0;
                double totalA4 = 0;
                double totalA5 = 0;
                double totalB = 0;
                double totalC = 0;
                double totalD = 0;
                double totalM = 0;
                double totalS = 0;

                foreach (CarboGroup cbg in cglist)
                {
                    double h1 = 0;
                    double h2 = 0;
                    double h3 = 0;

                    //If this is the first instance of a group, then write the title of the material
                    if (cbg.MaterialName != material)
                    {
                        y = y - (2 / 304.8);

                        material = cbg.MaterialName;
                        
                        TextNote title = TextNote.Create(doc, clcLegendView.Id, new XYZ(x1, y, 0.0), material, clc_TextOptions);
                        FormattedText titleText = title.GetFormattedText();
                        titleText.SetBoldStatus(true);
                        title.SetFormattedText(titleText);

                        y = y - (5 / 304.8);
                    }

                    TextNote cat = TextNote.Create(doc, clcLegendView.Id, new XYZ(x1, y, 0.0), cbg.Category, clc_TextOptions);
                    TextNote mat = TextNote.Create(doc, clcLegendView.Id, new XYZ(x2, y, 0.0), cbg.Material.Name, clc_TextOptions);
                    TextNote des = TextNote.Create(doc, clcLegendView.Id, new XYZ(x3, y, 0.0), cbg.Description, clc_TextOptions);

                    cat.Width = (50 / 304.8);
                    mat.Width = (50 / 304.8);
                    des.Width = (50 / 304.8);

                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x4, y, 0.0), Math.Round(cbg.Volume, 2).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x5, y, 0.0), Math.Round(cbg.TotalVolume, 2).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x6, y, 0.0), cbg.Density.ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x7, y, 0.0), Math.Round(cbg.Mass, 2).ToString(), clc_TextOptions);

                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x8, y, 0.0), Math.Round((cbg.ECI), 2).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x9, y, 0.0), Math.Round((cbg.getVolumeECI), 2).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x10, y, 0.0), Math.Round((cbg.EC), 2).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x11, y, 0.0), Math.Round((cbg.PerCent), 2).ToString(), clc_TextOptions);

                    totalA1 += (cbg.Material.ECI_A1A3 * cbg.Mass);
                    totalA4 += (cbg.Material.ECI_A4 * cbg.Mass);
                    totalA5 += (cbg.Material.ECI_A5 * cbg.Mass);
                    totalB += (cbg.Material.ECI_B1B5 * cbg.Mass);
                    totalC += (cbg.Material.ECI_C1C4 * cbg.Mass);
                    totalD += (cbg.Material.ECI_D * cbg.Mass);
                    totalM += (cbg.Material.ECI_Mix * cbg.Mass);
                    totalS += (cbg.Material.ECI_Seq * cbg.Mass);

                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x12, y, 0.0), Math.Round((cbg.Material.ECI_A1A3 * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x13, y, 0.0), Math.Round((cbg.Material.ECI_A4 * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x14, y, 0.0), Math.Round((cbg.Material.ECI_A5 * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x15, y, 0.0), Math.Round((cbg.Material.ECI_B1B5 * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x16, y, 0.0), Math.Round((cbg.Material.ECI_C1C4 * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x17, y, 0.0), Math.Round((cbg.Material.ECI_D * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x18, y, 0.0), Math.Round((cbg.Material.ECI_Mix * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);
                    TextNote.Create(doc, clcLegendView.Id, new XYZ(x19, y, 0.0), Math.Round((cbg.Material.ECI_Seq * cbg.Mass) / 1000, 3).ToString(), clc_TextOptions);


                    doc.Regenerate();

                    h1 = cat.Height * 304.8;
                    h2 = mat.Height * 304.8;
                    h3 = des.Height * 304.8;

                    double max = Math.Max(h1, Math.Max(h2, h3));

                    //Next Line
                    if (max > 4)
                    {
                        y = y - (max / 304.8);
                    }
                    else
                    {
                        y = y - (4 / 304.8);
                    }
                }

                CarboGroup totalGroup = project.getTotalsGroup();
                if(totalGroup != null)
                {

                y = y - (2.5 / 304.8);
                
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x1, y, 0.0), "Totals", clc_TextOptions);


                TextNote.Create(doc, clcLegendView.Id, new XYZ(x4, y, 0.0), Math.Round(totalGroup.Volume, 2).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x5, y, 0.0), Math.Round(totalGroup.TotalVolume, 2).ToString(), clc_TextOptions);
               // TextNote.Create(doc, clcLegendView.Id, new XYZ(x6, y, 0.0), cbg.Density.ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x7, y, 0.0), Math.Round(totalGroup.Mass, 2).ToString(), clc_TextOptions);

                //TextNote.Create(doc, clcLegendView.Id, new XYZ(x8, y, 0.0), Math.Round((cbg.ECI), 2).ToString(), clc_TextOptions);
                //TextNote.Create(doc, clcLegendView.Id, new XYZ(x9, y, 0.0), Math.Round((cbg.getVolumeECI), 2).ToString(), clc_TextOptions);
                //TextNote.Create(doc, clcLegendView.Id, new XYZ(x10, y, 0.0), Math.Round((totalGroup.EC), 2).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x11, y, 0.0), "100%", clc_TextOptions);

                TextNote.Create(doc, clcLegendView.Id, new XYZ(x12, y, 0.0), Math.Round(totalA1 / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x13, y, 0.0), Math.Round(totalA4 / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x14, y, 0.0), Math.Round(totalA5 / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x15, y, 0.0), Math.Round(totalB / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x16, y, 0.0), Math.Round(totalC / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x17, y, 0.0), Math.Round(totalD / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x18, y, 0.0), Math.Round(totalM / 1000, 3).ToString(), clc_TextOptions);
                TextNote.Create(doc, clcLegendView.Id, new XYZ(x19, y, 0.0), Math.Round(totalS / 1000, 3).ToString(), clc_TextOptions);

            }



            }
            catch
            {
            }

        }

        public static string getResultText(CarboProject CarboLifeProject)
        {
            string result = "";

            try
            {

                result += "Total Embodied Carbon: " + CarboLifeProject.getTotalEC().ToString() + " tCO₂e" + Environment.NewLine;

                //List<string> textGroups = CarboLifeProject.getCalcText();
                result = ReportBuilder.getFlattenedCalText(CarboLifeProject);
                result += Environment.NewLine;
                result += CarboLifeProject.getGeneralText();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Message", MessageBoxButton.OK);
            }
            return result;
        }

        private static void createColourLegend(CarboGraphResult resultData, Document doc, View clcLegendView, TextNoteType clc_titleText, TextNoteType clc_textText, FilledRegionType clc_HatchType, GraphicsStyle clc_invisibleLine)
        {

            //FROM HERE ON USE :
            //// clc_HatchType for SOlids
            //// clc_TextType for text
            /// clc_TextTitleType for Titles
            ////  GraphicsStyle invisible for lines


            CurveLoop newc = new CurveLoop();
            //IList<CurveLoop> contour = areaLoad.GetLoops();

            //double y_offset = 0.0150;

            double totalHeight = 125 / 304.8; ;

            //round all the datapoints 
            foreach (CarboValues cv in resultData.validData)
                cv.Value = Math.Round(cv.Value, 3);

            //Get unique points
            IList<CarboValues> uniquValues = new List<CarboValues>();
            uniquValues = resultData.validData.GroupBy(x => x.Value).Select(y => y.First()).ToList();

            //Get Sorted List
            List<CarboValues> SortedList = uniquValues.OrderBy(o => o.Value).ToList();

            int totalDataPoint = SortedList.Count;
            double y_offset = totalHeight / totalDataPoint;
            double x_text_offset = 0.11;
            double liney = 0;

            //Write Title

            //Create the annotation;
            //XYZ origin = new XYZ(0.0, 0.0, 0.0);


            TextNoteOptions clc_TitleTextOptions = new TextNoteOptions();
            clc_TitleTextOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
            clc_TitleTextOptions.TypeId = clc_titleText.Id;

            //Create the annotation;
            TextNoteOptions clc_TextOptions = new TextNoteOptions();
            clc_TextOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
            clc_TextOptions.TypeId = clc_textText.Id;



            TextNote.Create(doc, clcLegendView.Id, new XYZ(0.0, (5.0 / 304.8), 0.0), "COLOUR LEGEND", clc_TitleTextOptions);

            liney = liney - (5.0 / 304.8);

            Line L1 = Line.CreateBound(new XYZ(0.0, liney, 0.0), new XYZ(x_text_offset + x_text_offset, liney, 0.0));
            doc.Create.NewDetailCurve(clcLegendView, L1);


            //liney = liney - minTextGap;

            //XYZ line2a = new XYZ(0.0, liney, 0.0);
            TextNote.Create(doc, clcLegendView.Id, new XYZ(0.0, liney, 0.0), "COLOUR", clc_TextOptions);
            TextNote.Create(doc, clcLegendView.Id, new XYZ(x_text_offset, liney, 0.0),"[" + resultData.Unit + "]", clc_TextOptions);
            // TextNote.Create(doc, clcLegendView.Id, new XYZ(x_text_offset + x_text_offset, liney, 0.0), "VALUE", clc_TextOptions);
            liney = liney - (5.0 / 304.8);


            Line L2 = Line.CreateBound(new XYZ(0.0, liney, 0.0), new XYZ(x_text_offset + x_text_offset, liney, 0.0));
            doc.Create.NewDetailCurve(clcLegendView, L2);

            if(resultData.outOfBoundsMinData.Count >0)
            {
                //Draw a out of boundbox:
                List<CurveLoop> box = getBox(liney, (5 / 308.4), clc_invisibleLine);
                FilledRegion filledRegion = FilledRegion.Create(doc, clc_HatchType.Id, clcLegendView.Id, box);

                //now set colour overrides;
                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetProjectionLineColor(new Color(resultData.outOfBoundsMinData[0].r, resultData.outOfBoundsMinData[0].g, resultData.outOfBoundsMinData[0].b));
                ogs.SetSurfaceTransparency(0);
                ogs.SetProjectionLineWeight(1);
                clcLegendView.SetElementOverrides(filledRegion.Id, ogs);

                TextNote.Create(doc, clcLegendView.Id, 
                    new XYZ(x_text_offset, liney - (1.25 / 308.4), 0.0),
                    Math.Round(resultData.outOfBoundsMinData.Min(x => x.Value),3).ToString() + " " + resultData.Unit,
                    clc_TextOptions);

                liney = liney - (5 / 304.8);
            }

            double endy = 0;

            try
            {
                //vars for check Spacing check
                double minTextGap = 2.6 / 304.8; //(2.5mm)
                double lastTexty = 0;

                for (int i = 0; i < totalDataPoint; i++)
                {
                    //min linespacing;

                    CarboValues dataPoint = SortedList[i];

                    //List<CurveLoop> box = getBox(liney, y_offset, clc_invisibleLine);

                    //Draw Rectangles = Text
                    List<CurveLoop> profileList = new List<CurveLoop>();

                    double this_y_offset = liney - (i * y_offset);
                    endy = this_y_offset - y_offset;

                    XYZ[] points = new XYZ[5];
                    points[0] = new XYZ(0.0, this_y_offset, 0.0);
                    points[1] = new XYZ(0.090, this_y_offset, 0.0);
                    points[2] = new XYZ(0.090, this_y_offset - y_offset, 0.0);
                    points[3] = new XYZ(0.0, this_y_offset - y_offset, 0.0);
                    points[4] = new XYZ(0.0, this_y_offset, 0.0);

                    CurveLoop profileloop = new CurveLoop();

                    //create lines from points
                    for (int j = 0; j < 4; j++)
                    {
                        Line line = Line.CreateBound(points[j], points[j + 1]);
                        if (clc_invisibleLine != null)
                        {
                            line.SetGraphicsStyleId(clc_invisibleLine.Id);
                        }
                        profileloop.Append(line);
                    }

                    profileList.Add(profileloop);

                    FilledRegion filledRegion = FilledRegion.Create(doc, clc_HatchType.Id, clcLegendView.Id, profileList);

                    //now set colour overrides;
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetProjectionLineColor(new Color(dataPoint.r, dataPoint.g, dataPoint.b));
                    ogs.SetSurfaceTransparency(0);
                    ogs.SetProjectionLineWeight(1);
                    clcLegendView.SetElementOverrides(filledRegion.Id, ogs);

                    //The Location Of the the text
                    double textY = this_y_offset - (y_offset / 2) + (1.25 / 304.8);
                    XYZ textPoint = new XYZ(x_text_offset, textY, 0.0);

                    double deltaText = lastTexty - textY;

                    if(deltaText >= minTextGap)
                    {
                        lastTexty = this_y_offset - (y_offset / 2) + (1.25 / 304.8);
                        TextNote note = TextNote.Create(doc, clcLegendView.Id, textPoint, dataPoint.Value.ToString() + " " + resultData.Unit, clc_TextOptions);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            liney = endy;

            if (resultData.outOfBoundsMaxData.Count > 0)
            {
                //Draw a out of boundbox:
                List<CurveLoop> box = getBox(liney, (5 / 308.4), clc_invisibleLine);
                FilledRegion filledRegion = FilledRegion.Create(doc, clc_HatchType.Id, clcLegendView.Id, box);

                //now set colour overrides;
                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetProjectionLineColor(new Color(resultData.outOfBoundsMaxData[0].r, resultData.outOfBoundsMaxData[0].g, resultData.outOfBoundsMaxData[0].b));
                ogs.SetSurfaceTransparency(0);
                ogs.SetProjectionLineWeight(1);
                clcLegendView.SetElementOverrides(filledRegion.Id, ogs);

                TextNote.Create(doc, clcLegendView.Id, 
                    new XYZ(x_text_offset, liney - (1.25 / 308.4), 0.0),
                    Math.Round(resultData.outOfBoundsMaxData.Max(x => x.Value),3).ToString() + " " + resultData.Unit,
                    clc_TextOptions);

                liney = liney - (5 / 304.8);

            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topLeftYCoord">the Y coordinatefor the box</param>
        /// <param name="y_offset">the height of the box (down)</param>
        /// <param name="clc_invisibleLine">lineType</param>
        /// <returns></returns>
        private static List<CurveLoop> getBox(double topLeftYCoord, double y_offset, GraphicsStyle clc_invisibleLine)
        {
            List<CurveLoop> result = new List<CurveLoop>();

            XYZ[] points = new XYZ[5];
            points[0] = new XYZ(0.0, topLeftYCoord, 0.0);
            points[1] = new XYZ(0.090, topLeftYCoord, 0.0);
            points[2] = new XYZ(0.090, topLeftYCoord - y_offset, 0.0);
            points[3] = new XYZ(0.0, topLeftYCoord - y_offset, 0.0);
            points[4] = new XYZ(0.0, topLeftYCoord, 0.0);

            //The Location Of the the text

            CurveLoop profileloop = new CurveLoop();

            //create lines from points
            for (int j = 0; j < 4; j++)
            {
                Line line = Line.CreateBound(points[j], points[j + 1]);
                if (clc_invisibleLine != null)
                {
                    line.SetGraphicsStyleId(clc_invisibleLine.Id);
                }
                profileloop.Append(line);
            }

            result.Add(profileloop);

            return result;
        }

        private static View getOrCreateView(Document doc, string viewname)
        {                
            View clcLegendView = null;

            try
            {

                //Get all the Views in uidoc
                ElementClassFilter filter = new ElementClassFilter(typeof(Autodesk.Revit.DB.View));
                FilteredElementCollector collection = new FilteredElementCollector(doc);
                IList<Element> AllViews = collection.WherePasses(filter).OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();


                //Check if a legend already exists:

                foreach (Element el in AllViews)
                {
                    View viewElement = el as View;
                    if (viewElement != null)
                    {
                        //this is a valid View
                        if (viewElement.Name == viewname)
                        {
                            clcLegendView = viewElement;
                            break;
                        }
                    }
                }
                //Delete it and clear it
                if (clcLegendView != null)
                {

                    //Go though each element
                    IEnumerable<Element> Textcollector = new FilteredElementCollector(doc, clcLegendView.Id).OfCategory(BuiltInCategory.OST_TextNotes).WhereElementIsNotElementType().ToElements();
                    IEnumerable<Element> Hatchcollector = new FilteredElementCollector(doc, clcLegendView.Id).OfCategory(BuiltInCategory.OST_DetailComponents).WhereElementIsNotElementType().ToElements();
                    //IEnumerable<Element> LineCollector = new FilteredElementCollector(doc, clcLegendView.Id).OfClass(typeof(FilledRegion)).WhereElementIsNotElementType().ToElements();
                    IEnumerable<Element> LineCollector = new FilteredElementCollector(doc, clcLegendView.Id).OfCategory(BuiltInCategory.OST_Lines).WhereElementIsNotElementType().ToElements();

                    IList<ElementId> AllViewsIds = new List<ElementId>();

                    foreach (Element el in Textcollector)
                    {
                        if (el.Id != clcLegendView.Id)
                            AllViewsIds.Add(el.Id);
                    }
                    foreach (Element el in Hatchcollector)
                    {
                        if (el.Id != clcLegendView.Id)
                            AllViewsIds.Add(el.Id);
                    }
                    foreach (Element el in LineCollector)
                    {
                        if (el.Id != clcLegendView.Id)
                            AllViewsIds.Add(el.Id);
                    }

                    doc.Delete(AllViewsIds);
                }
                else
                {
                    //ViewFamilyType vdTExt = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(q => q.ViewFamily == ViewFamily.Legend);
                    //vdTExt.Duplicate("Newleged");

                    ViewFamilyType vd = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(q => q.ViewFamily == ViewFamily.Drafting);
                    clcLegendView = ViewDrafting.Create(doc, vd.Id);
                    clcLegendView.Scale = 1; //1:1
                    clcLegendView.Name = viewname;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            return clcLegendView;
    }
    }
}

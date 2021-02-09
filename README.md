# CarboLifeCalc
A Quick-Action Carbon full life calculator using Revit material quantities.

Status: Work in Good Progress

Current Build for testing download link: (29-11-2020)

https://drive.google.com/file/d/1W8pboSt65k_g3eWFbqFkMYgf_uinYVnx/view?usp=sharing

The current version holds a solid set of tools to use EPDs (Environmental Product Declarations) or customised materials to calculate embodied carbon in a building design. 
As a standalone application you can set quantities and materials,  or you can use a 3d model from Revit to import your building into the calculator. Your revit materials will automatically be mapped to to the CarboLifeCalc's own, thus giving you an instant result on the building's embodied carbon.

Videos:

Installation and demonstration:
https://youtu.be/pJJy4qMkvHk

Heatmaps:
https://youtu.be/O0gkl9B8Mvw

Screenshots:

![alt text](http://www.davidveld.nl/img/CarboCalc1.jpg)
![alt text](http://www.davidveld.nl/img/CarboCalc2.jpg)
![alt text](http://www.davidveld.nl/img/CarboCalc3.jpg)

Roadmap:

1. Improved material Database
2. Load materials from an on-line database, use material editor to save and sync to template & share online.
3. Carbon sequestering calculator in the material editor
4. Better B1-B7 analysis
5. Better A5 calculation
6. Improved C1-C4 calculation
7. Interface improvements.
8. Material Mapping
9. Write values into Revit Elements
10. Easy sync materials with template and vice versa.

Updates as per build 29.11.2020:

1. Metal deck uses convertion value, and will thus keep the elements associated with them. 
2. Heatmaps can now be normalized.

Updates as per build 25.11.2020:

1. EPD import possibility, using a new menu to convert volume based data to mass based data used in CarboLifeCalc
2. Feedback calculation results back into Revit to visualise the embodied carbon using a colour override.

# CarboLifeCalc
A Quick-Action Carbon Whole Life Carbon Assessment calculator using Revit material quantities.

Status: Work in Good Progress

Current Build for testing download link: (15-03-2021)

Zip:
https://drive.google.com/file/d/1uH7d4axtyb70D-U0QhDMairfdnuqLc4T/view?usp=sharing

Installer:
https://drive.google.com/file/d/11fARKfv_IilLaR9W-Rrg_xY4yoGYQ4aC/view?usp=sharing

The current version holds a solid set of tools to use EPDs (Environmental Product Declarations) or customised materials to calculate embodied carbon in a building design. 
As a standalone application you can set quantities and materials,  or you can use a 3d model from Revit to import your building into the calculator. Your revit materials will automatically be mapped to to the CarboLifeCalc's own, thus giving you an instant result on the building's embodied carbon.

Videos:

Installation and demonstration:
https://youtu.be/pJJy4qMkvHk

Heatmaps:
https://youtu.be/O0gkl9B8Mvw

Screenshots:
![alt text](https://www.davidveld.nl/img/carbocalc/bim1.jpg)
![alt text](https://www.davidveld.nl/img/CarboCalc1.jpg)
![alt text](https://www.davidveld.nl/img/CarboCalc2.jpg)
![alt text](https://www.davidveld.nl/img/CarboCalc3.jpg)

Roadmap:

1. Bug fixing
2. Update a project with new elements from Revit
3. Carbon sequestering calculator in the material editor
4. Record movie to explain working of addin
5. Project compare interface
6. Improved graphics for overview

Updates as per build 15.03.2021
1. Installer
2. Bugs in material editor 
3. Add a waste factor to group
4. B4 now works per group as well as per material
5. Improve material database
6. Improve B1-B7 analysis
7. Improve A5 calculation
8. Improve C1-C4 calculation
9. Reinforcement can be written into existing groups, in the material or as seperate elements

Updates as per build 24.02.2021
1.Bugs in material editor
2.Allowance to mix a material into another, for reinforcement or steel fixings in timber
4.HTML Report exporter updated
5.Export the entire project into excel
6.Load materials from an on-line database, use material editor to save and sync to template & share online. 

Updates as per build 19.02.2021
1. Improved sync, including online sync options
2. Direct access to material editor froun splash screen
3. Material Mapping now possible

Updates as per build 09.02.2021
1. Improved heatmap creator
2. Ability to write total carbon into the elements
3. Simple Syncronizing ability with the template added to the material editor
4. Activate Revit addins within tool.

Updates as per build 29.11.2020:
1. Metal deck uses convertion value, and will thus keep the elements associated with them. 
2. Heatmaps can now be normalized.

Updates as per build 25.11.2020:
1. EPD import possibility, using a new menu to convert volume based data to mass based data used in CarboLifeCalc
2. Feedback calculation results back into Revit to visualise the embodied carbon using a colour override.

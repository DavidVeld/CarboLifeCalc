# CarboLifeCalc
Carbo Life Calculator is an embodied carbon calculator for the build
environment. The application transforms quantities of materials, from
Revit, or manually specified to calculate the embodied carbon. Carbo
Life Calculator will automatically find available embodied carbon data
from EPDs or various databases and map these to your design. Thus
giving you almost instantaneous results.

Status: Work in Good Progress

Current Build for testing download link: (14.04.2021)  
(build 28.04.2021)  

Zip:  
https://github.com/DavidVeld/CarboLifeCalc/releases/download/2021.04.28/280428.-.Build.For.Testing.zip

Installer:  
https://github.com/DavidVeld/CarboLifeCalc/releases/download/2021.04.28/CarboLifeCalcInstaller.msi


For help and quick guide please see:  
https://github.com/DavidVeld/CarboLifeCalc/wiki  
Help documentation (formatted pdf):  
https://drive.google.com/file/d/1Omg-_2WtXjBDKgRInD4yzH7YDLlE1Hy1/view?usp=sharing  

Videos:  
Installation and demonstration:
https://youtu.be/pJJy4qMkvHk

Heatmaps:  
https://youtu.be/O0gkl9B8Mvw



Screenshots:
![alt text](https://www.davidveld.nl/img/carbocalc/bim1.jpg)
![alt text](https://www.davidveld.nl/img/carbocalc/CarboCalc1.png)
![alt text](https://www.davidveld.nl/img/carbocalc/CarboCalc2.png)
![alt text](https://www.davidveld.nl/img/carbocalc/CarboCalc3.png)
![alt text](https://www.davidveld.nl/img/carbocalc/CarboCalc4.png)

#Roadmap (essential for release):
1. Increase database with more materials.
2. Bug fixing (on-going, please supply feedback)
3. Full review of all functions and windows.
4. Record movie + Help docs to explain working of addin

#Roadmap (Future):
1. Carbon sequestering calculator (Fun, but low priority)
2. Project compare interface added graphs
3. Link into grasshopper and/or Tekla

Updates as per build 28.04.2021
1. Moved user materials to AppData Folder
2. Fixed Save and Open 
3. Material Template can now be shared among multiple PCs
4. Bugs 
5. Links to resources and additional information added

Updates as per build 14.04.2021 (Zip oply)  
1. Rounding error fixed in graph   
2. Moved user materials to AppData Folder (Test)  

Updates as per build 29.03.2021
1. Compare projects tab  
2. Update projects from Revit  
3. Improved graphics for overview  
4. Bug fixes  
5. Reduced file size when saving projects imported from Revit  

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
1. Bugs in material editor  
2. Allowance to mix a material into another, for reinforcement or steel fixings in timber  
4. HTML Report exporter updated  
5. Export the entire project into excel  
6. Load materials from an on-line database, use material editor to save and sync to template & share online.   

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

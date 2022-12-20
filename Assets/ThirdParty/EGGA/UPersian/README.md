# EGGA

UPersian:
Unity Game Engine RTL Support

This project used [ArabicSupprt for Unity](https://www.assetstore.unity3d.com/en/#!/content/2674) Asset.

## Features:

Supports Arabic and Persian for now. (~~Waiting for @Konash to opensource ArabicSupport plugin~~).

New GUI Items(Creation MenuItem > UPersian > ): 
- RTL Text
- RTL Inputfield
- RTL Buton 
- RTL Checkbox

### Runtime RTL Input Field

![inputfield](https://cloud.githubusercontent.com/assets/19928031/16045524/05988ed8-325e-11e6-8be9-f919321def01.gif)

### Supports BestFit:

![bestfit](https://cloud.githubusercontent.com/assets/19928031/16045806/5e3c93e4-325f-11e6-9bab-9242df7c225b.gif)


### RightClick Contex Menu:

![rightclickcontextmenu](https://cloud.githubusercontent.com/assets/19928031/16046308/371c261a-3261-11e6-83ee-2864cbffb57b.gif)


## String Extentions:
- string RtlFix(): fixes original rtl strings to show in unity GUI. (ie. ```string hello = "سلام".RtlFix();```)
- bool IsRtl: returns whether string is rtl or not. (ie.```bool checkRtl = "سلام".IsRtl();```)

## How to use:
- Import latest unitypackage into your project.
- Right click in hierarchy panel > UPersian > Rtl Text
- Select RtlText in hierarchy and change text inside inspector, everything should show correct in GUI.

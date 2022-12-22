# Editing KoboldKare

1. Install git. If you're on modern windows, you can use the `winget` package manager for this:
   ![winget example image](winget_install.png)
   Just install it by typing `winget install Git.Git` into a powershell or cmd.
2. Install blender. Funnily enough this is *also* in the windows package manager. You can install it with `winget install BlenderFoundation.Blender`.
3. Install UnityHub, winget has this available as: `winget install UnityTechnologies.UnityHub`
4. Download the KoboldKare's version of Unity. As of this post it is 2021.3.6f1, which you can download by putting this link into a browser: [unityhub://2021.3.6f1/7da38d85baf6](unityhub://2021.3.6f1/7da38d85baf6)
5. Restart the computer. This is important if you've just installed git. As you need to make sure the git executable is in your path **before** you try to open the project.
6. Clone the KoboldKare repository. This can be done with something like Github Desktop if you're unfamiliar with git, otherwise you just `git clone git@github.com:naelstrof/KoboldKare.git` or `git clone https://github.com/naelstrof/KoboldKare.git`.
7. Open the KoboldKare folder with UnityHub. It should now import correctly!

This is all you need to do to open, edit, and test KoboldKare.

# Building
In order to build KoboldKare, you first need to build addressables.
You can find instructions on that here: https://docs.unity3d.com/Packages/com.unity.addressables@1.20/manual/Builds.html

After that you can build normally.
# Penetration Tech

Penetration Tech is a procedural animation system that handles deformation for both a penetrator (a dick), and a penetratee (an orifice).

## Demo!

An ExampleScene Unity scene is included in the project. Simply loading it up and hitting play should show you this:

![A demo reel of kobolds getting dicked](https://cdn.discordapp.com/attachments/410685928466808843/636452750082965535/k8WhbaHuA1.mp4)

## Requirements

This animation tech only works with the following requirements:

* Unity version 2019.1 and later.
* Unity Animation Rigging package. (Currently in Preview)
* Both the penetrator and penetratee must be SkinnedMeshRenderers.
* Both the penetrator and penetratee require a specific bone setup and Blendshapes for the full effect (described below).
* Blendshape, Armature, and Rigging knowledge basics.

## Model Setup

The tech requires meshes to be setup in a specifc way, setup is different for both the penetrator and penetratee. This document also assumes 
**Imporant**: On export, ensure that you export with Tangent Space.

![Blender image of export settings with Tangent Space enabled](https://i.imgur.com/q8wPVlB.png)

And that on Unity import that you enable Tangents import.

![Unity image of import settings](https://i.imgur.com/HRPfHOa.png)

---

### Penetrator Model Setup

A penetrator could be by itself, or attached to a body. Any configuration works provided it follows these requirements.

#### Penetrator Armature Setup

The penetrator should have an Armature with at least one bone. It should be at least partially weight painted to the bone that represents the penetrator.

![A picture of a weight painted penetrator](https://i.imgur.com/AhtdV1P.png)

The penetrator will work best if the bone origin is positioned slightly behind or directly at the base of the penetrator.

### Penetrator Deformation Setup (Optional)

Optionally if your penetrator is squishy, you might want to implement 4 shape-keys relevent to different kinds of fleshy deformations.
They require this *exact* naming in order to be recognized by the Penetration Tech as usable deformations.
Also, if you want any fleshy deformations at all, you need to implement *all* of them.

#### DickConfine

This blendshape is used whenever the penetrator is partially embedded into a penetratee (confined at all sides). It generally isn't very visible unless extreme investigation is made.

![A picture of a penetrator squeezed on the Z and X axis](https://i.imgur.com/E7mTc4a.png)

#### DickSquish

This blendshape is used whenever the penetrator is being pressed down upon. Only occurs during movement going into a penetratee.

![A picture of a penetrator squeezed on the Y axis](https://i.imgur.com/zVH57pK.png)

Conservation of volume would cause knots to bulge, heads to widen, and generally for the whole shaft to get thicker and shorter. Exaggeration of this effect looks pretty nice!

#### DickPull

This blendshape is used whenever the penetrator is getting tugged on. Only occurs during movement going away from a penetratee.

![A picture of a penetrator stretched on the Y axis](https://i.imgur.com/JT63ei9.png)

#### DickCum

This blendshape is triggered manually by the user, it's partially triggered along the length of the penetrator which could be described as a "pumping" type animation.

![A picture of a penetrator inflated](https://i.imgur.com/IYf08ve.png)

![An animation of the same penetrator pumping](https://cdn.discordapp.com/attachments/510329587331498006/636152651134009363/6s1Dc6cqe7.mp4)

This has quite a bit of creative freedoms as its triggered specifically through a slider by the user.

---

### Penetratee Model Setup

A penetratee can also be a penetrator, though it still requires some blendshapes, and has its own armature requirements.

#### Penetratee Armature Setup

The penetratee needs a single bone to figure out where the hole is and which way it's facing. It also needs to be weighted to the bone so that it can be deflected.

![A picture of a butt with weights shown as coloring](https://i.imgur.com/gNmu0Z5.png)

Generally you want the part that will contact the penetrator to be weighted 100% to the bone.

![An animation showing the bone deflection](https://cdn.discordapp.com/attachments/410685928466808843/636395345596645396/8kicsyFoEU.mp4)

#### Penetratee Blendshape Setup

The penetratee requires all three of these blendshapes, though luckily each of them is fairly easy to setup.

The blendshape names do not matter whatsoever, though they should be clear enough so that you can distinguish them in a dropdown.

#### Penetratee Expand Blendshape

This blendshape expands the hole radially from the base of the bone.

![A picture of a butt gaping to an extreme degree](https://i.imgur.com/w2oN5eE.png)

You can make it as big as you want, though you should make it as big as your comfortable with since it will trigger to larger sizes to fit larger penetrators.

#### Penetratee Tug Blendshape

This blendshape should pull the hole out a bit, this blendshape is triggered when the penetrator is pulling out quickly.

![A picture of a butt weirdly sticking out](https://i.imgur.com/zqLwrZr.png)

This should follow the way the bone is facing perfectly.

#### Penetratee Push Blendshape

This blendshape should push the hole in a bit, like a penetrator is pressing up against it.

![A picture of a butt being indented](https://i.imgur.com/DBlEEhu.png)

This should also follow along the bone.

---

## Unity Setup

Ensure you have Animation Rigging installed in the Unity Package Manager (Window->Package Manager)
![A picture showing the Animation Rigging installed](https://i.imgur.com/NZnO9s4.png)

## Penetrator Unity Setup

1. Add the Dick component to your GameObject.
2. Set the Dick Transform to the dick bone in your armature.
3. Set the Mesh Parent to the GameObject containing your SkinnedMeshRenderers.
4. Specify the appropriate blendshapes in the Bake tab, then hit Generate All Dick Curves.

![A picture of the inspector at this stage](https://i.imgur.com/TTmKnZx.png)

5. Configure the curves in the Preview Bake tab to more closely match your mesh.

![A picture of me configuring the curves](https://i.imgur.com/IcjkDvd.png)

6. Under the Preview Deformation tab: Set Deformation Targets to your dick mesh. Make sure the material on your meshs uses the Naelstrof/DickDeformation shader so that the deformations show properly.

![A picture of me previewing deformations](https://i.imgur.com/llkykPH.png)

7. Set the target penetratable to interact with and you're done!

## Penetratee Unity Setup

1. Add the Penetratable component to your GameObject.
2. Set the hole Transform to the hole bone you've set up in your Armature.
3. Set the hole mesh to the mesh which blendshapes will be driven.
4. Set the hole mesh blendshapes to the cooresponding spot.

![A picture of the inspector at this stage](https://i.imgur.com/zIEOBlf.png)

5. Under the Preview Tab, adjust the Hole Diameter and Test Diameter sliders until the hole matches up with the 3D view ring.

![A picture of a gaping hole with the corresponding Hole Diameter and Test Diameter sliders.](https://i.imgur.com/nJsgT3M.png)

6. Adjust the Sample Offset to match up the hole better, then switch to the Push and Pull and do the same thing.

![A picture of a gaping hole again, this time being tugged](https://i.imgur.com/FrD6bAQ.png)

7. Set the target dick to interact with and you're done!


## Animation Baking

At some point you might want to apply these procedural effects to an animation for use in other games (Like VRChat), or if you want to get rid of the overhead of the scripts entirely.
Included in the package is a system for applying the procedural parts of the animation onto a pre-existing one! Just follow the steps below to get a full fledged baked animation.

1. The first thing you want is a functioning penetrator and penetratee on your model, under a single animator. (They should penetrate if brought together while in play-mode.)
2. In edit mode, create an animation that you desire. You don't have to worry about making sure the penetrator properly penetrates as that will be taken care of after baking.
3. Ensure the animation plays properly in play mode with the scripts enabled. (With the penetration working and everything.) This information will be recorded by the following script:
4. Add the DickAnimationBaker.cs component to the root of the gameobjects (with the animator). Set Original Animation count to 1 and set it to the clip you made.
5. Press Bake Animations and wait for it to finish, it should print "Exported x.clip to Assets/blah :thumbsup:" to the console when it's done.
6. With a baked animation, you can now see the results of it in edit mode. Here you can touch it up to your liking.
7. Remove the DickAnimationBaker.cs component.
8. Done! You have a generic animation that should work in VRChat without scripting assistance!


### Animation Tips

* You can hot-swap penetrator/penetratee targets inside the animation!
* If it seems delayed or laggy, increase the sample multiplier so more samples are taken.
* Make sure to disable or remove the DickAnimationBaker.cs when you're done with it. Otherwise everytime you hit play it will re-bake.
* For cumming animations to work properly, you have to temporarily set Cum Active to 0 when returning Cum Progress from 2 to -1. It's a bit finnicky but it's the only way to get it to work with interpolated low-sample animations.

---

## Troubleshooting

> *My dick exploded into shards!*

![An unfortunate picture of a dick exploding into shards](https://i.imgur.com/5cNqlzC.png)

If this happens, ensure that your blendshapes (DickConfing, DickSquish, DickPull, and DickCUm) exist. They're used to bake into the uv channels.
Also ensure that those blendshapes are set to 0 on the SkinnedMeshRenderer, and also ensure all the corresponding values in the shader are also set to 0.

![A picture showing the problem source](https://i.imgur.com/nufcbbf.png)

> *My hole is jackhammering!*

![An unfortunate video of a hole jackhammering](https://cdn.discordapp.com/attachments/410685928466808843/636434874508771339/remWWE7nfq.mp4)

This is due to the Penetratable Move Spring setting being too high, simply turn it down in your inspector.

> *Some other problem!*

Okay that's probably my fault, contact me on discord at naelstrof#7705 and holefully we can fix it.

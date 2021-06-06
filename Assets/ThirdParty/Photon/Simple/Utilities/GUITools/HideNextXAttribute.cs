// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Photon.Pun.Simple.Internal
{
    /// <summary>
    /// Attribute for use with HeaderEditor. Tells a bool state to dictate if the following fields will be rendered to the GUI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class HideNextXAttribute : PropertyAttribute
    {
        public int hideCount;
        public bool hideIf;
        public string guiStyle;

        public HideNextXAttribute(int hideCount, bool hideIf, string guiStyle = "HelpBox")
        {
            this.hideCount = hideCount;
            this.hideIf = hideIf;
            this.guiStyle = guiStyle;
        }

    }
}

#endif

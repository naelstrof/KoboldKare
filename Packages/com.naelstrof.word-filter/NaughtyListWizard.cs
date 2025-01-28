using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine.UIElements;

namespace WordFilter {

public class NaughtyListWizard : ScriptableWizard {
    [SerializeField] private VisualTreeAsset display;
    [MenuItem("Tools/Naelstrof/Naughty List Creator")]
    static void CreateWizard() {
        DisplayWizard<NaughtyListWizard>("Create Naughty List", "Save", "Load");
    }
    private void CreateGUI() {
        display.CloneTree(rootVisualElement);
    }

    private void SetEditText(string newText) {
        var textField = rootVisualElement.Q<TextField>();
        textField.value = newText;
    }

    private string GetEditText() {
        var textField = rootVisualElement.Q<TextField>();
        return textField.value;
    }

    private void OnWizardOtherButton() {
        try {
            var filePath = EditorUtility.OpenFilePanelWithFilters("Open Naughty List", Application.dataPath, new[] { "Text", "txt" });
            if (string.IsNullOrEmpty(filePath)) {
                return;
            }
            using FileStream file = File.Open(filePath, FileMode.Open);
            using var decompressor = new GZipStream(file, CompressionMode.Decompress);
            using var streamReader = new StreamReader(decompressor);
            SetEditText(streamReader.ReadToEnd());
        } catch (SystemException e) {
            EditorUtility.DisplayDialog("Error", e.Message, "OK");
            throw;
        }
    }

    private void OnWizardCreate() {
        try {
            var filePath = EditorUtility.SaveFilePanelInProject("Save Naughty List", "naughtyList.txt", "txt", "Saves a compressed text blob.");
            if (string.IsNullOrEmpty(filePath)) {
                return;
            }
            using FileStream file = File.Open(filePath, FileMode.Create);
            using var compressor = new GZipStream(file, CompressionMode.Compress);
            using var streamWriter = new StreamWriter(compressor);
            streamWriter.Write(GetEditText());
            AssetDatabase.Refresh();
        } catch (SystemException e) {
            EditorUtility.DisplayDialog("Error", e.Message, "OK");
            throw;
        }
    }
}

}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using System.IO;
using System.Text;
using SimpleJSON;
using System;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputOptions", menuName = "Data/Input Options", order = 1)]
public class InputOptions : ScriptableObject {
    public UnityEngine.InputSystem.InputActionAsset controls;
    public ScriptableFloat mouseSensitivity;
    public void Save() {
        string savePath = Application.persistentDataPath + "/inputBindings.json";
        FileStream file = File.Create(savePath);
        //string json = JsonUtility.ToJson(overrides, true);
        JSONNode n = JSON.Parse("{}");
        n["Mouse Sensitivity"] = mouseSensitivity.value;
        foreach (var map in controls.actionMaps) {
            foreach (var binding in map.bindings) {
                if (binding.name == "2D Vector" || binding.name == "1D Axis") {
                    continue;
                }
                if (!string.IsNullOrEmpty(binding.overridePath)) {
                    n[binding.action + " " + binding.name + map.FindAction(binding.action).bindings.IndexOf(i => i.id == binding.id)] = binding.overridePath;
                } else {
                    n[binding.action + " " + binding.name + map.FindAction(binding.action).bindings.IndexOf(i => i.id == binding.id)] = binding.path;
                }
            }
        }

        file.Write(Encoding.UTF8.GetBytes(n.ToString(2)),0,n.ToString(2).Length);
        file.Close();
        Debug.Log("Saved input bindings to " + savePath);
    }
    public void Load() {
        try {
            string savePath = Application.persistentDataPath + "/inputBindings.json";
            FileStream file = File.Open(savePath, FileMode.Open);
            byte[] b = new byte[file.Length];
            file.Read(b,0,(int)file.Length);
            file.Close();

            string data = Encoding.UTF8.GetString(b);
            JSONNode n = JSON.Parse(data);
            if (n.HasKey("Mouse Sensitivity")) {
                mouseSensitivity.set(n["Mouse Sensitivity"]);
            }
            foreach (var map in controls.actionMaps) {
                var bindings = map.bindings;
                for (var i = 0; i < bindings.Count; ++i) {
                    if (n.HasKey(bindings[i].action + " " + bindings[i].name + map.FindAction(bindings[i].action).bindings.IndexOf(o => o.id == bindings[i].id))) {
                        map.ApplyBindingOverride(i, new InputBinding { overridePath = n[bindings[i].action + " " + bindings[i].name + map.FindAction(bindings[i].action).bindings.IndexOf(o => o.id == bindings[i].id)] });
                    }
                }
            }
        } catch (Exception e) {
            if (e is FileNotFoundException) {
                return;
            }
            Debug.LogException(e);
        }
    }
    public void OnEnable() {
        Load();
    }
}
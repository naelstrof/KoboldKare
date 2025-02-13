using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WordFilter {
public static class NaughtyList {
    // Use the wizard to create the encrypted blobs
    public static string[] GetNaughtyList(TextAsset encryptedAsset) {
        var bytes = Convert.FromBase64String(encryptedAsset.text);
        var text = Encoding.UTF8.GetString(bytes);
        return text.Split('\n');
    }
    public static string[] GetNaughtyList(string encryptedBlob) {
        var bytes = Convert.FromBase64String(encryptedBlob);
        var text = Encoding.UTF8.GetString(bytes);
        return text.Split('\n');
    }
}
}

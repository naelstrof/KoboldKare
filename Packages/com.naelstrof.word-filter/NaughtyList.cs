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
    // Use the wizard to create the base64-encoded blobs
    public static string[] GetNaughtyList(TextAsset encodedAsset) {
        var bytes = Convert.FromBase64String(encodedAsset.text);
        var text = Encoding.UTF8.GetString(bytes);
        return text.Split('\n');
    }
    public static string[] GetNaughtyList(string encodedBlob) {
        var bytes = Convert.FromBase64String(encodedBlob);
        var text = Encoding.UTF8.GetString(bytes);
        return text.Split('\n');
    }
}
}

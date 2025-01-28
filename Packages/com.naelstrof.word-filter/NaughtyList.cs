using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

namespace WordFilter {
public static class NaughtyList {
    public static string[] GetNaughtyList(TextAsset asset) {
        using var stream = new MemoryStream(asset.bytes);
        using var decompressor = new GZipStream(stream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(decompressor);
        var text = streamReader.ReadToEnd();
        return text.Split('\n');
    }
}
}

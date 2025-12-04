using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using FishNet;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DirectJoinGameButton : MonoBehaviour {
    [SerializeField]
    private TMP_InputField addressField;
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private bool TryGetAddressAndPort(string uriAddress, out string host, out ushort port) {
        var address = addressField.text;
        if (!address.Contains("://")) {
            address = $"http://{address}";
        }
        try {
            var uri = new Uri(address);
            port = 27069;
            host = uri.Host;
            return true;
        } catch {
            port = 27069;
            host = "unknown";
            return false;
        }
    }

    private void OnClick() {
        var networkManager = InstanceFinder.NetworkManager;
        if (SteamIDParser.TryParseSteamId(addressField.text, out var steamId)) {
            var fishyworks = networkManager.GetComponent<FishySteamworks.FishySteamworks>();
            fishyworks.SetClientAddress(steamId.ToString());
            networkManager.GetComponent<Multipass>().SetClientTransport(fishyworks);
            networkManager.ClientManager.StartConnection();
            return;
        }
        if (TryGetAddressAndPort(addressField.text, out var host, out var port)) {
            var tugboat = networkManager.GetComponent<Tugboat>();
            tugboat.SetClientAddress(host);
            tugboat.SetPort(port);
            networkManager.GetComponent<Multipass>().SetClientTransport(tugboat);
            networkManager.ClientManager.StartConnection();
            return;
        }
        Debug.LogError("Failed to parse address for direct join.");
    }
}

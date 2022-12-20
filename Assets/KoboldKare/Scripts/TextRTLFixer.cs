using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UPersian.Utils;

public class TextRTLFixer : MonoBehaviour {
    private TMP_Text target;
    private WaitForSeconds waitForSeconds;
    private void OnEnable() {
        target = GetComponent<TMP_Text>();
        GetComponent<LocalizeStringEvent>()?.OnUpdateString.AddListener(OnStringChanged);
        OnStringChanged(target.text);
        waitForSeconds = new WaitForSeconds(1f);
    }

    private void OnDisable() {
        GetComponent<LocalizeStringEvent>()?.OnUpdateString.RemoveListener(OnStringChanged);
    }

    private void OnStringChanged(string newString) {
        if (isActiveAndEnabled && LocalizationSettings.SelectedLocale.Identifier.Code == "ar" && !newString.IsRtl()) {
            StartCoroutine(PollForChange());
            return;
        }

        if (newString.IsRtl()) {
            target.SetText(newString.RtlFix());
        }
    }

    private IEnumerator PollForChange() {
        int tries = 0;
        while (!target.text.IsRtl() && tries++ < 10) {
            yield return waitForSeconds;
        }

        if (target.text.IsRtl()) {
            target.SetText(target.text.RtlFix());
        }
    }
}

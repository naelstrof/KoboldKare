using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GameEventSanitizer {
#if UNITY_EDITOR
    private static bool TryConvertListener(SerializedProperty listener, out string methodName, out GameEventResponse response) {
        methodName = listener.FindPropertyRelative("m_MethodName").stringValue;
        var target = listener.FindPropertyRelative("m_Target").objectReferenceValue;
        var boolParam = listener.FindPropertyRelative("m_Arguments.m_BoolArgument");
        var objectParam = listener.FindPropertyRelative("m_Arguments.m_ObjectArgument");
        var stringParam = listener.FindPropertyRelative("m_Arguments.m_StringArgument");
        switch ((methodName, target)) {
            case ("set_enabled", MonoBehaviour m):
                response = new GameEventResponseBehaviorsSetEnabled() {
                    enabled = boolParam.boolValue,
                    targets = new [] { m }
                };
                return true;
            case ("SetActive", GameObject g):
                response = new GameEventResponseGameObjectSetActive() {
                    active = boolParam.boolValue,
                    targets = new [] { g }
                };
                return true;
            case ("Play", Animator a):
                response = new GameEventResponseAnimatorPlay() {
                    targets = new [] {
                        new GameEventResponseAnimatorPlay.AnimatorPlayTarget() {
                            animator = a,
                            animationName = stringParam.stringValue
                        }
                    }
                };
                return true;
            case ("PlayOneShot", AudioSource source):
                response = new GameEventResponseAudioSourcePlayOneShot() {
                    clip = (AudioClip)objectParam.objectReferenceValue,
                    target = source,
                };
                return true;
            case ("Play", AudioSource source):
                response = new GameEventResponsePlayAudioSource() {
                    target = source,
                };
                return true;
            case ("Play", UnityEngine.Video.VideoPlayer player):
                response = new GameEventResponseVideoPlayerPlay() {
                    target = player,
                };
                return true;
            case ("Pause", UnityEngine.Video.VideoPlayer player):
                response = new GameEventResponseVideoPlayerPause() {
                    target = player,
                };
                return true;
            case ("set_enabled", AudioSource source):
                response = new GameEventResponseAudioSourceEnable() {
                    target = source,
                    enabled = boolParam.boolValue,
                };
                return true;
            case ("ActivateInputField", TMPro.TMP_InputField inputField):
                response = new GameEventResponseInputFieldActivate() {
                    target = inputField,
                };
                return true;
            case ("DeactivateInputField", TMPro.TMP_InputField inputField):
                response = new GameEventResponseInputFieldDeactivate() {
                    target = inputField,
                };
                return true;
            case ("Interrupt", null):
                response = new GameEventResponseMusicInterrupt(){
                };
                return true;
            case ("SetTrigger", Animator animator):
                response = new GameEventResponseAnimatorTrigger() {
                    targets = new [] {
                        new GameEventResponseAnimatorTrigger.AnimatorTriggerTarget() {
                            animator = animator,
                            triggerName = stringParam.stringValue
                        }
                    }
                };
                return true;
            default:
                response = null;
                return false;
        }
        return false;
    }
    public static void SanitizeEditor(string eventName, string responseListName, MonoBehaviour owner) {
        var serializedObject = new SerializedObject(owner);
        var calls = serializedObject.FindProperty($"{eventName}.m_PersistentCalls.m_Calls");
        if (calls == null) {
            return;
        }
        var callCount = calls.arraySize;
        if (callCount == 0) {
            return;
        }

        for (int i = 0; i < callCount; i++) {
            var listener = calls.GetArrayElementAtIndex(i);
            if (!TryConvertListener(listener, out var methodName, out var _)) {
                Debug.LogError( $"No sanitization rule for listener {listener.displayName}: {methodName} on {owner.name}", owner);
                return;
            }
        }

        EditorApplication.delayCall += () => {
            var responseList = serializedObject.FindProperty(responseListName);
            responseList.ClearArray();
            for (int i = 0; i < callCount; i++) {
                var listener = calls.GetArrayElementAtIndex(i);
                if (TryConvertListener(listener, out var methodName, out var response)) {
                    responseList.InsertArrayElementAtIndex(0);
                    responseList.GetArrayElementAtIndex(0).managedReferenceValue = response;
                }
            }

            calls.ClearArray();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("Successfully sanitized event listeners to GameEventResponses on " + owner.name, owner);
        };
    }
#else
    public static void SanitizeEditor(string eventName, string responseListName, MonoBehaviour owner) {
    }
#endif


    private static bool TryConvertListener(string methodName, Object target, MonoBehaviour owner, out GameEventResponse response) {
        switch ((methodName, target)) {
            case ("Play", AudioSource source):
                response = new GameEventResponsePlayAudioSource() {
                    target = source,
                };
                return true;
            case ("Play", UnityEngine.Video.VideoPlayer player):
                response = new GameEventResponseVideoPlayerPlay() {
                    target = player,
                };
                return true;
            case ("Stop", UnityEngine.Video.VideoPlayer player):
                response = new GameEventResponseVideoPlayerStop() {
                    target = player,
                };
                return true;
            case ("Pause", UnityEngine.Video.VideoPlayer player):
                response = new GameEventResponseVideoPlayerPause() {
                    target = player,
                };
                return true;
            case ("ActivateInputField", TMPro.TMP_InputField inputField):
                response = new GameEventResponseInputFieldActivate() {
                    target = inputField,
                };
                return true;
            case ("DeactivateInputField", TMPro.TMP_InputField inputField):
                response = new GameEventResponseInputFieldDeactivate() {
                    target = inputField,
                };
                return true;
            default:
                response = null;
                Debug.LogError($"No sanitization rule for method {methodName} on {target.GetType()}", owner);
                break;
        }
        return false;
    }
    public static void SanitizeRuntime(UnityEvent e, List<GameEventResponse> responses, MonoBehaviour owner) {
        for(int i=0;i<e.GetPersistentEventCount();i++) {
            var methodName = e.GetPersistentMethodName(i);
            var target = e.GetPersistentTarget(i);
            if (TryConvertListener(methodName, target, owner, out var response)) {
                responses.Add(response);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBlock {
    public enum StatChangeSource {
        Equipment,
        Network,
        Misc,
    }
    private bool dirty = true;
    private float nextUpdateTime = float.PositiveInfinity;
    public void Clear() {
        activeEffects.Clear();
        cachedStats.Clear();
        dirty = true;
    }
    public class StatusEffectValidTimeTuple {
        public StatusEffectValidTimeTuple(StatusEffect effect) {
            this.effect = effect;
            this.validUntil = Time.time + effect.duration;
        }
        public StatusEffect effect;
        public float validUntil;
    }
    public List<StatusEffectValidTimeTuple> activeEffects = new List<StatusEffectValidTimeTuple>();
    private Dictionary<Stat, float> cachedStats = new Dictionary<Stat, float>();
    public delegate void StatusEffectsChangedHandler(StatBlock statuses, StatChangeSource source);
    public event StatusEffectsChangedHandler StatusEffectsChangedEvent;
    public float GetStat(Stat t) {
        Regenerate();
        if (cachedStats.ContainsKey(t)) {
            return cachedStats[t];
        } else {
            return 0f;
        }
    }
    public void AddStatusEffect(StatusEffect e, StatChangeSource source = StatChangeSource.Misc) {
        activeEffects.Add(new StatusEffectValidTimeTuple(e));
        dirty = true;
        StatusEffectsChangedEvent?.Invoke(this, source);
    }
    public void RemoveStatusEffect(StatusEffect e, StatChangeSource source = StatChangeSource.Misc, bool removeAllInstances = false) {
        for(int i=0;i<activeEffects.Count;i++) {
            if (activeEffects[i].effect == e) {
                activeEffects.RemoveAt(i);
                dirty = true;
                if (!removeAllInstances) {
                    break;
                }
            }
        }
        StatusEffectsChangedEvent?.Invoke(this, source);
    }
    private void Regenerate() {
        if (!dirty && Time.time < nextUpdateTime) {
            return;
        }
        cachedStats.Clear();
        for (int i = 0; i < activeEffects.Count; i++) {
            if (Time.time > activeEffects[i].validUntil) {
                activeEffects.RemoveAt(i);
            }
        }
        HashSet<StatusEffect> alreadyStackedEffects = new HashSet<StatusEffect>();
        nextUpdateTime = float.PositiveInfinity;
        foreach (var effect in activeEffects) {
            if (!effect.effect.stacks && alreadyStackedEffects.Contains(effect.effect)) {
                continue;
            }
            if (!effect.effect.stacks) {
                alreadyStackedEffects.Add(effect.effect);
            }
            foreach (var stat in effect.effect.statistics) {
                if (!cachedStats.ContainsKey(stat.statType)) {
                    cachedStats.Add(stat.statType, stat.addAmount);
                } else {
                    cachedStats[stat.statType] += stat.addAmount;
                }
                cachedStats[stat.statType] *= stat.multiplier;
            }
            nextUpdateTime = Mathf.Min(effect.validUntil, nextUpdateTime);
        }
        dirty = false;
    }
}

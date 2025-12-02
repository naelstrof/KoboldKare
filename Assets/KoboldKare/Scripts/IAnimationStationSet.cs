using System.Collections.ObjectModel;
using Photon.Pun;
using Vilar.AnimationStation;

public interface IAnimationStationSet {
    public ReadOnlyCollection<AnimationStation> GetAnimationStations();
}

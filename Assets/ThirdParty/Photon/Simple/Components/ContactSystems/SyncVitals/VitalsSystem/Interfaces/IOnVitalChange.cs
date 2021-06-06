// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{

    public interface IOnVitalsChange
    {

    }

    public interface IOnVitalChange
    {

    }

    public interface IOnVitalsValueChange : IOnVitalsChange
    {
        void OnVitalValueChange(Vital vital);
    }

    public interface IOnVitalValueChange : IOnVitalChange
    {
		void OnVitalValueChange(Vital vital);
	}

    public interface IOnVitalsParamChange : IOnVitalsChange
    {
        void OnVitalParamChange(Vital vital);
    }

    public interface IOnVitalParamChange : IOnVitalChange
    {
        void OnVitalParamChange(Vital vital);
    }

}



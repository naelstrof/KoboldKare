// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

namespace Photon.Pun.Simple
{
	public interface IContactGroupsAssign
	{
		//int Index { get; }
		int Mask { get; }
        bool ApplyToChildren { get; }
	}

	public interface IOnNetworkHit
	{
		void OnNetworkHit(NetworkHits results);
	}

	public interface IOnTerminate
	{
		void OnTerminate();
	}

	//public interface IDamageable
	//{
 //       /// <summary>
 //       /// Apply damage to this object, and return remaining damage.
 //       /// </summary>
 //       /// <param name="damage"></param>
 //       /// <returns>Return the remaining damage if not all was applied.</returns>
 //       double ApplyDamage(double damage);
	//	bool IsMine { get; }
	//	int ViewID { get; }
	//}

	//public interface IDamager
	//{

	//}
	//public interface IDamagerOnEnter : IDamager
	//{
	//	void OnEnter(IDamageable iDamageable);
	//}
	//public interface IDamagerOnStay : IDamager
	//{
	//	void OnStay(IDamageable iDamageable);
	//}
	//public interface IDamagerOnExit : IDamager
	//{
	//	void OnExit(IDamageable iDamageable);
	//}

}


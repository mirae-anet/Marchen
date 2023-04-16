using Fusion;
using UnityEngine;

namespace Fusion102
{
	public struct NetworkInputData : INetworkInput
	{
		public Vector3 moveDir;
		public NetworkBool isMove;        
		public NetworkBool walkOn; 	
	}
}

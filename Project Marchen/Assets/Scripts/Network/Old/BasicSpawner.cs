using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion102
{
	public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks 
	{
		private NetworkRunner _runner;
        private CameraController camControl;
		private void OnGUI()
		{
			if (_runner == null)
			{
				if (GUI.Button(new Rect(0,0,200,40), "Host"))
				{
					StartGame(GameMode.Host);
				}
				if (GUI.Button(new Rect(0,40,200,40), "Join"))
				{
					StartGame(GameMode.Client);
				}
			}
		}

		async void StartGame(GameMode mode)
		{
			// Create the Fusion runner and let it know that we will be providing user input
			_runner = gameObject.AddComponent<NetworkRunner>();
			_runner.ProvideInput = true;
	    
			// Start or join (depends on gamemode) a session with a specific name
			await _runner.StartGame(new StartGameArgs()
			{
				GameMode = mode, 
				SessionName = "TestRoom", 
				Scene = SceneManager.GetActiveScene().buildIndex,
				SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
			});
		}

		[SerializeField] private NetworkPrefabRef _playerPrefab; // Character to spawn for a joining player
		private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			if (runner.IsServer)
			{
				Vector3 spawnPosition = new Vector3((player.RawEncoded%runner.Config.Simulation.DefaultPlayers)*1,5,0);
				NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
				_spawnedCharacters.Add(player, networkPlayerObject);
                camControl = networkPlayerObject.GetComponentInChildren<CameraController>();
			}
			// if(player == runner.LocalPlayer)
			// {
			// 	NetworkObject networkObject = runner.GetPlayerObject(player);
			// 	camControl = networkObject.GetComponentInChildren<CameraController>();
			// 	//????
			// }
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
			{
				runner.Despawn(networkObject);
				_spawnedCharacters.Remove(player);
			}
		}
		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
			// 자료 참고해서 아래 코드를 handler로 분리하고 .local을 사용해서 조인시 각자 켐컨트롤 설정, 그 값을 호스트에게 전달
            var data = new Fusion102.NetworkInputData();
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));    // 이동 입력 값으로 moveInput 벡터
            bool isMove = (moveInput.magnitude != 0);
            data.isMove = isMove;
            if (isMove)
            {
                data.moveDir = camControl.GetMoveDir(moveInput);
            }
            // for animation
            data.walkOn = Input.GetButton("Walk");
            // send data to host
            input.Set(data);
		}
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
		public void OnConnectedToServer(NetworkRunner runner) { }
		public void OnDisconnectedFromServer(NetworkRunner runner) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
		public void OnSceneLoadDone(NetworkRunner runner) { }
		public void OnSceneLoadStart(NetworkRunner runner) { }
	}
}

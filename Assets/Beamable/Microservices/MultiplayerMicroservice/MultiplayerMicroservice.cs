using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.TicTacToe;
using Beamable.Server;
using UnityEngine;

namespace Beamable.Microservices
{
	[Microservice("MultiplayerMicroservice")]
	public class MultiplayerMicroservice : Microservice
	{
		[ClientCallable]
		public async Promise BroadcastMove (string channelName, List<long> playersToNotify, MoveEvent moveEvent)
		{
			try
			{
                await Services.Notifications.NotifyPlayer(playersToNotify, channelName, moveEvent);
            }
			catch (Exception e)
			{
                Debug.LogError($"Error while broadcasting move to notification channel {channelName}. \nError: {e.Message}");
            }
        }
	}
}
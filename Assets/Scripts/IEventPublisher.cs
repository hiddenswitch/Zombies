using UnityEngine;
using System.Collections;

namespace Tennis
{
	public interface IEventPublisher<TDelegateInterface>
	{
		void AddEventListener (TDelegateInterface d);

		void RemoveEventListener (TDelegateInterface d);
	}

	public interface IMatchDelegate
	{
		void HandlePointWasScored(int winPlayer);
		void HandleGameWasScored(int winPlayer);
		void HandleGameEnd(int winPlayer);
		void HandleServeFault (int losePlayer);
	}
	
	public interface IMatchControllerDelegate{
		void HandleFadeOutComplete();
	}
}
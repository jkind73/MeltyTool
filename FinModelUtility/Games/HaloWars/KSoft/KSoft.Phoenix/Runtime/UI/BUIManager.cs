
namespace KSoft.Phoenix.Runtime
{
	// Since BWorld serialization hasn't been finished, this code hasn't even been tested

	sealed class BuiManager
		: IO.IEndianStreamSerializable
	{
		public BTimerManager timerManager = new BTimerManager();
		public BuiCallouts uiCallouts = new BuiCallouts();
		public BuiWidget uiWidgets = new BuiWidget();
		public bool widgetsVisible, talkingHeadShown, objectiveTrackerShown,
			timerShown, objectiveWidgetsShown, hintsVisible,
			minimapVisible, unitStatsVisible, reticleVisible,
			resourcePanelVisible, dpadPanelVisible, gameTimeVisible;
		public float minimapRotationOffset;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(this.timerManager);
			s.Stream(this.uiCallouts);
			s.Stream(this.uiWidgets);
			s.Stream(ref this.widgetsVisible); s.Stream(ref this.talkingHeadShown); s.Stream(ref this.objectiveTrackerShown);
			s.Stream(ref this.timerShown); s.Stream(ref this.objectiveWidgetsShown); s.Stream(ref this.hintsVisible);
			s.Stream(ref this.minimapVisible); s.Stream(ref this.unitStatsVisible); s.Stream(ref this.reticleVisible);
			s.Stream(ref this.resourcePanelVisible); s.Stream(ref this.dpadPanelVisible); s.Stream(ref this.gameTimeVisible);
			s.Stream(ref this.minimapRotationOffset);
		}
		#endregion
	};
}
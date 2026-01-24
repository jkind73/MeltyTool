
namespace KSoft.Wwise.SoundBank
{
	[System.Reflection.Obfuscation(Exclude=true)]
	enum HircType : uint
	{
		STATE = 1,
		SOUND,
		ACTION,
		EVENT,
		RAN_SEQ_CNTR,
		SWITCH_CNTR,
		ACTOR_MIXER,
		BUS,
		LAYER_CNTR,
		SEGMENT,
		TRACK,
		MUSIC_SWITCH,
		MUSIC_RAN_SEQ,
		ATTENUATION,
		DIALOGUE_EVENT,
		FEEDBACK_BUS,
		FEEDBACK_NODE,

		FX_SHARE_SET,
		FX_CUSTOM,
		AUX_BUS,
	};
}
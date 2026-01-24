
namespace KSoft.Phoenix.Phx
{
	public enum BPlacementRuleType
	{
		AND,
		OR,

		DISTANCE_AT_MOST_FROM_TYPE,
		DISTANCE_AT_LEAST_FROM_TYPE,
		OBSTRUCTION_AT_LEAST_FROM_TYPE,
	};
	public enum BPlacementRuleFromTypeKind
	{
		BUILDER,
		UNIT,
	};
	public enum BPlacementRuleLifeType
	{
		ANY,
		ALIVE,
		DEAD,
	};
	public enum BPlacementRuleFoundationType
	{
		ANY,
		SOLID,
		FULLY_BUILT,
	};

	/*public*/ sealed class BPlacementRule
	{

	#region Xml constants
	#endregion
  };
	/*public*/ sealed class BPlacementRules;
}

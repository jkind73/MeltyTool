using Expr = System.Linq.Expressions.Expression;
using ExprParam = System.Linq.Expressions.ParameterExpression;

namespace KSoft
{
	partial class EnumFlags<TEnum>
	{
		/// <summary>
		/// Implements the expressions by casting the enum parameters to their underlying integer types before
		/// doing a bitwise operation on them
		/// </summary>
		static class V1
		{
			public static readonly ModifyDelegate
				KAddFlags = GenerateAddFlagsMethod(),
				KRemoveFlags = GenerateRemoveFlagsMethod();
			public static readonly ModifyByRefDelegate
				KAddFlagsByRef = GenerateAddFlagsMethodByRef(),
				KRemoveFlagsByRef = GenerateRemoveFlagsMethodByRef();

			public static readonly ModifyCondDelegate KModifyFlags = GenerateModifyFlagsMethod();
			public static readonly ModifyByRefCondDelegate KModifyFlagsByRef = GenerateModifyFlagsMethodByRef();

			public static readonly ReadDelegate KTestFlags = GenerateTestFlagsMethod();


			static Expr GenerateAddFlagsGuts(ExprParam paramV, ExprParam paramF)
			{
				var vAsInt = Expr.Convert(paramV, KUnderlyingType);					// integer v = (integer)value
				var fAsInt = Expr.Convert(paramF, KUnderlyingType);					// integer f = (integer)flags

				return Expr.Convert(Expr.Or(vAsInt, fAsInt), KEnumType);			// (TEnum)(v | f)
			}
			static Expr GenerateRemoveFlagsGuts(ExprParam paramV, ExprParam paramF)
			{
				var vAsInt = Expr.Convert(paramV, KUnderlyingType);					// integer v = (integer)value
				var fAsInt = Expr.Convert(paramF, KUnderlyingType);					// integer f = (integer)flags

				var fComplement = Expr.Not(fAsInt);									// ~f

				return Expr.Convert(Expr.And(vAsInt, fComplement), KEnumType);		// (TEnum)(v & ~f)
			}
			static Expr GenerateModifyFlagsGuts(ExprParam paramV, ExprParam paramF, ExprParam paramCond)
			{
				return Expr.Condition(paramCond,
					GenerateAddFlagsGuts(paramV, paramF),
					GenerateRemoveFlagsGuts(paramV, paramF));
			}

			static ModifyDelegate GenerateAddFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var paramV = GenerateParamValue(false);
				var paramF = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// return value | flags
				var ret = GenerateAddFlagsGuts(paramV, paramF);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyDelegate>(ret, paramV, paramF);
				return lambda.Compile();
			}
			static ModifyByRefDelegate GenerateAddFlagsMethodByRef()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var paramV = GenerateParamValue(true);
				var paramF = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// value = value | flags
				var assign = Expr.Assign(paramV, GenerateAddFlagsGuts(paramV, paramF));

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyByRefDelegate>(assign, paramV, paramF);
				return lambda.Compile();
			}

			static ModifyDelegate GenerateRemoveFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var paramV = GenerateParamValue(false);
				var paramF = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// return value & flags
				var ret = GenerateRemoveFlagsGuts(paramV, paramF);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyDelegate>(ret, paramV, paramF);
				return lambda.Compile();
			}
			static ModifyByRefDelegate GenerateRemoveFlagsMethodByRef()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var paramV = GenerateParamValue(true);
				var paramF = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// value = value & flags
				var assign = Expr.Assign(paramV, GenerateRemoveFlagsGuts(paramV, paramF));

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyByRefDelegate>(assign, paramV, paramF);
				return lambda.Compile();
			}

			static ModifyCondDelegate GenerateModifyFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var paramV = GenerateParamValue(false);
				var paramF = GenerateParamFlags();
				var paramC = GenerateParamAddOrRemove();

				//////////////////////////////////////////////////////////////////////////
				// return value & flags
				var ret = GenerateModifyFlagsGuts(paramV, paramF, paramC);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyCondDelegate>(ret, paramC, paramV, paramF);
				return lambda.Compile();
			}
			static ModifyByRefCondDelegate GenerateModifyFlagsMethodByRef()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var paramV = GenerateParamValue(true);
				var paramF = GenerateParamFlags();
				var paramC = GenerateParamAddOrRemove();

				//////////////////////////////////////////////////////////////////////////
				// return value & flags
				var assign = Expr.Assign(paramV, GenerateModifyFlagsGuts(paramV, paramF, paramC));

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyByRefCondDelegate>(assign, paramC, paramV, paramF);
				return lambda.Compile();
			}

			static ReadDelegate GenerateTestFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters and return constructs
				var paramV = GenerateParamValue(false);
				var paramF = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// return (value & flags) == flags
				var vAsInt = Expr.Convert(paramV, KUnderlyingType);
				var fAsInt = Expr.Convert(paramF, KUnderlyingType);

				var and = Expr.Convert(Expr.And(vAsInt, fAsInt), KEnumType);
				var equ = Expr.Equal(and, paramF);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ReadDelegate>(equ, paramV, paramF);
				return lambda.Compile();
			}
		};
	};
}

using Expr = System.Linq.Expressions.Expression;
using ExprParam = System.Linq.Expressions.ParameterExpression;

namespace KSoft
{
	using EnumUtils = Reflection.EnumUtils;

	partial class EnumFlags<TEnum>
	{
		/// <summary>
		/// Implements the expressions by directly accessing the enum's internal integer field and doing bitwise
		/// operations on that instead
		/// </summary>
		static class V2
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
				var paramVMember=Expr.PropertyOrField(paramV, EnumUtils.K_MEMBER_NAME);	// value.value__
				var paramFMember=Expr.PropertyOrField(paramF, EnumUtils.K_MEMBER_NAME);	// flags.value__

				var or = Expr.Or(paramVMember, paramFMember);
				return Expr.Assign(paramVMember, or);
				//return Expr.OrAssign(param_v_member, param_f_member);					// value.value__ |= flags.value__
			}
			static Expr GenerateRemoveFlagsGuts(ExprParam paramV, ExprParam paramF)
			{
				var paramVMember=Expr.PropertyOrField(paramV, EnumUtils.K_MEMBER_NAME);	// value.value__
				var paramFMember=Expr.PropertyOrField(paramF, EnumUtils.K_MEMBER_NAME);	// flags.value__

				var fComplement = Expr.Not(paramFMember);							// ~flags.value__

				var and = Expr.And(paramVMember, fComplement);
				return Expr.Assign(paramVMember, and);
				//return Expr.AndAssign(param_v_member, f_complement);					// value.value__ &= ~flags.value__
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
				var ret = Expr.Convert(GenerateAddFlagsGuts(paramV, paramF), KEnumType);

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
				var assign = GenerateAddFlagsGuts(paramV, paramF);

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
				var ret = Expr.Convert(GenerateRemoveFlagsGuts(paramV, paramF), KEnumType);

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
				var assign = GenerateRemoveFlagsGuts(paramV, paramF);

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
				var ret = Expr.Convert(GenerateModifyFlagsGuts(paramV, paramF, paramC), KEnumType);

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
				var assign = GenerateModifyFlagsGuts(paramV, paramF, paramC);

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
				var paramVMember = Expr.PropertyOrField(paramV, EnumUtils.K_MEMBER_NAME);
				var paramFMember = Expr.PropertyOrField(paramF, EnumUtils.K_MEMBER_NAME);

				var and = Expr.And(paramVMember, paramFMember);
				var equ = Expr.Equal(and, paramFMember);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ReadDelegate>(equ, paramV, paramF);
				return lambda.Compile();
			}
		};
	};
}

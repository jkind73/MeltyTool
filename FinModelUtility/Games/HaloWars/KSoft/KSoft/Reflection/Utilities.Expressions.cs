using System;
using System.Linq;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Expr = System.Linq.Expressions.Expression;
using Reflect = System.Reflection;

namespace KSoft.Reflection
{
	public partial class Util
	{
		public const int K_GENERATE_DYNAMIC_DELEGATE_MAXIMUM_PARAMETERS = 16;
		const string K_DELEGATE_INVOKE_METHOD_NAME_ = "Invoke";

		// Reference:
		// http://www.codeproject.com/KB/cs/FastMethodInvoker.aspx
		// http://www.codeproject.com/KB/cs/AsyncMethodInvocation.aspx

		static Type GetDynamicDelegateActionType(int paramCount)
		{
			switch (paramCount)
			{
				case 0:  return typeof(Action);
				case 1:  return typeof(Action<>);
				case 2:  return typeof(Action<,>);
				case 3:  return typeof(Action<,,>);
				case 4:  return typeof(Action<,,,>);
				case 5:  return typeof(Action<,,,,>);
				case 6:  return typeof(Action<,,,,,>);
				case 7:  return typeof(Action<,,,,,,>);
				case 8:  return typeof(Action<,,,,,,,>);
				case 9:  return typeof(Action<,,,,,,,,>);
				case 10: return typeof(Action<,,,,,,,,,>);
				case 11: return typeof(Action<,,,,,,,,,,>);
				case 12: return typeof(Action<,,,,,,,,,,,>);
				case 13: return typeof(Action<,,,,,,,,,,,,>);
				case 14: return typeof(Action<,,,,,,,,,,,,,>);
				case 15: return typeof(Action<,,,,,,,,,,,,,,>);
				case 16: return typeof(Action<,,,,,,,,,,,,,,,>);

				default:
					throw new Debug.UnreachableException(paramCount.ToString(KSoft.Util.InvariantCultureInfo));
			}
		}
		static Type GetDynamicDelegateFuncType(int paramCount)
		{
			switch (paramCount)
			{
				case 0:  return typeof(Func<>);
				case 1:  return typeof(Func<,>);
				case 2:  return typeof(Func<,,>);
				case 3:  return typeof(Func<,,,>);
				case 4:  return typeof(Func<,,,,>);
				case 5:  return typeof(Func<,,,,,>);
				case 6:  return typeof(Func<,,,,,,>);
				case 7:  return typeof(Func<,,,,,,,>);
				case 8:  return typeof(Func<,,,,,,,,>);
				case 9:  return typeof(Func<,,,,,,,,,>);
				case 10: return typeof(Func<,,,,,,,,,,>);
				case 11: return typeof(Func<,,,,,,,,,,,>);
				case 12: return typeof(Func<,,,,,,,,,,,,>);
				case 13: return typeof(Func<,,,,,,,,,,,,,>);
				case 14: return typeof(Func<,,,,,,,,,,,,,,>);
				case 15: return typeof(Func<,,,,,,,,,,,,,,,>);
				case 16: return typeof(Func<,,,,,,,,,,,,,,,,>);

				default:
					throw new Debug.UnreachableException(paramCount.ToString(KSoft.Util.InvariantCultureInfo));
			}
		}
		static Type GetDynamicDelegateType(bool hasResult, int paramCount)
		{
			return hasResult
				? GetDynamicDelegateFuncType(paramCount)
				: GetDynamicDelegateActionType(paramCount);
		}
		static Type[] GetDynamicDelegateParamTypes(Type result, params Type[] parameters)
		{
			bool hasResult = result != null;

			var types = parameters;
			if (hasResult)
			{
				types = new Type[parameters.Length + 1];

				int i = 0;
				foreach (var param in parameters)
					types[i++] = param;
				types[i] = result;
			}

			return types;
		}
		public static Type GenerateDynamicDelegateType(Type result, params Type[] parameters)
		{
			Contract.Requires<ArgumentNullException>(parameters != null);
			Contract.Requires<ArgumentException>(parameters.Length <= K_GENERATE_DYNAMIC_DELEGATE_MAXIMUM_PARAMETERS);

			bool hasResult = result != null || result != typeof(void);

			var delType = GetDynamicDelegateType(hasResult, parameters.Length);
			var delParams = GetDynamicDelegateParamTypes(result, parameters);

			return delType.MakeGenericType(delParams);
		}
		public static Type GenerateDynamicDelegateType(Reflect.MethodInfo method)
		{
			return GenerateDynamicDelegateType(method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
		}

		public static TFunc GenerateObjectMethodProxy<T, TFunc, TSig>(
			string methodName,
			Reflect.BindingFlags bindingAttr = Reflect.BindingFlags.NonPublic | Reflect.BindingFlags.Instance)
			where TFunc : class
			where TSig : class
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(methodName));
			Contract.Requires<ArgumentException>(typeof(TSig).IsSubclassOf(typeof(Delegate)));
			Contract.Requires<ArgumentException>(typeof(TFunc).IsSubclassOf(typeof(Delegate)));
			Contract.Ensures(Contract.Result<TFunc>() != null);

			var type = typeof(T);
			var sigMethodInfo = typeof(TSig).GetMethod(K_DELEGATE_INVOKE_METHOD_NAME_);
			var methodParams = sigMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
			var method = type.GetMethod(methodName, bindingAttr, null, methodParams, null);

			if (method == null)
				throw new InvalidOperationException(string.Format(KSoft.Util.InvariantCultureInfo,
					"Couldn't find a method in {0} named '{1}' ({2})",
					type, methodName, bindingAttr));

			var paramThis =Expr.Parameter(type, K_THIS_NAME_);
			// have to convert it to a collection, else a different set of Parameter objects will be created for Call and the Lambda
			var @params =	(from param_type in methodParams
							select Expr.Parameter(param_type)).ToArray();
			var call =		Expr.Call(paramThis, method, @params);

			var paramsLamda = new System.Linq.Expressions.ParameterExpression[methodParams.Length+1];
			{
				paramsLamda[0] = paramThis;
				int i = 1;
				foreach(var param in @params)
					paramsLamda[i++] = param;
			}
			return Expr.Lambda<TFunc>(call, paramsLamda).Compile();
		}

		#region GenerateConstructorFunc
		// Inspired by: http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/

		static TFunc GenerateConstructorFuncImpl<TFunc>(
			Type type,
			Reflect.BindingFlags bindingAttr)
			where TFunc : class
		{
			var funcType = typeof(TFunc);
			var funcMethodInfo = funcType.GetMethod(K_DELEGATE_INVOKE_METHOD_NAME_);
			#region func_method_info validation
			if (!funcMethodInfo.ReturnType.IsAssignableFrom(type))
			{
				string msg = string.Format(KSoft.Util.InvariantCultureInfo,
					"Generation failed: {0} returns a {1} which isn't assignable from {2}",
					funcType, funcMethodInfo.ReturnType, type);
				throw new InvalidOperationException(msg);
			}
			#endregion

			var funcParams = funcMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
			var ctor = type.GetConstructor(bindingAttr, null, funcParams, null);
			#region ctor validation
			if (ctor == null)
			{
				var paramNames = new System.Text.StringBuilder();
				foreach (var paramType in funcParams)
				{
					if (paramNames.Length != 0)
						paramNames.Append(',');
					paramNames.Append(paramType.Name);
				}
				if (paramNames.Length == 0)
					paramNames.Append("<no-parameters>");

				string msg = string.Format(KSoft.Util.InvariantCultureInfo,
					"Generation failed: {0} has no ctor which matches the bindings '{1}' and takes the following parameter types: {2}",
					type, bindingAttr, paramNames);

				throw new InvalidOperationException(msg);
			}
			#endregion

			// have to convert it to a collection, else a different set of Parameter objects will be created for New and the Lambda
			var callParams = (from param_type in funcParams
							  select Expr.Parameter(param_type)).ToArray();

			var newExpr = Expr.New(ctor, callParams);
			var lambda = Expr.Lambda<TFunc>(newExpr, callParams);

			return lambda.Compile();
		}

		public static TFunc GenerateConstructorFunc<T, TFunc>(
			Type type,
			Reflect.BindingFlags bindingAttr = Reflect.BindingFlags.Public | Reflect.BindingFlags.Instance)
			where TFunc : class
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentException>(type.IsSubclassOf(typeof(T)));
			Contract.Requires<ArgumentException>(typeof(TFunc).IsSubclassOf(typeof(Delegate)));
			Contract.Ensures(Contract.Result<TFunc>() != null);

			return GenerateConstructorFuncImpl<TFunc>(type, bindingAttr);
		}

		public static TFunc GenerateConstructorFunc<T, TFunc>(
			Reflect.BindingFlags bindingAttr = Reflect.BindingFlags.Public | Reflect.BindingFlags.Instance)
			where TFunc : class
		{
			Contract.Requires<ArgumentException>(typeof(TFunc).IsSubclassOf(typeof(Delegate)));
			Contract.Ensures(Contract.Result<TFunc>() != null);

			var type = typeof(T);

			return GenerateConstructorFuncImpl<TFunc>(type, bindingAttr);
		}
		#endregion
	};
}

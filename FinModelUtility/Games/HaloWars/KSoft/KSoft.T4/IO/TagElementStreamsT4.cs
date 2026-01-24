using System;
using System.Collections.Generic;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4
{
	public enum TagElementStreamSubjectType
	{
		CURSOR,
		ELEMENT,
		ELEMENT_OPT,
		ATTRIBUTE,
		ATTRIBUTE_OPT,
	};

	public static class TagElementStreamsT4
	{
		public sealed class OperationDefinition
		{
			public TagElementStreamSubjectType SubjectType { get; private set; }
			public string Name { get; private set; }
			public bool SupportsOptional { get; private set; }

			public OperationDefinition(TagElementStreamSubjectType type, bool supportsOpt = true)
			{
				this.SubjectType = type;
				this.Name = type.ToString();
				this.SupportsOptional = supportsOpt;
			}
		};
		static readonly OperationDefinition KCursorOp = new OperationDefinition(TagElementStreamSubjectType.CURSOR, false);
		static readonly OperationDefinition KElementOp = new OperationDefinition(TagElementStreamSubjectType.ELEMENT);
		static readonly OperationDefinition KAttributeOp = new OperationDefinition(TagElementStreamSubjectType.ATTRIBUTE);

		public static IEnumerable<OperationDefinition> Operations { get {
			yield return KCursorOp;
			yield return KElementOp;
			yield return KAttributeOp;
		} }

		public static IEnumerable<PrimitiveCodeDefinition> SerializableTypesMisc { get {
			yield return PrimitiveDefinitions.KString;
			yield return PrimitiveDefinitions.KChar;
			yield return PrimitiveDefinitions.KBool;

			yield return PrimitiveDefinitions.KSingle;
			yield return PrimitiveDefinitions.KDouble;
		} }

		public static IEnumerable<NumberCodeDefinition> SerializableTypesIntegers { get {
			foreach (var numType in PrimitiveDefinitions.Numbers)
				if (numType.IsInteger)
					yield return numType;
		} }

		public static IEnumerable<PrimitiveCodeDefinition> SerializableTypesSpecial { get {
			yield return PrimitiveDefinitions.KKGuid;
		} }

		public static void GenerateObjectPropertyStreamMethod(TextTemplating.TextTransformation ttFile,
			TagElementStreamSubjectType subject, PrimitiveCodeDefinition codeDef, bool hasTNameParam = true)
		{
			if (ttFile == null)
				throw new ArgumentNullException(nameof(ttFile));
			if (codeDef == null)
				throw new ArgumentNullException(nameof(codeDef));

			ttFile.PushIndent("\t");
			ttFile.PushIndent("\t");

			bool isOpt =
				subject == TagElementStreamSubjectType.ELEMENT_OPT ||
				subject == TagElementStreamSubjectType.ATTRIBUTE_OPT
				;

			string methodName = subject.ToString();
			ttFile.WriteLine(
				"public {5} Stream{0}<T>({2} T theObj, Exprs.Expression<Func<T, {1} >> propExpr {3} {4})",
				methodName,
				codeDef.Keyword,
				hasTNameParam.UseStringOrEmpty("TName name,"),
				isOpt.UseStringOrEmpty(", Predicate<{0}> predicate = null", codeDef.Keyword),
				codeDef.IsInteger.UseStringOrEmpty(", NumeralBase numBase=kDefaultRadix"),
				!isOpt ? "void" : "bool"
			);

			ttFile.WriteLine("{");
			ttFile.PushIndent("\t");

			if (hasTNameParam)
			{
				ttFile.WriteLine("Contract.Requires(ValidateNameArg(name));");
				ttFile.WriteLine("");
			}

			if (isOpt)
			{
				ttFile.WriteLine("if (predicate == null)");
				using (var cb1 = ttFile.EnterCodeBlock())
					ttFile.WriteLine("predicate = x => true;");

				ttFile.NewLine();
				ttFile.WriteLine("bool executed = false;");
			}

			ttFile.WriteLine("var property = Reflection.Util.PropertyFromExpr(propExpr);");
			ttFile.WriteLine("if (IsReading)");
			using (var cb1 = ttFile.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				ttFile.WriteLine("var value = default( {0} );", codeDef.Keyword);
				ttFile.WriteLine("{1}Read{0}({2} ref value {3});",
					methodName,
					isOpt.UseStringOrEmpty("executed = "),
					hasTNameParam.UseStringOrEmpty("name,"),
					codeDef.IsInteger.UseStringOrEmpty(", numBase")
				);
				if (isOpt)
					ttFile.WriteLine("if (executed)");
				using (var cb2 = ttFile.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
					ttFile.WriteLine("property.SetValue(theObj, value, null);");
			}

			ttFile.WriteLine("else if (IsWriting)");
			using (var cb1 = ttFile.EnterCodeBlock())
			{
				ttFile.WriteLine("{2}Write{0}{3}({4} ({1})property.GetValue(theObj, null) {5}{6});",
					methodName,									// 0
					codeDef.Keyword,								// 1
					isOpt.UseStringOrEmpty("executed = "),			// 2
					isOpt.UseStringOrEmpty("OnTrue"),				// 3
					hasTNameParam.UseStringOrEmpty("name,"),		// 4
					isOpt.UseStringOrEmpty(", predicate"),			// 5
					codeDef.IsInteger.UseStringOrEmpty(", numBase")	// 6
				);
			};

			if (isOpt)
			{
				ttFile.NewLine();
				ttFile.WriteLine("return executed;");
			}

			ttFile.PopIndent();
			ttFile.WriteLine("}");

			ttFile.PopIndent();
			ttFile.PopIndent();
		}
	};
}

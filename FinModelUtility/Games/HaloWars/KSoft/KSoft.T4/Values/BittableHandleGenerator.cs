using System;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Values
{
	public enum BittableHandleKind
	{
		/// <summary>Handle is otherwise opaque</summary>
		UNDEFINED,
		/// <summary>Handle represents a value where NONE (-1) is the 'invalid' sentinel</summary>
		NONEABLE,
		/// <summary>Handle represents a value where NULL (0) is the 'invalid' sentinel</summary>
		NULLABLE,
		/// <summary>Handle is a wrapper for a void* value</summary>
		INT_PTR_WRAPPER,
	};

	public sealed class BittableHandleGenerator
	{
		#region Constants
		const string K_NONE_VALUE_CODE_NAME_ = "KSoft.TypeExtensions.kNone";
		const string K_NONE_FIELD_NAME_ = "None";

		const string K_NULL_FIELD_NAME_ = "Null";
		#endregion

		readonly TextTemplating.TextTransformation mFile_;
		readonly string mUnderlyingTypeName_;
		readonly string mStructName_;

		public string BackingFieldName { get; set; }

		/// <summary>Is this handle only internally accessed, or is it public (default value)?</summary>
		public bool IsInternal { get; set; }
		/// <summary>Should the type decl include a 'partial' specification? (default is true)</summary>
		public bool IsPartial { get; set; }

		public BittableHandleKind Kind { get; set; }

		public bool SupportsIComparable { get; set; }
		public bool SupportsIEquatable { get; set; }

		public string CtorFromValueTypeName { get; set; }
		public string CtorFromValueParamName { get; set; }

		#region Ctors
		BittableHandleGenerator()
		{
			this.BackingFieldName = "mValue";

			this.IsPartial = true;

			this.Kind = BittableHandleKind.UNDEFINED;

			this.SupportsIComparable = true;
			this.SupportsIEquatable = true;

			this.CtorFromValueTypeName = null;
			this.CtorFromValueParamName = "value";
		}
		public BittableHandleGenerator(TextTemplating.TextTransformation ttFile,
			string underlyingTypeName, string structName)
			: this()
		{
			this.mFile_ = ttFile;
			this.mUnderlyingTypeName_ = underlyingTypeName;
			this.mStructName_ = structName;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
		public BittableHandleGenerator(TextTemplating.TextTransformation ttFile,
			NumberCodeDefinition underlyingType, string structName)
			: this(ttFile, underlyingType.Code.ToString(), structName)
		{
			if (underlyingType == null)
				throw new ArgumentNullException(nameof(underlyingType));
		}

		public static BittableHandleGenerator ForIntPtr(TextTemplating.TextTransformation ttFile,
			string structName)
		{
			return new BittableHandleGenerator(ttFile, "IntPtr", structName)
			{
				Kind = BittableHandleKind.INT_PTR_WRAPPER,

				SupportsIComparable = false,

				CtorFromValueTypeName = "IntPtr",
				CtorFromValueParamName = "pointer",
			};
		}
		#endregion

		#region Generate type declaration
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1822", Justification="Remove supression once this does something")]
		void WriteDeclAttributes()
		{
			// #TODO_IMPLEMENT
		}
		void WriteDeclHeader()
		{
			string visibility = this.IsInternal
				? "internal"
				: "public";

			string partial = this.IsPartial
				? "partial "
				: "";

			this.mFile_.WriteLine("{0} {1} struct {2}", visibility, partial, this.mStructName_);
		}
		#region GenerateDeclInheritance
		// Curiously recurring template pattern
		string BuildDeclInheritanceTextCrtp(string genericClassName)
		{
			return string.Format(UtilT4.InvariantCultureInfo, "{0}<{1}>", genericClassName, this.mStructName_);
		}
		int WriteDeclInheritanceEntry(int position, bool entryExists, string entryText)
		{
			if (entryExists)
			{
				string prefix = position > 0
					? ","
					: ":";

				this.mFile_.WriteLine("{0} {1}", prefix, entryText);
				position++;
			}

			return position;
		}
		void WriteDeclInheritance()
		{
			int entryPos = 0;
			// indent the inheritance entries +1 more than the type decl (ie, public struct...)
			using (this.mFile_.EnterCodeBlock())
			{
				entryPos = this.WriteDeclInheritanceEntry(entryPos,
				                                           this.SupportsIComparable,
				                                           this.BuildDeclInheritanceTextCrtp("IComparable"));

				entryPos = this.WriteDeclInheritanceEntry(entryPos,
				                                           this.SupportsIEquatable,
				                                           this.BuildDeclInheritanceTextCrtp("IEquatable"));
			}
		}
		#endregion
		public void GenerateDecl()
		{
			// indent the decl by 1. it is assumed the decl is nested in a single namespace declaration (like this codegen class)
			using (this.mFile_.EnterCodeBlock())
			{
				this.WriteDeclAttributes();
				this.WriteDeclHeader();
				this.WriteDeclInheritance();
			}
		}
		#endregion

		public void GenerateBackingFieldDecl()
		{
			this.mFile_.WriteLine("{0} {1};",
			                     this.mUnderlyingTypeName_,
			                     this.BackingFieldName);

			this.mFile_.NewLine();
		}

		// Use non-default CtorFromValue... inputs when the 'value' is of a different bit size than the underlying type
		// eg, underlying type is Int16, but we construct from Int32 inputs
		public void GenerateCtorFromValueMethod()
		{
			if (this.CtorFromValueTypeName == null)
				this.CtorFromValueTypeName = this.mUnderlyingTypeName_;

			this.mFile_.WriteLine("public {0}({1} {2})",
			                     this.mStructName_,
			                     this.CtorFromValueTypeName,
			                     this.CtorFromValueParamName);

			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				this.mFile_.WriteLine("{0} = ({1}){2};",
				                     this.BackingFieldName,
				                     this.mUnderlyingTypeName_,
				                     this.CtorFromValueParamName);
			}

			this.mFile_.NewLine();
		}

		#region Generate cast to and from methods
		void WriteCastToUnderlyingTypeMethod()
		{
			const string kParamName = "handle";

			this.mFile_.WriteLine("public static implicit operator {0}({1} {2})",
			                     this.mUnderlyingTypeName_,
			                     this.mStructName_, kParamName);

			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				this.mFile_.WriteLine("return {0}.{1}",
				                     kParamName,
				                     this.BackingFieldName);
			}
		}
		void WriteCastFromUnderlyingTypeMethod()
		{
			const string kParamName = "value";

			this.mFile_.WriteLine("public static implicit operator {0}({1} {2})",
			                     this.mStructName_,
			                     this.mUnderlyingTypeName_, kParamName);

			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				this.mFile_.WriteLine("return new {0}({1});",
				                     this.mStructName_, kParamName);
			}
		}
		public void GenerateStockCastMethods()
		{
			this.WriteCastToUnderlyingTypeMethod();
			this.WriteCastFromUnderlyingTypeMethod();
			this.mFile_.NewLine();
		}
		#endregion

		#region Generate equality methods
		void WriteEqualityOperatorMethod(string operatorSymbol)
		{
			Debug.Assert(this.SupportsIEquatable,
				"Trying to generate handle without IEquatable support, but with equality operators (this is non-optimal)",
				"in {0}",
				this.mStructName_);

			this.mFile_.WriteLine("public static bool operator {0}({1} x, {1} y)",
			                     operatorSymbol,
			                     this.mStructName_);

			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				this.mFile_.WriteLine("return x.Equals(y) {0} true",
				                     operatorSymbol);
			}
		}
		public void GenerateEqualityOperatorMethods()
		{
			this.WriteEqualityOperatorMethod("==");
			this.WriteEqualityOperatorMethod("!=");
			this.mFile_.NewLine();
		}
		#endregion

		#region Generate NONE utilities
		void WriteNoneField(string fieldName = K_NONE_FIELD_NAME_)
		{
			this.mFile_.WriteLine("public static readonly {0} {1} = new {0}({2});",
			                     this.mStructName_, fieldName, K_NONE_VALUE_CODE_NAME_);
		}
		void WriteNoneableProperties()
		{
			this.mFile_.WriteLine("public bool IsNone { get { return {0} == {1}; } }",
			                     this.BackingFieldName, K_NONE_VALUE_CODE_NAME_);

			this.mFile_.WriteLine("public bool IsNotNone { get { return {0} != {1}; } }",
			                     this.BackingFieldName, K_NONE_VALUE_CODE_NAME_);
		}
		public void GenereateNoneableMembers(string noneFieldName = K_NONE_FIELD_NAME_)
		{
			if (this.Kind == BittableHandleKind.NONEABLE)
			{
				this.WriteNoneField(noneFieldName);
				this.mFile_.NewLine();

				this.WriteNoneableProperties();
				this.mFile_.NewLine();
			}
		}
		#endregion

		#region Generate NULL utilities
		void WriteNullField(string fieldName = K_NULL_FIELD_NAME_)
		{
			this.mFile_.WriteLine("public static readonly {0} {1} = new {0}( 0 );",
			                     this.mStructName_, fieldName);
		}
		void WriteNullableProperties()
		{
			this.mFile_.WriteLine("public bool IsNull { get { return {0} == 0; } }",
			                     this.BackingFieldName);

			this.mFile_.WriteLine("public bool IsNotNull { get { return {0} != 0; } }",
			                     this.BackingFieldName);
		}
		public void GenereateNullableMembers(string nullFieldName = K_NULL_FIELD_NAME_)
		{
			if (this.Kind == BittableHandleKind.NULLABLE)
			{
				this.WriteNullField(nullFieldName);
				this.mFile_.NewLine();

				this.WriteNullableProperties();
				this.mFile_.NewLine();
			}
		}
		#endregion

		#region Generate object overrides
		void WriteGetHashCodeOverride()
		{
			this.mFile_.WriteLine("public override int GetHashCode()");

			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				this.mFile_.WriteLine("{0}.GetHashCode();",
				                     this.BackingFieldName);
			}
		}
		void WriteEqualsOverride()
		{
			this.mFile_.WriteLine("public override bool Equals(object obj)");

			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
			{
				this.mFile_.WriteLine("if (!(obj is {0}))",
				                     this.mStructName_);
				using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
				{
					this.mFile_.WriteLine("return false;");
				}

				this.mFile_.WriteLine("return Equals( ({0})obj );",
				                     this.mStructName_);
			}
		}
		public void GenerateObjectMethodOverrides()
		{
			this.WriteGetHashCodeOverride();
			this.mFile_.NewLine();

			this.WriteEqualsOverride();
			this.mFile_.NewLine();
		}
		#endregion

		#region Generate Interface impls
		#region IComparable<struct> Members
		void GenerateIComparableImpl()
		{
			const string kInterfaceName = "IComparable";

			this.mFile_.WriteLine("#region {0}<{1}> Members",
			                     kInterfaceName,
			                     this.mStructName_);
			{
				const string kReturnType = "int";

				this.mFile_.WriteLine("public {0} {1}({2} other)",
				                     kReturnType, "ComapreTo",
				                     this.mStructName_);

				using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
				{
					this.mFile_.WriteLine("return ({0})({1} - other.{1});",
					                     kReturnType,
					                     this.BackingFieldName);
				}
			}
			this.mFile_.WriteLine("#endregion");
		}
		#endregion

		#region IEquatable<struct> Members
		void GenerateIEquatableImpl()
		{
			const string kInterfaceName = "IEquatable";

			this.mFile_.WriteLine("#region {0}<{1}> Members",
			                     kInterfaceName,
			                     this.mStructName_);
			{
				const string kReturnType = "bool";

				this.mFile_.WriteLine("public {0} {1}({2} other)",
				                     kReturnType, "Equals",
				                     this.mStructName_);

				using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS))
				{
					this.mFile_.WriteLine("return {0} == other.{0};",
					                     this.BackingFieldName);
				}
			}
			this.mFile_.WriteLine("#endregion");
		}
		#endregion

		public void GenerateSupportedInterfaceImplementations()
		{
			if (this.SupportsIComparable)
			{
				this.GenerateIComparableImpl();
				this.mFile_.NewLine();
			}

			if (this.SupportsIEquatable)
			{
				this.GenerateIEquatableImpl();
				this.mFile_.NewLine();
			}
		}
		#endregion

		public void GenerateDefaultBody()
		{
			using (this.mFile_.EnterCodeBlock(TextTransformationCodeBlockType.BRACKETS_STATEMENT))
			{
				switch (this.Kind)
				{
				case BittableHandleKind.NONEABLE:
					this.WriteNoneField();
					break;
				case BittableHandleKind.NULLABLE:
					this.WriteNullField();
					break;
				}
				if (this.Kind != BittableHandleKind.UNDEFINED)
					this.mFile_.NewLine();

				this.GenerateBackingFieldDecl();

				this.GenerateCtorFromValueMethod();

				this.GenerateStockCastMethods();
				this.GenerateEqualityOperatorMethods();

				switch (this.Kind)
				{
				case BittableHandleKind.NONEABLE:
					this.WriteNoneableProperties();
					break;
				case BittableHandleKind.NULLABLE:
					this.WriteNullableProperties();
					break;
				}
				if (this.Kind != BittableHandleKind.UNDEFINED)
					this.mFile_.NewLine();

				this.GenerateObjectMethodOverrides();

				this.GenerateSupportedInterfaceImplementations();
			}
		}

		public void GenerateDefinition()
		{
			this.GenerateDecl();
			this.GenerateDefaultBody();
		}
	};
}

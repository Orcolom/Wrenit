using System;
using System.Collections.Generic;

namespace Wrenit.Utilities
{
	public enum SignatureStyle
	{
		Signature,
		Implementation,
		ForeignImplementation,
	}
	
	struct WrenSignature
	{
		#region Struct

		public readonly (int, int) Arguments;
		public readonly string Format;
		public readonly string ForcedName;
		public readonly Func<bool, string> CustomValue;

		private WrenSignature(string format, (int, int) arguments, string customName = null,
			Func<bool, string> customValue = null)
		{
			ForcedName = customName;
			Arguments = arguments;
			Format = format;
			CustomValue = customValue;
		}
		
		#endregion
		
		// 0 = custom word
		// 1 = name
		// 2 = arguments list
		// 3 = extra arguments list
		private const string FormatCustomNameArg = "{0} {1}({2})";
		private const string FormatNameArg = "{1}({2})";
		private const string FormatName = "{1}";

		public static readonly Dictionary<WrenMethodType, WrenSignature> Signatures =
			new Dictionary<WrenMethodType, WrenSignature>()
			{
				{WrenMethodType.Method, new WrenSignature(FormatNameArg, (0, -1))},
				{
					WrenMethodType.StaticMethod,
					new WrenSignature(FormatCustomNameArg, (0, -1), customValue: implement => implement ? "static" : "")
				},
				{
					WrenMethodType.Construct,
					new WrenSignature(FormatCustomNameArg, (0, -1), customValue: implement => implement ? "construct" : "init")
				},
				{WrenMethodType.RangeInclusive, new WrenSignature(FormatNameArg, (1, 1), "..")},
				{WrenMethodType.RangeExclusive, new WrenSignature(FormatNameArg, (1, 1), "...")},
				{WrenMethodType.Times, new WrenSignature(FormatNameArg, (1, 1), "*")},
				{WrenMethodType.Divide, new WrenSignature(FormatNameArg, (1, 1), "/")},
				{WrenMethodType.Modulo, new WrenSignature(FormatNameArg, (1, 1), "%")},
				{WrenMethodType.Plus, new WrenSignature(FormatNameArg, (1, 1), "+")},
				{WrenMethodType.Minus, new WrenSignature(FormatNameArg, (1, 1), "-")},
				{WrenMethodType.BitwiseLeftShift, new WrenSignature(FormatNameArg, (1, 1), "<<")},
				{WrenMethodType.BitwiseRightShift, new WrenSignature(FormatNameArg, (1, 1), ">>")},
				{WrenMethodType.BitwiseXor, new WrenSignature(FormatNameArg, (1, 1), "^")},
				{WrenMethodType.BitwiseOr, new WrenSignature(FormatNameArg, (1, 1), "|")},
				{WrenMethodType.BitwiseAnd, new WrenSignature(FormatNameArg, (1, 1), "&")},
				{WrenMethodType.SmallerThen, new WrenSignature(FormatNameArg, (1, 1), "<")},
				{WrenMethodType.SmallerEqualThen, new WrenSignature(FormatNameArg, (1, 1), "<=")},
				{WrenMethodType.BiggerThen, new WrenSignature(FormatNameArg, (1, 1), ">")},
				{WrenMethodType.BiggerEqualThen, new WrenSignature(FormatNameArg, (1, 1), ">=")},
				{WrenMethodType.Equal, new WrenSignature(FormatNameArg, (1, 1), "==")},
				{WrenMethodType.NotEqual, new WrenSignature(FormatNameArg, (1, 1), "!=")},
				{WrenMethodType.Is, new WrenSignature(FormatNameArg, (1, 1), "is")},
				{WrenMethodType.FieldGetter, new WrenSignature(FormatName, (0, 0))},
				{WrenMethodType.Inverse, new WrenSignature(FormatName, (0, 0), "-")},
				{WrenMethodType.Not, new WrenSignature(FormatName, (0, 0), "!")},
				{WrenMethodType.Tilda, new WrenSignature(FormatName, (0, 0), "~")},
				{WrenMethodType.FieldSetter, new WrenSignature("{1}=({2})", (1, 1))},
				{
					WrenMethodType.SubScriptSetter,
					new WrenSignature("[{2}]=({0})", (1, -1), customValue: implement => implement ? "value" : "_")
				},
				{WrenMethodType.SubScriptGetter, new WrenSignature("[{2}]", (1, -1))},
			};

		/// <summary>
		/// create a wren signature based on its type name and arguments.
		/// Will correct a name and argument if needed
		/// </summary>
		/// <param name="type">type of the method signature</param>
		/// <param name="name">name of the method</param>
		/// <param name="argumentCount">amount of arguments wanted</param>
		/// <param name="style">style of signature</param>
		/// <returns>the wren style signature</returns>
		public static string CreateSignature(WrenMethodType type, string name, int argumentCount,
			SignatureStyle style = SignatureStyle.Signature)
		{
			argumentCount = CorrectArgumentCount(type, argumentCount);
			string arguments = CreateArgumentList(argumentCount, style != SignatureStyle.Signature);
			return CreateSignature(type, name, arguments, style);
		}

		internal static string CreateSignature(WrenMethodType type, string name, List<WrenSlotAttribute> slots)
		{
			string arguments = CreateArgumentList(slots);
			return CreateSignature(type, name, arguments, SignatureStyle.ForeignImplementation);
		}
		
		private static string CreateSignature(WrenMethodType type, string name, string arguments, SignatureStyle style)
		{
			WrenSignature signature = WrenSignature.Signatures[type];
			
			if (string.IsNullOrEmpty(signature.ForcedName) == false)
			{
				name = signature.ForcedName;
			}

			string extra = null;
			if (signature.CustomValue != null)
			{
				extra = signature.CustomValue.Invoke(style != SignatureStyle.Signature);
			}

			string result = string.Format(signature.Format, extra, name, arguments).Trim();

			if (style == SignatureStyle.ForeignImplementation)
			{
				result = $"foreign {result}";
			}
			
			return result;
		}

		internal static int CorrectArgumentCount(WrenMethodType type, int count)
		{
			WrenSignature signature = WrenSignature.Signatures[type];

				if (count < signature.Arguments.Item1) count = signature.Arguments.Item1;
				
				if (signature.Arguments.Item2 != -1)
				{
					return count > signature.Arguments.Item2 ? signature.Arguments.Item2 : count;
				}

				return count;
		}
		
		/// <summary>
		/// creates an argument list string 
		/// </summary>
		/// <param name="argumentCount">amount of wanted arguments</param>
		/// <param name="implement">do an argument list for an implementation signature</param>
		internal static string CreateArgumentList(int argumentCount, bool implement)
		{
			string arguments = null;
			for (int i = 0; i < argumentCount; i++)
			{
				arguments += implement ? (char) ('a' + i) : '_';
				if (i + 1 < argumentCount) arguments += ',';
			}

			return arguments;
		}
				
		/// <summary>
		/// creates an argument list string 
		/// </summary>
		/// <param name="argumentCount">amount of wanted arguments</param>
		/// <param name="implement">do an argument list for an implementation signature</param>
		internal static string CreateArgumentList(List<WrenSlotAttribute> slots)
		{
			string arguments = null;
			for (int i = 0; i < slots.Count; i++)
			{
				arguments += slots[i].Name;
				if (i + 1 < slots.Count) arguments += ',';
			}

			return arguments;
		}

	}
}

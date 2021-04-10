namespace Wrenit.Utilities
{
	public enum WrenMethodType
	{
		Method, // foo()
		StaticMethod, // static foo()
		Construct, // init foo()

		FieldGetter, // foo
		FieldSetter, // foo=()
		SubScriptGetter, // []
		SubScriptSetter, // []=()

		Not, // !
		Inverse, // -
		Tilda, // ~

		RangeInclusive, // ..		?RangeExlusive
		RangeExclusive, // ...	?RangeInclusive (check docs for dot count)

		Times, // * 
		Divide, // / 
		Modulo, // % 
		Plus, // + 
		Minus, // - 

		BitwiseLeftShift, // <<		BitwiseLeft
		BitwiseRightShift, // >>		BitwiseRight
		BitwiseXor, // ^		BitwiseXor
		BitwiseOr, // |		BitwiseOr
		BitwiseAnd, // &		BitwiseAnd

		SmallerThen,
		SmallerEqualThen,
		BiggerThen,
		BiggerEqualThen,
		Equal,
		NotEqual,

		Is,
	}
}

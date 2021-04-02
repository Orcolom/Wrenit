using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{
	public enum WrenErrorType
	{
		WREN_ERROR_COMPILE,
		WREN_ERROR_RUNTIME,
		WREN_ERROR_STACK_TRACE
	}
	public enum WrenInterpretResult
	{
		SUCCESS,
		WREN_ERROR_COMPILE,
		WREN_ERROR_RUNTIME,
	}
}

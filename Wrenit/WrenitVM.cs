using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{
	public enum WrenitResult
	{
		Unknown,
		Success,
		CompileError,
		RuntimeError,
		StackTrace,
		Disposed,
	}

	public class WrenitVM : IDisposable
	{
		private IntPtr _vm;

		public bool IsAlive => _vm != IntPtr.Zero;

		#region Events

		public delegate void WrenitWrite(WrenitVM vm, string text);
		public event WrenitWrite WriteEvent;
		private void InvokeWriteEvent(string text)
		{
			WriteEvent?.Invoke(this, text);
		}

		public delegate void WrenitError(WrenitVM vm, WrenitResult result, string module, int line, string message);
		public event WrenitError ErrorEvent;
		private void InvokeErrorEvent(WrenitResult result, string module, int line, string message)
		{
			ErrorEvent?.Invoke(this, result, module, line, message);
		}

		#endregion

		public WrenitVM() : this(WrenitConfig.CreateInitialized()) { }

		public WrenitVM(WrenitConfig config)
		{
			WrenConfig wrenConfig = WrenitConfig.ToWren(config);
			wrenConfig.WrenWriteFn = OnWrenWrite;
			wrenConfig.WrenErrorFn = OnWrenError;
			_vm = WrenImport.xWrenNewVM(wrenConfig);
		}

		~WrenitVM()
		{
			WrenImport.xWrenFreeVM(_vm);
			_vm = IntPtr.Zero;
		}

		public void Dispose()
		{
			if (IsAlive == false) return;

			WrenImport.xWrenFreeVM(_vm);
			_vm = IntPtr.Zero;

			GC.SuppressFinalize(this);
		}

		public WrenitResult Interpret(string module, string source)
		{
			if (IsAlive == false)
				throw new ObjectDisposedException("Tried to Interpret module in disposed VM");

			WrenInterpretResult error = WrenImport.xWrenInterpret(_vm, module, source);
			return ProcessEnum(error);
		}

		private WrenitResult ProcessEnum(WrenInterpretResult error)
		{
			switch (error)
			{
				default:
					return WrenitResult.Unknown;

				case WrenInterpretResult.SUCCESS:
					return WrenitResult.Success;

				case WrenInterpretResult.WREN_ERROR_COMPILE:
					return WrenitResult.CompileError;

				case WrenInterpretResult.WREN_ERROR_RUNTIME:
					return WrenitResult.RuntimeError;
			}
		}

		private WrenitResult ProcessEnum(WrenErrorType error)
		{
			switch(error)
			{
				default:
					return WrenitResult.Unknown;

				case WrenErrorType.WREN_ERROR_COMPILE:
					return WrenitResult.CompileError;
				
				case WrenErrorType.WREN_ERROR_RUNTIME:
					return WrenitResult.RuntimeError;

				case WrenErrorType.WREN_ERROR_STACK_TRACE:
					return WrenitResult.StackTrace;
			}
		}

		private void OnWrenWrite(IntPtr vm, string text) => InvokeWriteEvent(text);
		private void OnWrenError(IntPtr vm, WrenErrorType type, string module, int line, string message) => InvokeErrorEvent(ProcessEnum(type), module, line, message);
	}
}

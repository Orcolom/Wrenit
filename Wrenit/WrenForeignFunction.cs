using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Wrenit
{
	public delegate void WrenForeignMethod0(WrenVm vm);

	public delegate void WrenForeignMethod1(WrenVm vm, IWrenSlot s0);

	public delegate void WrenForeignMethod2(WrenVm vm, IWrenSlot s0, IWrenSlot s1);

	public delegate void WrenForeignMethod3(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2);

	public delegate void WrenForeignMethod4(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3);

	public delegate void WrenForeignMethod5(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4);

	public delegate void WrenForeignMethod6(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5);

	public delegate void WrenForeignMethod7(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6);

	public delegate void WrenForeignMethod8(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7);

	public delegate void WrenForeignMethod9(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8);

	public delegate void WrenForeignMethod10(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9);

	public delegate void WrenForeignMethod11(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9, IWrenSlot s10);

	public delegate void WrenForeignMethod12(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9, IWrenSlot s10, IWrenSlot s11);

	public delegate void WrenForeignMethod13(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9, IWrenSlot s10, IWrenSlot s11, IWrenSlot s12);

	public delegate void WrenForeignMethod14(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9, IWrenSlot s10, IWrenSlot s11, IWrenSlot s12,
		IWrenSlot s13);

	public delegate void WrenForeignMethod15(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9, IWrenSlot s10, IWrenSlot s11, IWrenSlot s12,
		IWrenSlot s13, IWrenSlot s14);

	public delegate void WrenForeignMethod16(WrenVm vm, IWrenSlot s0, IWrenSlot s1, IWrenSlot s2, IWrenSlot s3, IWrenSlot s4,
		IWrenSlot s5, IWrenSlot s6, IWrenSlot s7, IWrenSlot s8, IWrenSlot s9, IWrenSlot s10, IWrenSlot s11, IWrenSlot s12,
		IWrenSlot s13, IWrenSlot s14, IWrenSlot s15);

	/// <summary>
	/// defines how to create and finalize a foreign class
	/// </summary>
	public class WrenForeignMethod
	{
		private readonly int _parameters;
		private readonly Delegate _method;
		private readonly WrenSlot[] _slots;

		internal WrenForeignMethod(MethodInfo method, int slots)
		{
			_parameters = slots;
			_parameters = slots;
			_slots = new WrenSlot[slots];

			FillSlots();

			switch (_parameters)
			{
				case 0:
					_method = CreateDelegate<WrenForeignMethod0>(method);
					break;

				case 1:
					_method = CreateDelegate<WrenForeignMethod1>(method);
					break;

				case 2:
					_method = CreateDelegate<WrenForeignMethod2>(method);
					break;

				case 3:
					_method = CreateDelegate<WrenForeignMethod3>(method);
					break;

				case 4:
					_method = CreateDelegate<WrenForeignMethod4>(method);
					break;

				case 5:
					_method = CreateDelegate<WrenForeignMethod5>(method);
					break;

				case 6:
					_method = CreateDelegate<WrenForeignMethod6>(method);
					break;

				case 7:
					_method= CreateDelegate<WrenForeignMethod7>(method);
					break;

				case 8:
					_method = CreateDelegate<WrenForeignMethod8>(method);
					break;

				case 9:
					_method = CreateDelegate<WrenForeignMethod9>(method);
					break;

				case 10:
					_method = CreateDelegate<WrenForeignMethod10>(method);
					break;

				case 11:
					_method = CreateDelegate<WrenForeignMethod11>(method);
					break;

				case 12:
					_method = CreateDelegate<WrenForeignMethod12>(method);
					break;

				case 13:
					_method = CreateDelegate<WrenForeignMethod13>(method);
					break;

				case 14:
					_method = CreateDelegate<WrenForeignMethod14>(method);
					break;

				case 15:
					_method = CreateDelegate<WrenForeignMethod15>(method);
					break;

				case 16:
					_method = CreateDelegate<WrenForeignMethod16>(method);
					break;
			}
		}

		public WrenForeignMethod(WrenForeignMethod0 method) : this(0, method) { }
		public WrenForeignMethod(WrenForeignMethod1 method) : this(1, method) { }
		public WrenForeignMethod(WrenForeignMethod2 method) : this(2, method) { }
		public WrenForeignMethod(WrenForeignMethod3 method) : this(3, method) { }
		public WrenForeignMethod(WrenForeignMethod4 method) : this(4, method) { }
		public WrenForeignMethod(WrenForeignMethod5 method) : this(5, method) { }
		public WrenForeignMethod(WrenForeignMethod6 method) : this(6, method) { }
		public WrenForeignMethod(WrenForeignMethod7 method) : this(7, method) { }
		public WrenForeignMethod(WrenForeignMethod8 method) : this(8, method) { }
		public WrenForeignMethod(WrenForeignMethod9 method) : this(9, method) { }
		public WrenForeignMethod(WrenForeignMethod10 method) : this(10, method) { }
		public WrenForeignMethod(WrenForeignMethod11 method) : this(11, method) { }
		public WrenForeignMethod(WrenForeignMethod12 method) : this(12, method) { }
		public WrenForeignMethod(WrenForeignMethod13 method) : this(13, method) { }
		public WrenForeignMethod(WrenForeignMethod14 method) : this(14, method) { }
		public WrenForeignMethod(WrenForeignMethod15 method) : this(15, method) { }
		public WrenForeignMethod(WrenForeignMethod16 method) : this(16, method) { }

		private WrenForeignMethod(int parameters, Delegate method)
		{
			_parameters = parameters;
			_method = method;
			_slots = new WrenSlot[parameters];
			FillSlots();
		}

		private void FillSlots()
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				// index 0 is the caller
				_slots[i] = new WrenSlot(i + 1);
			}
		}

		private static T CreateDelegate<T>(MethodInfo methodInfo)
			where T : Delegate
		{
			return (T) Delegate.CreateDelegate(typeof(T), methodInfo);
		}
		
		
		private static T As<T>(Delegate method)
			where T : Delegate
		{
			return (T) method;
		}

		private void SetVm(WrenVm vm)
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				_slots[i].Vm = vm;
			}
		}

		public void Invoke(WrenVm vm)
		{
			SetVm(vm);
			try
			{
				InvokeInternal(vm);
			}
			finally
			{
				SetVm(null); // make sure we dont keep the vm alive
			}
		}

		private void InvokeInternal(WrenVm vm)
		{
			vm.EnsureSlots(_parameters);
			switch (_parameters)
			{
				case 0:
					As<WrenForeignMethod0>(_method).Invoke(vm);
					break;

				case 1:
					As<WrenForeignMethod1>(_method).Invoke(vm, _slots[0]);
					break;

				case 2:
					As<WrenForeignMethod2>(_method).Invoke(vm, _slots[0], _slots[1]);
					break;

				case 3:
					As<WrenForeignMethod3>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2]);
					break;

				case 4:
					As<WrenForeignMethod4>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3]);
					break;

				case 5:
					As<WrenForeignMethod5>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4]);
					break;

				case 6:
					As<WrenForeignMethod6>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5]);
					break;

				case 7:
					As<WrenForeignMethod7>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6]);
					break;

				case 8:
					As<WrenForeignMethod8>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7]);
					break;

				case 9:
					As<WrenForeignMethod9>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8]);
					break;

				case 10:
					As<WrenForeignMethod10>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9]);
					break;

				case 11:
					As<WrenForeignMethod11>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10]);
					break;

				case 12:
					As<WrenForeignMethod12>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11]);
					break;

				case 13:
					As<WrenForeignMethod13>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12]);
					break;

				case 14:
					As<WrenForeignMethod14>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12], _slots[13]);
					break;

				case 15:
					As<WrenForeignMethod15>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12], _slots[13], _slots[14]);
					break;

				case 16:
					As<WrenForeignMethod16>(_method).Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12], _slots[13], _slots[14], _slots[15]);
					break;
			}
		}
	}
}

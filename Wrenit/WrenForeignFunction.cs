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

	[StructLayout(LayoutKind.Explicit)]
	public struct MethodUnion
	{
		[FieldOffset(0)]
		public WrenForeignMethod0 Method0;

		[FieldOffset(0)]
		public WrenForeignMethod1 Method1;

		[FieldOffset(0)]
		public WrenForeignMethod2 Method2;

		[FieldOffset(0)]
		public WrenForeignMethod3 Method3;

		[FieldOffset(0)]
		public WrenForeignMethod4 Method4;

		[FieldOffset(0)]
		public WrenForeignMethod5 Method5;

		[FieldOffset(0)]
		public WrenForeignMethod6 Method6;

		[FieldOffset(0)]
		public WrenForeignMethod7 Method7;

		[FieldOffset(0)]
		public WrenForeignMethod8 Method8;

		[FieldOffset(0)]
		public WrenForeignMethod9 Method9;

		[FieldOffset(0)]
		public WrenForeignMethod10 Method10;

		[FieldOffset(0)]
		public WrenForeignMethod11 Method11;

		[FieldOffset(0)]
		public WrenForeignMethod12 Method12;

		[FieldOffset(0)]
		public WrenForeignMethod13 Method13;

		[FieldOffset(0)]
		public WrenForeignMethod14 Method14;

		[FieldOffset(0)]
		public WrenForeignMethod15 Method15;

		[FieldOffset(0)]
		public WrenForeignMethod16 Method16;
	}

	/// <summary>
	/// defines how to create and finalize a foreign class
	/// </summary>
	public class WrenForeignMethod
	{
		private readonly int _parameters;
		private readonly MethodUnion _method;
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
					_method.Method0 = CreateDelegate<WrenForeignMethod0>(method);
					break;

				case 1:
					_method.Method1 = CreateDelegate<WrenForeignMethod1>(method);
					break;

				case 2:
					_method.Method2 = CreateDelegate<WrenForeignMethod2>(method);
					break;

				case 3:
					_method.Method3 = CreateDelegate<WrenForeignMethod3>(method);
					break;

				case 4:
					_method.Method4 = CreateDelegate<WrenForeignMethod4>(method);
					break;

				case 5:
					_method.Method5 = CreateDelegate<WrenForeignMethod5>(method);
					break;

				case 6:
					_method.Method6 = CreateDelegate<WrenForeignMethod6>(method);
					break;

				case 7:
					_method.Method7 = CreateDelegate<WrenForeignMethod7>(method);
					break;

				case 8:
					_method.Method8 = CreateDelegate<WrenForeignMethod8>(method);
					break;

				case 9:
					_method.Method9 = CreateDelegate<WrenForeignMethod9>(method);
					break;

				case 10:
					_method.Method10 = CreateDelegate<WrenForeignMethod10>(method);
					break;

				case 11:
					_method.Method11 = CreateDelegate<WrenForeignMethod11>(method);
					break;

				case 12:
					_method.Method12 = CreateDelegate<WrenForeignMethod12>(method);
					break;

				case 13:
					_method.Method13 = CreateDelegate<WrenForeignMethod13>(method);
					break;

				case 14:
					_method.Method14 = CreateDelegate<WrenForeignMethod14>(method);
					break;

				case 15:
					_method.Method15 = CreateDelegate<WrenForeignMethod15>(method);
					break;

				case 16:
					_method.Method16 = CreateDelegate<WrenForeignMethod16>(method);
					break;
			}
		}

		public WrenForeignMethod(WrenForeignMethod0 method) : this(0, new MethodUnion {Method0 = method}) { }
		public WrenForeignMethod(WrenForeignMethod1 method) : this(1, new MethodUnion {Method1 = method}) { }
		public WrenForeignMethod(WrenForeignMethod2 method) : this(2, new MethodUnion {Method2 = method}) { }
		public WrenForeignMethod(WrenForeignMethod3 method) : this(3, new MethodUnion {Method3 = method}) { }
		public WrenForeignMethod(WrenForeignMethod4 method) : this(4, new MethodUnion {Method4 = method}) { }
		public WrenForeignMethod(WrenForeignMethod5 method) : this(5, new MethodUnion {Method5 = method}) { }
		public WrenForeignMethod(WrenForeignMethod6 method) : this(6, new MethodUnion {Method6 = method}) { }
		public WrenForeignMethod(WrenForeignMethod7 method) : this(7, new MethodUnion {Method7 = method}) { }
		public WrenForeignMethod(WrenForeignMethod8 method) : this(8, new MethodUnion {Method8 = method}) { }
		public WrenForeignMethod(WrenForeignMethod9 method) : this(9, new MethodUnion {Method9 = method}) { }
		public WrenForeignMethod(WrenForeignMethod10 method) : this(10, new MethodUnion {Method10 = method}) { }
		public WrenForeignMethod(WrenForeignMethod11 method) : this(11, new MethodUnion {Method11 = method}) { }
		public WrenForeignMethod(WrenForeignMethod12 method) : this(12, new MethodUnion {Method12 = method}) { }
		public WrenForeignMethod(WrenForeignMethod13 method) : this(13, new MethodUnion {Method13 = method}) { }
		public WrenForeignMethod(WrenForeignMethod14 method) : this(14, new MethodUnion {Method14 = method}) { }
		public WrenForeignMethod(WrenForeignMethod15 method) : this(15, new MethodUnion {Method15 = method}) { }
		public WrenForeignMethod(WrenForeignMethod16 method) : this(16, new MethodUnion {Method16 = method}) { }

		private WrenForeignMethod(int parameters, MethodUnion method)
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
			catch
			{
				SetVm(null); // make sure we dont keep the vm alive
				throw;
			}
		}

		private void InvokeInternal(WrenVm vm)
		{
			vm.EnsureSlots(_parameters);
			switch (_parameters)
			{
				case 0:
					_method.Method0.Invoke(vm);
					break;

				case 1:
					_method.Method1.Invoke(vm, _slots[0]);
					break;

				case 2:
					_method.Method2.Invoke(vm, _slots[0], _slots[1]);
					break;

				case 3:
					_method.Method3.Invoke(vm, _slots[0], _slots[1], _slots[2]);
					break;

				case 4:
					_method.Method4.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3]);
					break;

				case 5:
					_method.Method5.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4]);
					break;

				case 6:
					_method.Method6.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5]);
					break;

				case 7:
					_method.Method7.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6]);
					break;

				case 8:
					_method.Method8.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7]);
					break;

				case 9:
					_method.Method9.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8]);
					break;

				case 10:
					_method.Method10.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9]);
					break;

				case 11:
					_method.Method11.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10]);
					break;

				case 12:
					_method.Method12.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11]);
					break;

				case 13:
					_method.Method13.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12]);
					break;

				case 14:
					_method.Method14.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12], _slots[13]);
					break;

				case 15:
					_method.Method15.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12], _slots[13], _slots[14]);
					break;

				case 16:
					_method.Method16.Invoke(vm, _slots[0], _slots[1], _slots[2], _slots[3], _slots[4], _slots[5], _slots[6],
						_slots[7], _slots[8], _slots[9], _slots[10], _slots[11], _slots[12], _slots[13], _slots[14], _slots[15]);
					break;
			}
		}
	}
}

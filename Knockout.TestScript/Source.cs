using KnockoutApi;

namespace Knockout.TestScript {
	public class Model1 {
		[KnockoutProperty]
		public int P1 { get; set; }

		[KnockoutProperty(true)]
		public int P2 { get; set; }

		[KnockoutProperty(false)]
		public int P3 { get; set; }

		public int P4 { get; set; }
	}

	[KnockoutModel]
	public class Model2 {
		[KnockoutProperty]
		public int P1 { get; set; }

		[KnockoutProperty(true)]
		public int P2 { get; set; }

		[KnockoutProperty(false)]
		public int P3 { get; set; }

		public int P4 { get; set; }

		public int P5 { get; set; }
	}

	public class C {
		public void M1() {
			var m = new Model1();

			m.P1 = 1;
			m.P2 = 2;
			m.P3 = 3;
			m.P4 = 4;
			int i1 = m.P1;
			int i2 = m.P2;
			int i3 = m.P3;
			int i4 = m.P4;

		}

		public void M2() {
			var m = new Model2();

			m.P1 = 1;
			m.P2 = 2;
			m.P3 = 3;
			m.P4 = 4;
			int i1 = m.P1;
			int i2 = m.P2;
			int i3 = m.P3;
			int i4 = m.P4;
		}
	}
}

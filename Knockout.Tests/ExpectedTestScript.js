(function() {
	'use strict';
	var $asm = {};
	global.Knockout = global.Knockout || {};
	global.Knockout.TestScript = global.Knockout.TestScript || {};
	ss.initAssembly($asm, 'Knockout.TestScript');
	////////////////////////////////////////////////////////////////////////////////
	// Knockout.TestScript.C
	var $Knockout_TestScript_C = function() {
	};
	$Knockout_TestScript_C.__typeName = 'Knockout.TestScript.C';
	global.Knockout.TestScript.C = $Knockout_TestScript_C;
	////////////////////////////////////////////////////////////////////////////////
	// Knockout.TestScript.Model1
	var $Knockout_TestScript_Model1 = function() {
		this.p1 = ko.observable(0);
		this.p2 = ko.observable(0);
		this.$1$P3Field = 0;
		this.$1$P4Field = 0;
	};
	$Knockout_TestScript_Model1.__typeName = 'Knockout.TestScript.Model1';
	global.Knockout.TestScript.Model1 = $Knockout_TestScript_Model1;
	////////////////////////////////////////////////////////////////////////////////
	// Knockout.TestScript.Model2
	var $Knockout_TestScript_Model2 = function() {
		this.p1 = ko.observable(0);
		this.p2 = ko.observable(0);
		this.$1$P3Field = 0;
		this.p4 = ko.observable(0);
		this.p5 = ko.observable(0);
	};
	$Knockout_TestScript_Model2.__typeName = 'Knockout.TestScript.Model2';
	global.Knockout.TestScript.Model2 = $Knockout_TestScript_Model2;
	ss.initClass($Knockout_TestScript_C, $asm, {
		m1: function() {
			var m = new $Knockout_TestScript_Model1();
			m.p1(1);
			m.p2(2);
			m.set_p3(3);
			m.set_p4(4);
			var i1 = m.p1();
			var i2 = m.p2();
			var i3 = m.get_p3();
			var i4 = m.get_p4();
		},
		m2: function() {
			var m = new $Knockout_TestScript_Model2();
			m.p1(1);
			m.p2(2);
			m.set_p3(3);
			m.p4(4);
			var i1 = m.p1();
			var i2 = m.p2();
			var i3 = m.get_p3();
			var i4 = m.p4();
		}
	});
	ss.initClass($Knockout_TestScript_Model1, $asm, {
		get_p3: function() {
			return this.$1$P3Field;
		},
		set_p3: function(value) {
			this.$1$P3Field = value;
		},
		get_p4: function() {
			return this.$1$P4Field;
		},
		set_p4: function(value) {
			this.$1$P4Field = value;
		}
	});
	ss.initClass($Knockout_TestScript_Model2, $asm, {
		get_p3: function() {
			return this.$1$P3Field;
		},
		set_p3: function(value) {
			this.$1$P3Field = value;
		}
	});
})();

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using GTA;
using GTA.Math;
using GTA.Native;

using GTSim;

public class GTTest : GTScript
{
	public GTTest() : base(new GTAccident(5.0f, 10.0f, 1.0f, 1), 8086)
	{
		this.KeyUp += OnKeyUp;
	}

	private void OnKeyUp(object sender, KeyEventArgs e)
	{
		;
	}
}

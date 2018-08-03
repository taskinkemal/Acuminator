﻿using System;
using System.Collections.Generic;
using System.Linq;



namespace Acuminator.Analyzers
{
	/// <summary>
	/// A class with string constatns representing diagnostic property names. They are used to pass custom data strings from diagnostic to code fix.
	/// </summary>
	internal static class DiagnosticProperty
	{
		
		/// <summary>
		/// The property responsible for code fix registration.
		/// </summary>
		public const string RegisterCodeFix = nameof(RegisterCodeFix);

		/// <summary>
		/// The property used to transfer name of the DAC with the diagnostic.
		/// </summary>
		public const string DacName = nameof(DacName);

		/// <summary>
		/// The property used to transfer the DAC metadata with the diagnostic.
		/// </summary>
		public const string DacMetadataName = nameof(DacMetadataName);
	}
}
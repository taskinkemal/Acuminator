﻿using Acuminator.Utilities.Common;
using System;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	/// <summary>
	/// The class holds information about suppression of reported Acuminator's diagnostic 
	/// </summary>
	public readonly struct SuppressMessage : IEquatable<SuppressMessage>
	{
		/// <summary>
		/// Suppressed diagnostic Id
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Description of a member declaration where suppressed diagnostic is located
		/// </summary>
		public string Target { get; }

		/// <summary>
		/// Syntax node of a suppressed diagnostic
		/// </summary>
		public string SyntaxNode { get; }

		public SuppressMessage(string id, string target, string syntaxNode)
		{
			id.ThrowOnNull(nameof(id));
			target.ThrowOnNull(nameof(target));
			syntaxNode.ThrowOnNull(nameof(syntaxNode));

			Id = id;
			Target = target;
			SyntaxNode = syntaxNode;
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Id.GetHashCode();
				hash = 23 * hash + Target.GetHashCode();
				hash = 23 * hash + SyntaxNode.GetHashCode();
			}

			return hash;
		}

		public bool Equals(SuppressMessage other)
		{
			if (!other.Id.Equals(Id, StringComparison.Ordinal))
			{
				return false;
			}

			if (!other.Target.Equals(Target, StringComparison.Ordinal))
			{
				return false;
			}

			if (!other.SyntaxNode.Equals(SyntaxNode, StringComparison.Ordinal))
			{
				return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SuppressMessage message))
			{
				return false;
			}

			return Equals(message);
		}
	}
}

﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;
using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacNodeViewModel : TreeNodeViewModel
	{
		public DacSemanticModel DacModel { get; }

		public override string Name
		{
			get => DacModel.Symbol.Name;
			protected set { }
		}

		public override Icon NodeIcon => DacModel.DacType == DacType.Dac
			? Icon.Dac
			: Icon.DacExtension;

		public override bool DisplayNodeWithoutChildren => true;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public DacNodeViewModel(DacSemanticModel dacModel, TreeViewModel tree, bool isExpanded) : 
						   base(tree, parent: null, isExpanded)
		{
			DacModel = dacModel.CheckIfNull();
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(GetDacExtraInfos());
		}

		private IEnumerable<ExtraInfoViewModel> GetDacExtraInfos()
		{
			if (DacModel.IsProjectionDac)
			{
				yield return new IconViewModel(this, Icon.ProjectionDac);
			}

			Color color = Color.FromRgb(38, 155, 199);
			string dacType = DacModel.DacType == DacType.Dac
				? VSIXResource.CodeMap_ExtraInfo_IsDac
				: VSIXResource.CodeMap_ExtraInfo_IsDacExtension;
			yield return new TextViewModel(this, dacType, darkThemeForeground: color, lightThemeForeground: color);
		}

		public override Task NavigateToItemAsync() => DacModel.Symbol.NavigateToAsync();

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class ViewNodeViewModel : GraphMemberNodeViewModel, IElementWithTooltip
	{
		public DataViewInfo ViewInfo => (MemberInfo as DataViewInfo)!;

		public override Icon NodeIcon { get; } 

		public override ExtendedObservableCollection<ExtraInfoViewModel>? ExtraInfos { get; }

		public ViewNodeViewModel(ViewCategoryNodeViewModel viewCategoryVM, DataViewInfo viewInfo, bool isExpanded = false) :
							base(viewCategoryVM, viewCategoryVM, viewInfo, isExpanded)
		{
			NodeIcon = GetIcon();
			var infos = GetExtraInfos();

			if (infos.Any())
			{
				ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(infos);
			}
		}

		private Icon GetIcon()
		{
			if (ViewInfo.IsFilter)
				return Icon.Filter;
			else if (ViewInfo.IsSetup)
				return Icon.Settings;
			else if (ViewInfo.IsProcessing)
				return Icon.Processing;
			else
				return Icon.View;
		}

		private IEnumerable<ExtraInfoViewModel> GetExtraInfos()
		{
			if (ViewInfo.IsFilter && NodeIcon != Icon.Filter)
			{
				yield return new IconViewModel(this, Icon.Filter);
			}

			if (ViewInfo.IsSetup && NodeIcon != Icon.Settings)
			{
				yield return new IconViewModel(this, Icon.Settings);
			}

			if (ViewInfo.IsProcessing && NodeIcon != Icon.Processing)
			{
				yield return new IconViewModel(this, Icon.Processing);
			}

			if (ViewInfo.IsCustomView)
			{
				yield return new TextViewModel(this, VSIXResource.CodeMap_ExtraInfo_CustomViewLabel)
				{
					Tooltip = VSIXResource.CodeMap_ExtraInfo_CustomViewTooltip
				};
			}

			if (ViewInfo.IsPXSelectReadOnly)
			{
				yield return new TextViewModel(this, VSIXResource.CodeMap_ExtraInfo_PXSelectReadOnlyViewLabel)
				{
					Tooltip = VSIXResource.CodeMap_ExtraInfo_PXSelectReadOnlyViewTooltip
				};
			}
		}

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			string? tooltip = GetTooltip();
			return tooltip.IsNullOrWhiteSpace()
				? null
				: new TooltipInfo(tooltip) { TrimExcess = true };
		}

		private string? GetTooltip()
		{
			if (ViewInfo.Symbol?.Locations.Length != 1 || ViewInfo.Symbol.Locations[0].IsInMetadata || Tree.CodeMapViewModel.Workspace == null)
			{
				return ViewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}

			int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
			SyntaxNode? viewSyntaxNode = ViewInfo.Symbol.GetSyntax(Tree.CodeMapViewModel.CancellationToken ?? default);

			switch (viewSyntaxNode)
			{
				case PropertyDeclarationSyntax propertyDeclaration:
					{
						int prependLength = IndentUtils.GetPrependLength(propertyDeclaration.Modifiers);
						return propertyDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize, prependLength);
					}
				case VariableDeclarationSyntax variableDeclaration:
					{
						int prependLength = IndentUtils.GetPrependLength((variableDeclaration.Parent as FieldDeclarationSyntax)?.Modifiers);
						return variableDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize, prependLength);
					}
				case VariableDeclaratorSyntax variableDeclarator
				when variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration:
					{
						int prependLength = IndentUtils.GetPrependLength((variableDeclaration.Parent as FieldDeclarationSyntax)?.Modifiers);
						return variableDeclaration.Type.GetSyntaxNodeStringWithRemovedIndent(tabSize, prependLength);
					}
				default:
					return ViewInfo.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

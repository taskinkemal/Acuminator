﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acuminator.Utilities;
using Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix
{
	public class GeneralOptionsPage : DialogPage
	{
		private const string AllSettings = "All";
		private const string ColoringCategoryName = "BQL Coloring";
		private const string OutliningCategoryName = "BQL Outlining";
		private const string CodeAnalysisCategoryName = "Code Analysis";

		private bool colorSettingsChanged;
		public event EventHandler<SettingChangedEventArgs> ColoringSettingChanged;
		public const string PageTitle = "General";

		private bool coloringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_ColoringEnabled_Description))]
		public bool ColoringEnabled
		{
			get => coloringEnabled;
			set
			{
				if (coloringEnabled != value)
				{
					coloringEnabled = value;
					colorSettingsChanged = true;
				}
			}
		}

		private bool pxActionColoringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_PXActionColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_PXActionColoringEnabled_Description))]
		public bool PXActionColoringEnabled
		{
			get => pxActionColoringEnabled;
			set
			{
				if (pxActionColoringEnabled != value)
				{
					pxActionColoringEnabled = value;
					colorSettingsChanged = true;
				}
			}
		}

		private bool pxGraphColoringEnabled = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_PXGraphColoringEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_PXGraphColoringEnabled_Description))]
		public bool PXGraphColoringEnabled
		{
			get => pxGraphColoringEnabled;
			set
			{
				if (pxGraphColoringEnabled != value)
				{
					pxGraphColoringEnabled = value;
					colorSettingsChanged = true;
				}
			}
		}

		private bool colorOnlyInsideBQL;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_ColorOnlyInsideBQL_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_ColorOnlyInsideBQL_Description))]
		public bool ColorOnlyInsideBQL
		{
			get => colorOnlyInsideBQL;
			set
			{
				if (colorOnlyInsideBQL != value)
				{
					colorOnlyInsideBQL = value;
					colorSettingsChanged = true;
				}
			}
		}

		private bool useRegexColoring;

		[CategoryFromResources(nameof(VSIXResource.Category_Coloring), ColoringCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseRegexColoring_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseRegexColoring_Description))]
		public bool UseRegexColoring
		{
			get => useRegexColoring;
			set
			{
				if (useRegexColoring != value)
				{
					useRegexColoring = value;
					colorSettingsChanged = true;
				}
			}
		}

		private bool useBqlOutlining = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Outlining), OutliningCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlOutlining_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlOutlining_Description))]
		public bool UseBqlOutlining
		{
			get => useBqlOutlining;
			set
			{
				if (useBqlOutlining != value)
				{
					useBqlOutlining = value;
					colorSettingsChanged = true;
				}
			}
		}

		private bool useBqlDetailedOutlining = true;

		[CategoryFromResources(nameof(VSIXResource.Category_Outlining), OutliningCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlDetailedOutlining_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_UseBqlDetailedOutlining_Description))]
		public bool UseBqlDetailedOutlining
		{
			get => useBqlDetailedOutlining;
			set
			{
				if (useBqlDetailedOutlining != value)
				{
					useBqlDetailedOutlining = value;
					colorSettingsChanged = true;
				}
			}
		}

		[CategoryFromResources(nameof(VSIXResource.Category_CodeAnalysis), CodeAnalysisCategoryName)]
		[DisplayNameFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_RecursiveAnalysisEnabled_Title))]
		[DescriptionFromResources(resourceKey: nameof(VSIXResource.Setting_CodeAnalysis_RecursiveAnalysisEnabled_Description))]
		public bool RecursiveAnalysisEnabled { get; set; }

		public override void ResetSettings()
		{
			coloringEnabled = true;
			useRegexColoring = false;
			useBqlOutlining = true;
			useBqlDetailedOutlining = true;
			pxActionColoringEnabled = true;
			pxGraphColoringEnabled = true;
			colorOnlyInsideBQL = false;

			RecursiveAnalysisEnabled = CodeAnalysisSettings.Default.RecursiveAnalysisEnabled;

			colorSettingsChanged = false;
			base.ResetSettings();
			OnSettingsChanged(AllSettings);
		}

		public override void SaveSettingsToStorage()
		{
			base.SaveSettingsToStorage();

			if (colorSettingsChanged)
			{
				colorSettingsChanged = false;
				OnSettingsChanged(AllSettings);
			}
		}

		private void OnSettingsChanged(string setting)
		{
			ColoringSettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
		}
	}
}
